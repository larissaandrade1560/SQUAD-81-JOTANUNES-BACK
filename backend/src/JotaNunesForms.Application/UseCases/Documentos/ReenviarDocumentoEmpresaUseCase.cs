using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.UseCases.Auditoria;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.Logging;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class ReenviarDocumentoEmpresaUseCase
{
    private const long MaxBytes = 10 * 1024 * 1024;

    private readonly IDocumentoEmpresaRepository _documentos;
    private readonly IEmpresaRepository _empresas;
    private readonly IObjectStorage _storage;
    private readonly AccessScopeGuard _scopeGuard;
    private readonly IDocumentoArquivoVersaoRepository _versoes;
    private readonly ITransactionalExecutor _transactions;
    private readonly AuditoriaDocumentoService _auditoria;
    private readonly ILogger<ReenviarDocumentoEmpresaUseCase> _logger;

    public ReenviarDocumentoEmpresaUseCase(
        IDocumentoEmpresaRepository documentos,
        IEmpresaRepository empresas,
        IObjectStorage storage,
        AccessScopeGuard scopeGuard,
        IDocumentoArquivoVersaoRepository versoes,
        ITransactionalExecutor transactions,
        AuditoriaDocumentoService auditoria,
        ILogger<ReenviarDocumentoEmpresaUseCase> logger)
    {
        _documentos = documentos;
        _empresas = empresas;
        _storage = storage;
        _scopeGuard = scopeGuard;
        _versoes = versoes;
        _transactions = transactions;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<DocumentoEmpresaResponse> ExecuteAsync(
        Guid id,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        string nomeArquivo,
        string contentType,
        long tamanhoBytes,
        Stream conteudo,
        CancellationToken cancellationToken = default)
    {
        var documento = await _documentos.GetByIdAsync(id, cancellationToken);
        if (documento is null)
        {
            throw new DocumentoEmpresaException("Recurso não encontrado.", 404);
        }

        if (!await _scopeGuard.AllowsCompanyAsync(scope, documento.EmpresaId, securityRequest, cancellationToken))
        {
            throw new DocumentoEmpresaException("Recurso não encontrado.", 404);
        }

        if (tamanhoBytes <= 0 || tamanhoBytes > MaxBytes)
        {
            throw new DocumentoEmpresaException("Arquivo deve ter no máximo 10 MB.");
        }

        if (!string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
            && !nomeArquivo.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new DocumentoEmpresaException("Envie apenas arquivos PDF.");
        }

        var empresa = await _empresas.GetByIdAsync(documento.EmpresaId, cancellationToken);
        if (empresa is null)
        {
            throw new DocumentoEmpresaException("Recurso não encontrado.", 404);
        }

        if (scope is not AccessScope.Company)
        {
            throw new DocumentoEmpresaException("Acesso não permitido.", 403);
        }

        if (!_storage.IsConfigured)
        {
            throw new DocumentoEmpresaException("Armazenamento de documentos não configurado (R2).", 503);
        }

        var versionId = Guid.NewGuid();
        var storageKey = $"empresas/{documento.EmpresaId:D}/documentos/{versionId:D}.pdf";
        var actorId = scope is AccessScope.Company companyScope
            ? companyScope.UserId
            : throw new DocumentoEmpresaException("Acesso não permitido.", 403);
        var occurredAt = DateTime.UtcNow;
        var uploaded = false;

        try
        {
            await _storage.UploadAsync(storageKey, conteudo, "application/pdf", cancellationToken);
            uploaded = true;
            var updated = await _transactions.ExecuteAsync(async transactionToken =>
            {
                var current = await _documentos.GetByIdForUpdateAsync(id, transactionToken)
                    ?? throw new DocumentoEmpresaException("Recurso não encontrado.", 404);
                var previous = await _versoes.GetCurrentForEmpresaAsync(id, transactionToken)
                    ?? throw new DocumentoEmpresaException("Versão vigente não encontrada.", 409);
                var number = await _versoes.GetNextNumberForEmpresaAsync(id, transactionToken);

                try
                {
                    current.Reenviar(nomeArquivo, storageKey, "application/pdf", tamanhoBytes);
                }
                catch (InvalidOperationException)
                {
                    throw new DocumentoEmpresaException(AuditoriaConflictException.StableCode, 409);
                }

                previous.MarkNotCurrent();
                var next = DocumentoArquivoVersao.ForEmpresa(
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
                    OrigemDocumentoAuditoria.DocumentoEmpresa,
                    id,
                    next.Id,
                    next.Numero,
                    previous.Id,
                    previous.Numero,
                    OcorreuEm: occurredAt), transactionToken);
                return current;
            }, cancellationToken);
            return DocumentoEmpresaResponse.FromEntity(updated, empresa.RazaoSocial);
        }
        catch (Exception failure)
        {
            if (uploaded)
            {
                await TryCompensateAsync(storageKey);
            }

            if (failure is OperationCanceledException or DocumentoEmpresaException)
            {
                throw;
            }

            if (failure is AuditoriaConflictException)
            {
                throw new DocumentoEmpresaException(AuditoriaConflictException.StableCode, 409);
            }

            throw new DocumentoEmpresaException("Não foi possível concluir o reenvio do documento.", 500);
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
