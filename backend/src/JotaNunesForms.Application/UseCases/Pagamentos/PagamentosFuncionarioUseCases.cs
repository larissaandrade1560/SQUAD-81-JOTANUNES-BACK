using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Pagamentos;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Pagamentos;

public sealed class ListPagamentosFuncionarioUseCase
{
    private readonly IPagamentoFuncionarioRepository _pagamentos;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;

    public ListPagamentosFuncionarioUseCase(
        IPagamentoFuncionarioRepository pagamentos,
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas)
    {
        _pagamentos = pagamentos;
        _funcionarios = funcionarios;
        _empresas = empresas;
    }

    public async Task<IReadOnlyList<PagamentoFuncionarioResponse>> ExecuteAsync(
        Guid? scopeEmpresaId,
        CancellationToken cancellationToken = default)
    {
        var list = await _pagamentos.ListAsync(scopeEmpresaId, cancellationToken);
        var funcionarios = await _funcionarios.ListAsync(scopeEmpresaId, cancellationToken);
        var nomesFunc = funcionarios.ToDictionary(f => f.Id, f => f.Nome);
        var empresas = await _empresas.ListAsync(cancellationToken);
        var nomesEmpresa = empresas.ToDictionary(e => e.Id, e => e.RazaoSocial);
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        return list
            .Select(p =>
            {
                var situacao = p.ObterSituacaoComprovante(hoje);
                return new PagamentoFuncionarioResponse(
                    p.Id,
                    p.FuncionarioId,
                    nomesFunc.GetValueOrDefault(p.FuncionarioId, "—"),
                    p.EmpresaId,
                    nomesEmpresa.GetValueOrDefault(p.EmpresaId, "—"),
                    p.Competencia,
                    p.DataPagamento,
                    p.PrazoComprovante,
                    p.ComprovanteEnviadoEm,
                    (int)situacao,
                    RotuloSituacao(situacao));
            })
            .ToList();
    }

    internal static string RotuloSituacao(SituacaoComprovante situacao) =>
        situacao switch
        {
            SituacaoComprovante.Pendente => "Pendente",
            SituacaoComprovante.EmAtraso => "Em atraso",
            SituacaoComprovante.NoPrazo => "No prazo",
            SituacaoComprovante.EnviadoEmAtraso => "Enviado em atraso",
            _ => situacao.ToString(),
        };
}

public sealed class RegistrarPagamentoFuncionarioUseCase
{
    private readonly IPagamentoFuncionarioRepository _pagamentos;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;

    public RegistrarPagamentoFuncionarioUseCase(
        IPagamentoFuncionarioRepository pagamentos,
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas)
    {
        _pagamentos = pagamentos;
        _funcionarios = funcionarios;
        _empresas = empresas;
    }

    public async Task<PagamentoFuncionarioResponse> ExecuteAsync(
        Guid empresaId,
        RegistrarPagamentoRequest request,
        CancellationToken cancellationToken = default)
    {
        var competencia = NormalizeCompetencia(request.Competencia);

        var funcionario = await _funcionarios.GetByIdAsync(request.FuncionarioId, cancellationToken);
        if (funcionario is null || funcionario.EmpresaId != empresaId)
        {
            throw new PagamentoException("Funcionário não encontrado.");
        }

        if (!funcionario.Ativo)
        {
            throw new PagamentoException("Funcionário inativo.");
        }

        if (await _pagamentos.ExistsCompetenciaAsync(funcionario.Id, competencia, cancellationToken))
        {
            throw new PagamentoException(
                "Já existe pagamento registrado para este funcionário nesta competência.");
        }

        var pagamento = new PagamentoFuncionario(
            funcionario.Id,
            empresaId,
            competencia,
            request.DataPagamento);

        await _pagamentos.AddAsync(pagamento, cancellationToken);

        var empresas = await _empresas.ListAsync(cancellationToken);
        var razao = empresas.FirstOrDefault(e => e.Id == empresaId)?.RazaoSocial ?? "—";
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var situacao = pagamento.ObterSituacaoComprovante(hoje);

        return new PagamentoFuncionarioResponse(
            pagamento.Id,
            pagamento.FuncionarioId,
            funcionario.Nome,
            pagamento.EmpresaId,
            razao,
            pagamento.Competencia,
            pagamento.DataPagamento,
            pagamento.PrazoComprovante,
            pagamento.ComprovanteEnviadoEm,
            (int)situacao,
            ListPagamentosFuncionarioUseCase.RotuloSituacao(situacao));
    }

    private static DateOnly NormalizeCompetencia(DateOnly competencia)
    {
        if (competencia.Day != 1)
        {
            return new DateOnly(competencia.Year, competencia.Month, 1);
        }

        return competencia;
    }
}
