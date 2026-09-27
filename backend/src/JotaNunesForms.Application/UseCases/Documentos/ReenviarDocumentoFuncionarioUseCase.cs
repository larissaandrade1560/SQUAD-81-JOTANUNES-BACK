using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.UseCases.Auditoria;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.Logging;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class ReenviarDocumentoFuncionarioUseCase
{
    private const long MaxBytes = 10 * 1024 * 1024;

    private readonly IDocumentoFuncionarioRepository _documentos;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;
    private readonly IObjectStorage _storage;
    private readonly AccessScopeGuard _scopeGuard;
    private readonly IDocumentoArquivoVersaoRepository _versoes;
    private readonly ITransactionalExecutor _transactions;
    private readonly AuditoriaDocumentoService _auditoria;
    private readonly ILogger<ReenviarDocumentoFuncionarioUseCase> _logger;

    public ReenviarDocumentoFuncionarioUseCase(
        IDocumentoFuncionarioRepository documentos,
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas,
        IObjectStorage storage,
        AccessScopeGuard scopeGuard,
        IDocumentoArquivoVersaoRepository versoes,
        ITransactionalExecutor transactions,
        AuditoriaDocumentoService auditoria,
        ILogger<ReenviarDocumentoFuncionarioUseCase> logger)
    {
        _documentos = documentos;
        _funcionarios = funcionarios;
        _empresas = empresas;
        _storage = storage;
        _scopeGuard = scopeGuard;
        _versoes = versoes;
        _transactions = transactions;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<DocumentoFuncionarioResponse> ExecuteAsync(
        Guid id,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        string nomeArquivo,
        string contentType,
        long tamanhoBytes,
        Stream conteudo,
        CancellationToken cancellationToken = default)
    {
        if (scope is not AccessScope.Company companyScope || companyScope.Type != TipoEmpresa.MaoDeObra)
        {
            throw new DocumentoFuncionarioException("Acesso não permitido.", 403);
        }

        var documento = await _documentos.GetByIdAsync(id, cancellationToken);
        if (documento is null)
        {
            throw new DocumentoFuncionarioException("Recurso não encontrado.", 404);
        }

        var funcionario = await _funcionarios.GetByIdAsync(documento.FuncionarioId, cancellationToken);
        if (funcionario is null)
        {
            throw new DocumentoFuncionarioException("Recurso não encontrado.", 404);
        }

        if (!await _scopeGuard.AllowsCompanyAsync(scope, funcionario.EmpresaId, securityRequest, cancellationToken))
        {
            throw new DocumentoFuncionarioException("Recurso não encontrado.", 404);
        }

        if (tamanhoBytes <= 0 || tamanhoBytes > MaxBytes)
        {
            throw new DocumentoFuncionarioException("Arquivo deve ter no máximo 10 MB.");
        }

        if (!string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
            && !nomeArquivo.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new DocumentoFuncionarioException("Envie apenas arquivos PDF.");
        }

        var empresa = await _empresas.GetByIdAsync(funcionario.EmpresaId, cancellationToken);
        if (empresa is null)
        {
            throw new DocumentoFuncionarioException("Recurso não encontrado.", 404);
        }

        if (!_storage.IsConfigured)
        {
            throw new DocumentoFuncionarioException("Armazenamento de documentos não configurado (R2).", 503);
        }

        var versionId = Guid.NewGuid();
        var storageKey =
            $"empresas/{funcionario.EmpresaId:D}/funcionarios/{funcionario.Id:D}/documentos/{versionId:D}.pdf";
        var actorId = companyScope.UserId;
        var occurredAt = DateTime.UtcNow;
        var uploaded = false;

        try
        {
            await _storage.UploadAsync(storageKey, conteudo, "application/pdf", cancellationToken);
            uploaded = true;
            var updated = await _transactions.ExecuteAsync(async transactionToken =>
            {
                var current = await _documentos.GetByIdForUpdateAsync(id, transactionToken)
                    ?? throw new DocumentoFuncionarioException("Recurso não encontrado.", 404);
                var previous = await _versoes.GetCurrentForFuncionarioAsync(id, transactionToken)
                    ?? throw new DocumentoFuncionarioException("Versão vigente não encontrada.", 409);
                var number = await _versoes.GetNextNumberForFuncionarioAsync(id, transactionToken);

                try
                {
                    current.Reenviar(nomeArquivo, storageKey, "application/pdf", tamanhoBytes);
                }
                catch (InvalidOperationException)
                {
                    throw new DocumentoFuncionarioException(AuditoriaConflictException.StableCode, 409);
                }

                previous.MarkNotCurrent();
                var next = DocumentoArquivoVersao.ForFuncionario(
                    id,
                    number,
                    nomeArquivo,
                    storageKey,
                    "application/pdf",
                    tamanhoBytes,
                    actorId,
                    occurredAt,
                    versionId);
                await _versoes.UpdateAsync(previous, transactionToken);
                await _documentos.UpdateAsync(current, transactionToken);
                await _versoes.AddAsync(next, transactionToken);
                await _auditoria.RegisterAsync(new RegistrarEventoAuditoriaDocumento(
                    CodigoAuditoriaDocumento.DocumentoReenviado,
                    actorId,
                    OrigemDocumentoAuditoria.DocumentoFuncionario,
                    id,
                    next.Id,
                    next.Numero,
                    previous.Id,
                    previous.Numero,
                    OcorreuEm: occurredAt), transactionToken);
                return current;
            }, cancellationToken);
            return DocumentoFuncionarioResponse.FromEntity(updated, funcionario, empresa.RazaoSocial);
        }
        catch (Exception failure)
        {
            if (uploaded)
            {
                await TryCompensateAsync(storageKey);
            }

            if (failure is OperationCanceledException or DocumentoFuncionarioException)
            {
                throw;
            }

            if (failure is AuditoriaConflictException)
            {
                throw new DocumentoFuncionarioException(AuditoriaConflictException.StableCode, 409);
            }

            throw new DocumentoFuncionarioException("Não foi possível concluir o reenvio do documento.", 500);
        }
    }

    private async Task TryCompensateAsync(string storageKey)
    {
        try
        {
            await _storage.DeleteAsync(storageKey, CancellationToken.None);
        }
        catch (Exception failure)
        {
            _logger.LogWarning(
                "Compensating storage cleanup failed. FailureType={FailureType}",
                failure.GetType().Name);
        }
    }
}
