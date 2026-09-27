using JotaNunesForms.Application.Auth;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.Mobilizacoes;

public sealed record MobilizacaoOwnership(
    Mobilizacao Mobilizacao,
    Funcionario Funcionario,
    Empresa Empresa,
    IReadOnlyList<ItemChecklist> ItensTrabalhador);

public sealed class MobilizacaoAccessService
{
    private readonly IMobilizacaoRepository _mobilizacoes;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;
    private readonly IItemChecklistRepository _itens;
    private readonly AccessScopeGuard _scopeGuard;

    public MobilizacaoAccessService(
        IMobilizacaoRepository mobilizacoes,
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas,
        IItemChecklistRepository itens,
        AccessScopeGuard scopeGuard)
    {
        _mobilizacoes = mobilizacoes;
        _funcionarios = funcionarios;
        _empresas = empresas;
        _itens = itens;
        _scopeGuard = scopeGuard;
    }

    public async Task<MobilizacaoOwnership> RequireReadableAsync(
        Guid mobilizacaoId,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        CancellationToken cancellationToken = default)
    {
        if (scope is AccessScope.Company companyScope && companyScope.Type != TipoEmpresa.MaoDeObra)
        {
            throw new MobilizacaoException("Acesso não permitido.", 403);
        }

        var mobilizacao = await _mobilizacoes.GetByIdAsync(mobilizacaoId, cancellationToken)
            ?? throw new MobilizacaoException("Recurso não encontrado.", 404);
        var funcionario = await _funcionarios.GetByIdAsync(mobilizacao.FuncionarioId, cancellationToken)
            ?? throw new MobilizacaoException("Recurso não encontrado.", 404);
        if (!await _scopeGuard.AllowsCompanyAsync(scope, funcionario.EmpresaId, securityRequest, cancellationToken))
        {
            throw new MobilizacaoException("Recurso não encontrado.", 404);
        }

        var empresa = await _empresas.GetByIdAsync(funcionario.EmpresaId, cancellationToken)
            ?? throw new MobilizacaoException("Recurso não encontrado.", 404);
        var itens = await _itens.ListActiveWorkerItemsByMobilizacaoAsync(
            mobilizacao.Id,
            mobilizacao.ProcessoId,
            cancellationToken);

        return new MobilizacaoOwnership(mobilizacao, funcionario, empresa, itens);
    }

    public static bool CadastroCompleto(Mobilizacao mobilizacao, Funcionario funcionario) =>
        !string.IsNullOrWhiteSpace(funcionario.Nome)
        && !string.IsNullOrWhiteSpace(funcionario.Cpf)
        && !string.IsNullOrWhiteSpace(mobilizacao.Funcao)
        && mobilizacao.ContratoId != Guid.Empty
        && mobilizacao.ObraId != Guid.Empty
        && mobilizacao.ProcessoId != Guid.Empty;
}
