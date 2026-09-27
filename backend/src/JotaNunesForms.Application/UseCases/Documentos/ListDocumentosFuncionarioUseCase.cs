using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.UseCases.Auditoria;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class ListDocumentosFuncionarioUseCase
{
    private readonly IDocumentoFuncionarioRepository _documentos;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;
    private readonly AccessScopeGuard _scopeGuard;
    private readonly RegistrarVencimentoDocumentoUseCase _vencimento;

    public ListDocumentosFuncionarioUseCase(
        IDocumentoFuncionarioRepository documentos,
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas,
        AccessScopeGuard scopeGuard,
        RegistrarVencimentoDocumentoUseCase vencimento)
    {
        _documentos = documentos;
        _funcionarios = funcionarios;
        _empresas = empresas;
        _scopeGuard = scopeGuard;
        _vencimento = vencimento;
    }

    public async Task<IReadOnlyList<DocumentoFuncionarioResponse>> ExecuteAsync(
        Guid funcionarioId,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        CancellationToken cancellationToken = default)
    {
        var scopeEmpresaId = scope switch
        {
            AccessScope.Internal => (Guid?)null,
            AccessScope.Company company when company.Type == TipoEmpresa.MaoDeObra => company.CompanyId,
            _ => throw new DocumentoFuncionarioException("Acesso não permitido.", 403),
        };

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
        var empresaNome = empresa?.RazaoSocial ?? "—";

        var list = (await _documentos.ListByFuncionarioAsync(funcionarioId, cancellationToken)).ToList();
        var utcNow = DateTime.UtcNow;

        for (var index = 0; index < list.Count; index++)
        {
            var documento = list[index];
            if (documento.Status == StatusDocumento.Aprovado
                && documento.ValidoAte is DateTime validoAte
                && validoAte <= utcNow)
            {
                list[index] = await _vencimento.ExpireEmployeeIfDueAsync(documento.Id, utcNow, cancellationToken);
            }
        }

        return list
            .Select(d => DocumentoFuncionarioResponse.FromEntity(d, funcionario, empresaNome))
            .ToList();
    }
}
