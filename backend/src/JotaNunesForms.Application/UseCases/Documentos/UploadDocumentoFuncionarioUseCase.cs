using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.UseCases.Auditoria;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.Logging;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class UploadDocumentoFuncionarioUseCase
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
    private readonly ILogger<UploadDocumentoFuncionarioUseCase> _logger;

    public UploadDocumentoFuncionarioUseCase(
        IDocumentoFuncionarioRepository documentos,
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas,
        IObjectStorage storage,
        AccessScopeGuard scopeGuard,
        IDocumentoArquivoVersaoRepository versoes,
        ITransactionalExecutor transactions,
        AuditoriaDocumentoService auditoria,
        ILogger<UploadDocumentoFuncionarioUseCase> logger)
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
        Guid funcionarioId,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        TipoDocumentoFuncionario tipo,
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

        var funcionario = await _funcionarios.GetByIdAsync(funcionarioId, cancellationToken);
        if (funcionario is null)
        {
            throw new DocumentoFuncionarioException("Recurso não encontrado.", 404);
        }

        if (!await _scopeGuard.AllowsCompanyAsync(scope, funcionario.EmpresaId, securityRequest, cancellationToken))
        {
            throw new DocumentoFuncionarioException("Recurso não encontrado.", 404);
        }

        var empresa = await _empresas.GetByIdAsync(funcionario.EmpresaId, cancellationToken);
        if (empresa is null)
        {
            throw new DocumentoFuncionarioException("Recurso não encontrado.", 404);
        }

        if (empresa.Tipo != TipoEmpresa.MaoDeObra)
        {
            throw new DocumentoFuncionarioException("Upload disponível apenas para empresas de Mão de Obra.");
        }

        if (!_storage.IsConfigured)
        {
            throw new DocumentoFuncionarioException("Armazenamento de documentos não configurado (R2).", 503);
        }

        if (!Enum.IsDefined(typeof(TipoDocumentoFuncionario), tipo))
        {
            throw new DocumentoFuncionarioException("Tipo de documento inválido.");
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

        var documentoId = Guid.NewGuid();
        var storageKey =
            $"empresas/{funcionario.EmpresaId:D}/funcionarios/{funcionarioId:D}/documentos/{documentoId:D}.pdf";

        var documento = new DocumentoFuncionario(
            documentoId,
            funcionarioId,
            tipo,
            nomeArquivo,
            storageKey,
            "application/pdf",
            tamanhoBytes);

        var actorId = companyScope.UserId;
        var occurredAt = DateTime.UtcNow;
        var version = DocumentoArquivoVersao.ForFuncionario(
            documento.Id,
            1,
            nomeArquivo,
            storageKey,
            "application/pdf",
            tamanhoBytes,
            actorId,
            occurredAt);
        var uploaded = false;

        try
        {
            await _storage.UploadAsync(storageKey, conteudo, "application/pdf", cancellationToken);
            uploaded = true;
            await _transactions.ExecuteAsync(async transactionToken =>
            {
                await _documentos.AddAsync(documento, transactionToken);
                await _versoes.AddAsync(version, transactionToken);
                await _auditoria.RegisterAsync(new RegistrarEventoAuditoriaDocumento(
                    CodigoAuditoriaDocumento.DocumentoEnviado,
                    actorId,
                    OrigemDocumentoAuditoria.DocumentoFuncionario,
                    documento.Id,
                    version.Id,
                    version.Numero,
                    OcorreuEm: occurredAt), transactionToken);
                return true;
            }, cancellationToken);
        }
        catch (Exception failure)
        {
            if (uploaded)
            {
                await TryCompensateAsync(storageKey);
            }

            if (failure is OperationCanceledException)
            {
                throw;
            }

            if (failure is DocumentoFuncionarioException)
            {
                throw;
            }

            throw new DocumentoFuncionarioException("Não foi possível concluir o registro do documento.", 500);
        }

        return DocumentoFuncionarioResponse.FromEntity(documento, funcionario, empresa.RazaoSocial);
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
