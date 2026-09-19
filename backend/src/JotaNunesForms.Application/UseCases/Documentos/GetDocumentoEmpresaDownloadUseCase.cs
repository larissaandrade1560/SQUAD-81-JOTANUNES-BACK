using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class GetDocumentoEmpresaDownloadUseCase
{
    private readonly IDocumentoEmpresaRepository _documentos;
    private readonly IEmpresaRepository _empresas;
    private readonly IObjectStorage _storage;

    public GetDocumentoEmpresaDownloadUseCase(
        IDocumentoEmpresaRepository documentos,
        IEmpresaRepository empresas,
        IObjectStorage storage)
    {
        _documentos = documentos;
        _empresas = empresas;
        _storage = storage;
    }

    public async Task<DocumentoDownloadResponse> ExecuteAsync(
        Guid id,
        Guid? scopeEmpresaId,
        CancellationToken cancellationToken = default)
    {
        if (!_storage.IsConfigured)
        {
            throw new DocumentoEmpresaException("Armazenamento de documentos não configurado (R2).");
        }

        var documento = await _documentos.GetByIdAsync(id, cancellationToken);
        if (documento is null)
        {
            throw new DocumentoEmpresaException("Documento não encontrado.");
        }

        if (scopeEmpresaId is not null && documento.EmpresaId != scopeEmpresaId.Value)
        {
            throw new DocumentoEmpresaException("Sem permissão para acessar este documento.");
        }

        var empresa = await _empresas.GetByIdAsync(documento.EmpresaId, cancellationToken);
        if (empresa is null)
        {
            throw new DocumentoEmpresaException("Empresa não encontrada.");
        }

        var validFor = TimeSpan.FromMinutes(15);
        var url = await _storage.GetDownloadUrlAsync(documento.StorageKey, validFor, cancellationToken);
        return new DocumentoDownloadResponse(url, DateTime.UtcNow.Add(validFor));
    }
}
