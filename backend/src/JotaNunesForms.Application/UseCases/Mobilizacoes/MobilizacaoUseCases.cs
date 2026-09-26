using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Application.Mobilizacoes;
using JotaNunesForms.Application.Obras;
using JotaNunesForms.Application.Processos;
using JotaNunesForms.Application.UseCases.Processos;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Domain.Services;

namespace JotaNunesForms.Application.UseCases.Mobilizacoes;

public sealed class CreateMobilizacaoUseCase
{
    private readonly IEmpresaRepository _empresas;
    private readonly IObraRepository _obras;
    private readonly IContratoRepository _contratos;
    private readonly IProcessoContratacaoRepository _processos;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IFuncionarioObraRepository _vinculos;
    private readonly IMobilizacaoRepository _mobilizacoes;
    private readonly ICatalogoRequisitoRepository _catalogo;
    private readonly IItemChecklistRepository _itens;

    public CreateMobilizacaoUseCase(
        IEmpresaRepository empresas,
        IObraRepository obras,
        IContratoRepository contratos,
        IProcessoContratacaoRepository processos,
        IFuncionarioRepository funcionarios,
        IFuncionarioObraRepository vinculos,
        IMobilizacaoRepository mobilizacoes,
        ICatalogoRequisitoRepository catalogo,
        IItemChecklistRepository itens)
    {
        _empresas = empresas;
        _obras = obras;
        _contratos = contratos;
        _processos = processos;
        _funcionarios = funcionarios;
        _vinculos = vinculos;
        _mobilizacoes = mobilizacoes;
        _catalogo = catalogo;
        _itens = itens;
    }

    public async Task<MobilizacaoResponse> ExecuteAsync(
        CreateMobilizacaoRequest request,
        Guid? scopeEmpresaId,
        CancellationToken cancellationToken = default)
    {
        var empresaId = scopeEmpresaId ?? request.EmpresaId
            ?? throw new MobilizacaoException("Empresa é obrigatória.");
        var empresa = await _empresas.GetByIdAsync(empresaId, cancellationToken)
            ?? throw new EmpresaException("Empresa não encontrada.");

        if (empresa.Tipo == TipoEmpresa.Materiais)
        {
            throw new MobilizacaoException("Empresa de materiais não mobiliza trabalhadores.", 403);
        }

        var obra = await _obras.GetByIdAsync(request.ObraId, cancellationToken)
            ?? throw new ObraException("Obra não encontrada.");
        var contrato = await _contratos.GetByIdAsync(request.ContratoId, cancellationToken)
            ?? throw new MobilizacaoException("Contrato não encontrado.", 404);

        if (contrato.EmpresaId != empresa.Id || contrato.ObraId != obra.Id)
        {
            throw new MobilizacaoException("Contrato não pertence à empresa e obra informadas.");
        }

        ProcessoContratacao? processo = null;
        if (request.ProcessoId is { } processoId)
        {
            processo = await _processos.GetByIdAsync(processoId, cancellationToken);
        }
        else
        {
            processo = (await _processos.ListAsync(empresa.Id, cancellationToken))
                .FirstOrDefault(p => p.ContratoId == contrato.Id && p.MobilizaTrabalhadores);
        }

        if (processo is null)
        {
            throw new MobilizacaoException("Abra um processo com mobilização de trabalhadores para este contrato.");
        }

        if (!processo.MobilizaTrabalhadores || processo.ContratoId != contrato.Id)
        {
            throw new MobilizacaoException("O processo informado não autoriza mobilização neste contrato.");
        }

        string cpf;
        try
        {
            cpf = Funcionario.NormalizeCpf(request.Cpf);
        }
        catch (ArgumentException ex)
        {
            throw new MobilizacaoException(ex.Message);
        }

        var funcionario = await _funcionarios.GetByCpfInEmpresaAsync(empresa.Id, cpf, cancellationToken);
        if (funcionario is null)
        {
            try
            {
                funcionario = new Funcionario(empresa.Id, request.Nome, cpf, request.Funcao);
            }
            catch (ArgumentException ex)
            {
                throw new MobilizacaoException(ex.Message);
            }

            await _funcionarios.AddAsync(funcionario, cancellationToken);
        }
        else
        {
            funcionario.Atualizar(request.Nome, request.Funcao);
            await _funcionarios.UpdateAsync(funcionario, cancellationToken);
        }

        await _vinculos.EnsureVinculoAsync(funcionario.Id, obra.Id, cancellationToken);

        Mobilizacao mobilizacao;
        try
        {
            mobilizacao = new Mobilizacao(
                funcionario.Id,
                contrato.Id,
                obra.Id,
                processo.Id,
                request.Funcao,
                request.DataFimObra,
                request.TurnoJornada);
        }
        catch (ArgumentException ex)
        {
            throw new MobilizacaoException(ex.Message);
        }

        await _mobilizacoes.AddAsync(mobilizacao, cancellationToken);
        await _mobilizacoes.AddLotacaoAsync(
            new HistoricoLotacao(mobilizacao.Id, obra.Id, "Abertura da mobilização"),
            cancellationToken);

        var catalogo = await _catalogo.ListAsync(cancellationToken);
        var existentes = await _itens.ListByProcessoAsync(processo.Id, cancellationToken);
        var drafts = GeracaoChecklistAdmissional.Gerar(catalogo);
        var novos = drafts
            .Where(d => existentes.All(e =>
                !(e.CatalogoRequisitoId == d.CatalogoRequisitoId
                  && e.TitularTipo == TitularRequisito.Trabalhador
                  && e.TitularId == mobilizacao.Id)))
            .Select(d => new ItemChecklist(
                processo.Id,
                d.CatalogoRequisitoId,
                TitularRequisito.Trabalhador,
                d.Obrigatorio,
                titularId: mobilizacao.Id))
            .ToList();
        if (novos.Count > 0)
        {
            await _itens.AddRangeAsync(novos, cancellationToken);
        }

        return await MapAsync(mobilizacao, funcionario, empresa, catalogo, cancellationToken);
    }

