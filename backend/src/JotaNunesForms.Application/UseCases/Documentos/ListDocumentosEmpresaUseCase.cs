using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.UseCases.Auditoria;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class ListDocumentosEmpresaUseCase
{
    private readonly IDocumentoEmpresaRepository _documentos;
    private readonly IEmpresaRepository _empresas;
    private readonly RegistrarVencimentoDocumentoUseCase _vencimento;

    public ListDocumentosEmpresaUseCase(
        IDocumentoEmpresaRepository documentos,
        IEmpresaRepository empresas,
        RegistrarVencimentoDocumentoUseCase vencimento)
    {
        _documentos = documentos;
        _empresas = empresas;
        _vencimento = vencimento;
    }

    public async Task<IReadOnlyList<DocumentoEmpresaResponse>> ExecuteAsync(
        AccessScope scope,
        CancellationToken cancellationToken = default)
    {
        var scopeEmpresaId = scope is AccessScope.Company company ? company.CompanyId : (Guid?)null;
        var list = (await _documentos.ListAsync(scopeEmpresaId, cancellationToken)).ToList();
        var utcNow = DateTime.UtcNow;

        for (var index = 0; index < list.Count; index++)
        {
            var documento = list[index];
            if (documento.Status == StatusDocumento.Aprovado
                && documento.ValidoAte is DateTime validoAte
                && validoAte <= utcNow)
            {
                list[index] = await _vencimento.ExpireCompanyIfDueAsync(documento.Id, utcNow, cancellationToken);
            }
        }

        var empresas = await _empresas.ListAsync(cancellationToken);
        var nomes = empresas.ToDictionary(e => e.Id, e => e.RazaoSocial);

        return list
            .Select(d => DocumentoEmpresaResponse.FromEntity(
                d,
                nomes.GetValueOrDefault(d.EmpresaId, "—")))
            .ToList();
    }
}
