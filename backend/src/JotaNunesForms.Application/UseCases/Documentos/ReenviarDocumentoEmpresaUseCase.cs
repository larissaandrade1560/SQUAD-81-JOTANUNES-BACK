using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class ReenviarDocumentoEmpresaUseCase
{
    private const long MaxBytes = 10 * 1024 * 1024;

    private readonly IDocumentoEmpresaRepository _documentos;
    private readonly IEmpresaRepository _empresas;
    private readonly IObjectStorage _storage;
    private readonly AccessScopeGuard _scopeGuard;

    public ReenviarDocumentoEmpresaUseCase(
        IDocumentoEmpresaRepository documentos,
        IEmpresaRepository empresas,
        IObjectStorage storage,
        AccessScopeGuard scopeGuard)
    {
        _documentos = documentos;
        _empresas = empresas;
        _storage = storage;
        _scopeGuard = scopeGuard;
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

        var novoArquivoId = Guid.NewGuid();
        var storageKey = $"empresas/{documento.EmpresaId:D}/documentos/{novoArquivoId:D}.pdf";

        await _storage.UploadAsync(
            storageKey,
            conteudo,
            "application/pdf",
            cancellationToken);

        try
        {
            documento.Reenviar(nomeArquivo, storageKey, "application/pdf", tamanhoBytes);
        }
        catch (InvalidOperationException ex)
        {
            throw new DocumentoEmpresaException(ex.Message);
        }

        await _documentos.UpdateAsync(documento, cancellationToken);
        return DocumentoEmpresaResponse.FromEntity(documento, empresa.RazaoSocial);
    }
}