    internal async Task<MobilizacaoResponse> MapAsync(
        Mobilizacao mobilizacao,
        Funcionario funcionario,
        Empresa empresa,
        IReadOnlyList<CatalogoRequisito> catalogo,
        CancellationToken cancellationToken)
    {
        var lotacoes = await _mobilizacoes.ListLotacoesAsync(mobilizacao.Id, cancellationToken);
        var itens = (await _itens.ListByProcessoAsync(mobilizacao.ProcessoId, cancellationToken))
            .Where(i => i.TitularTipo == TitularRequisito.Trabalhador && i.TitularId == mobilizacao.Id)
            .ToList();
        var processo = await _processos.GetByIdAsync(mobilizacao.ProcessoId, cancellationToken)
            ?? throw new ProcessoException("Processo da mobilização não encontrado.");
        var contrato = await _contratos.GetByIdAsync(mobilizacao.ContratoId, cancellationToken)
            ?? throw new MobilizacaoException("Contrato não encontrado.", 404);
        var mapped = ChecklistMapper.ToResponse(processo, contrato, catalogo, itens);
        return new MobilizacaoResponse(
            mobilizacao.Id,
            funcionario.Id,
            funcionario.Nome,
            funcionario.Cpf,
            empresa.Id,
            mobilizacao.ContratoId,
            mobilizacao.ObraId,
            mobilizacao.ProcessoId,
            mobilizacao.Funcao,
            mobilizacao.DataFimObra,
            mobilizacao.TurnoJornada,
            mobilizacao.Situacao,
            mobilizacao.CriadoEm,
            lotacoes.Select(HistoricoLotacaoResponse.FromEntity).ToList(),
            mapped.Checklist);
    }
}

public sealed class ListMobilizacoesUseCase
{
    private readonly IMobilizacaoRepository _mobilizacoes;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;
    private readonly ICatalogoRequisitoRepository _catalogo;
    private readonly CreateMobilizacaoUseCase _create;

    public ListMobilizacoesUseCase(
        IMobilizacaoRepository mobilizacoes,
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas,
        ICatalogoRequisitoRepository catalogo,
        CreateMobilizacaoUseCase create)
    {
        _mobilizacoes = mobilizacoes;
        _funcionarios = funcionarios;
        _empresas = empresas;
        _catalogo = catalogo;
        _create = create;
    }

