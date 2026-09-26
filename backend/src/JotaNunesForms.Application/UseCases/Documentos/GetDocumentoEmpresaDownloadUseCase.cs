using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class GetDocumentoEmpresaDownloadUseCase
{
    private readonly IDocumentoEmpresaRepository _documentos;
    private readonly IEmpresaRepository _empresas;
    private readonly IObjectStorage _storage;
    private readonly AccessScopeGuard _scopeGuard;

    public GetDocumentoEmpresaDownloadUseCase(
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

    public async Task<DocumentoDownloadResponse> ExecuteAsync(
        Guid id,
        AccessScope scope,
        SecurityRequestContext securityRequest,
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

        if (!_storage.IsConfigured)
        {
            throw new DocumentoEmpresaException("Armazenamento de documentos não configurado (R2).", 503);
        }

        var empresa = await _empresas.GetByIdAsync(documento.EmpresaId, cancellationToken);
        if (empresa is null)
        {
            throw new DocumentoEmpresaException("Recurso não encontrado.", 404);
        }

        var validFor = TimeSpan.FromMinutes(15);
        var url = await _storage.GetDownloadUrlAsync(documento.StorageKey, validFor, cancellationToken);
        return new DocumentoDownloadResponse(url, DateTime.UtcNow.Add(validFor));
    }
}
