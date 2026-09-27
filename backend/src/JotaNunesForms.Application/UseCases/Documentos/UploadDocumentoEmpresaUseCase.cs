using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.UseCases.Auditoria;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.Logging;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class UploadDocumentoEmpresaUseCase
{
    private const long MaxBytes = 10 * 1024 * 1024;

    private readonly IDocumentoEmpresaRepository _documentos;
    private readonly IEmpresaRepository _empresas;
    private readonly IObjectStorage _storage;
    private readonly IDocumentoArquivoVersaoRepository _versoes;
    private readonly ITransactionalExecutor _transactions;
    private readonly AuditoriaDocumentoService _auditoria;
    private readonly ILogger<UploadDocumentoEmpresaUseCase> _logger;

    public UploadDocumentoEmpresaUseCase(
        IDocumentoEmpresaRepository documentos,
        IEmpresaRepository empresas,
        IObjectStorage storage,
        IDocumentoArquivoVersaoRepository versoes,
        ITransactionalExecutor transactions,
        AuditoriaDocumentoService auditoria,
        ILogger<UploadDocumentoEmpresaUseCase> logger)
    {
        _documentos = documentos;
        _empresas = empresas;
        _storage = storage;
        _versoes = versoes;
        _transactions = transactions;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<DocumentoEmpresaResponse> ExecuteAsync(
        AccessScope scope,
        TipoDocumentoEmpresarial tipo,
        string nomeArquivo,
        string contentType,
        long tamanhoBytes,
        Stream conteudo,
        CancellationToken cancellationToken = default)
    {
        if (scope is not AccessScope.Company companyScope)
        {
            throw new DocumentoEmpresaException("Acesso não permitido.", 403);
        }

        var empresaId = companyScope.CompanyId;
        var empresa = await _empresas.GetByIdAsync(empresaId, cancellationToken);
        if (empresa is null)
        {
            throw new DocumentoEmpresaException("Recurso não encontrado.", 404);
        }

        if (!_storage.IsConfigured)
        {
            throw new DocumentoEmpresaException("Armazenamento de documentos não configurado (R2).", 503);
        }

        if (!Enum.IsDefined(typeof(TipoDocumentoEmpresarial), tipo))
        {
            throw new DocumentoEmpresaException("Tipo de documento inválido.");
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

        var documentoId = Guid.NewGuid();
        var storageKey = $"empresas/{empresaId:D}/documentos/{documentoId:D}.pdf";

        var documento = new DocumentoEmpresa(
            documentoId,
            empresaId,
            tipo,
            nomeArquivo,
            storageKey,
            "application/pdf",
            tamanhoBytes);

        var actorId = companyScope.UserId;
        var occurredAt = DateTime.UtcNow;
        var version = DocumentoArquivoVersao.ForEmpresa(
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
                    OrigemDocumentoAuditoria.DocumentoEmpresa,
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

            if (failure is DocumentoEmpresaException)
            {
                throw;
            }

            throw new DocumentoEmpresaException("Não foi possível concluir o registro do documento.", 500);
        }

        return DocumentoEmpresaResponse.FromEntity(documento, empresa.RazaoSocial);
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