    public async Task<IReadOnlyList<MobilizacaoResponse>> ExecuteAsync(
        Guid? empresaId,
        Guid? obraId,
        Guid? contratoId,
        SituacaoMobilizacao? situacao,
        CancellationToken cancellationToken = default)
    {
        var lista = await _mobilizacoes.ListAsync(empresaId, obraId, contratoId, situacao, cancellationToken);
        var catalogo = await _catalogo.ListAsync(cancellationToken);
        var resultado = new List<MobilizacaoResponse>();
        foreach (var mobilizacao in lista)
        {
            var funcionario = await _funcionarios.GetByIdAsync(mobilizacao.FuncionarioId, cancellationToken);
            if (funcionario is null)
            {
                continue;
            }

            var empresa = await _empresas.GetByIdAsync(funcionario.EmpresaId, cancellationToken);
            if (empresa is null)
            {
                continue;
            }

            resultado.Add(await _create.MapAsync(mobilizacao, funcionario, empresa, catalogo, cancellationToken));
        }

        return resultado;
    }
}

public sealed class GetMobilizacaoUseCase
{
    private readonly IMobilizacaoRepository _mobilizacoes;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;
    private readonly ICatalogoRequisitoRepository _catalogo;
    private readonly CreateMobilizacaoUseCase _create;

    public GetMobilizacaoUseCase(
        IMobilizacaoRepository mobilizacoes,
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas,
        ICatalogoRequisitoRepository catalogo,
        CreateMobilizacaoUseCase create)
    {
        _mobilizacoes = mobilizacoes;
        _funcionarios = funcionarios;
        _empresas = empresas;
        _catalogo = catalogo;
        _create = create;
    }

    public async Task<MobilizacaoResponse> ExecuteAsync(
        Guid id,
        Guid? scopeEmpresaId,
        CancellationToken cancellationToken = default)
    {
        var mobilizacao = await _mobilizacoes.GetByIdAsync(id, cancellationToken)
            ?? throw new MobilizacaoException("Mobilização não encontrada.", 404);
        var funcionario = await _funcionarios.GetByIdAsync(mobilizacao.FuncionarioId, cancellationToken)
            ?? throw new MobilizacaoException("Funcionário não encontrado.", 404);
        if (scopeEmpresaId is not null && funcionario.EmpresaId != scopeEmpresaId)
        {
            throw new MobilizacaoException("Sem permissão para esta mobilização.", 403);
        }

        var empresa = await _empresas.GetByIdAsync(funcionario.EmpresaId, cancellationToken)
            ?? throw new EmpresaException("Empresa não encontrada.");
        var catalogo = await _catalogo.ListAsync(cancellationToken);
        return await _create.MapAsync(mobilizacao, funcionario, empresa, catalogo, cancellationToken);
    }
}

public sealed class UpdateMobilizacaoUseCase
{
    private readonly IMobilizacaoRepository _mobilizacoes;
    private readonly GetMobilizacaoUseCase _get;

    public UpdateMobilizacaoUseCase(IMobilizacaoRepository mobilizacoes, GetMobilizacaoUseCase get)
    {
        _mobilizacoes = mobilizacoes;
        _get = get;
    }

    public async Task<MobilizacaoResponse> ExecuteAsync(
        Guid id,
        UpdateMobilizacaoRequest request,
        Guid? scopeEmpresaId,
        CancellationToken cancellationToken = default)
    {
        await _get.ExecuteAsync(id, scopeEmpresaId, cancellationToken);
        var mobilizacao = await _mobilizacoes.GetByIdAsync(id, cancellationToken)
            ?? throw new MobilizacaoException("Mobilização não encontrada.", 404);
        try
        {
            mobilizacao.AtualizarCadastro(request.Funcao, request.DataFimObra, request.TurnoJornada);
        }
        catch (ArgumentException ex)
        {
            throw new MobilizacaoException(ex.Message);
        }

        await _mobilizacoes.UpdateAsync(mobilizacao, cancellationToken);
        return await _get.ExecuteAsync(id, scopeEmpresaId, cancellationToken);
    }
}
