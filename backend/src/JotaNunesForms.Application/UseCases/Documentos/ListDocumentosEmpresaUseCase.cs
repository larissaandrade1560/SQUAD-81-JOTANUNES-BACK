using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class ListDocumentosEmpresaUseCase
{
    private readonly IDocumentoEmpresaRepository _documentos;
    private readonly IEmpresaRepository _empresas;

    public ListDocumentosEmpresaUseCase(
        IDocumentoEmpresaRepository documentos,
        IEmpresaRepository empresas)
    {
        _documentos = documentos;
        _empresas = empresas;
    }

    public async Task<IReadOnlyList<DocumentoEmpresaResponse>> ExecuteAsync(
        Guid? scopeEmpresaId,
        CancellationToken cancellationToken = default)
    {
        var list = await _documentos.ListAsync(scopeEmpresaId, cancellationToken);
        var empresas = await _empresas.ListAsync(cancellationToken);
        var nomes = empresas.ToDictionary(e => e.Id, e => e.RazaoSocial);

        return list
            .Select(d => DocumentoEmpresaResponse.FromEntity(
                d,
                nomes.GetValueOrDefault(d.EmpresaId, "—")))
            .ToList();
    }
}
