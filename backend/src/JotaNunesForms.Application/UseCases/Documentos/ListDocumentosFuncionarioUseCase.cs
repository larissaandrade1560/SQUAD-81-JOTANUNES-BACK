using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class ListDocumentosFuncionarioUseCase
{
    private readonly IDocumentoFuncionarioRepository _documentos;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;
    private readonly AccessScopeGuard _scopeGuard;

    public ListDocumentosFuncionarioUseCase(
        IDocumentoFuncionarioRepository documentos,
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas,
        AccessScopeGuard scopeGuard)
    {
        _documentos = documentos;
        _funcionarios = funcionarios;
        _empresas = empresas;
        _scopeGuard = scopeGuard;
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

        var list = await _documentos.ListByFuncionarioAsync(funcionarioId, cancellationToken);
        var utcNow = DateTime.UtcNow;

        foreach (var documento in list)
        {
            var statusAnterior = documento.Status;
            documento.AtualizarVencimentoSeExpirado(utcNow);
            if (documento.Status != statusAnterior)
            {
                await _documentos.UpdateAsync(documento, cancellationToken);
            }
        }

        return list
            .Select(d => DocumentoFuncionarioResponse.FromEntity(d, funcionario, empresaNome))
            .ToList();
    }
}
