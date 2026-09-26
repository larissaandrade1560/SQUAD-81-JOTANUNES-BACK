using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Application.Obras;
using JotaNunesForms.Application.Processos;
using JotaNunesForms.Application.UseCases.Socios;
using JotaNunesForms.Application.UseCases.Validacao;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Domain.Services;

namespace JotaNunesForms.Application.UseCases.Processos;

public sealed class CreateProcessoContratacaoUseCase
{
    private readonly IEmpresaRepository _empresas;
    private readonly IObraRepository _obras;
    private readonly IContratoRepository _contratos;
    private readonly IProcessoContratacaoRepository _processos;
    private readonly ICatalogoRequisitoRepository _catalogo;
    private readonly IItemChecklistRepository _itens;
    private readonly ISocioRepository _socios;

    public CreateProcessoContratacaoUseCase(
        IEmpresaRepository empresas,
        IObraRepository obras,
        IContratoRepository contratos,
        IProcessoContratacaoRepository processos,
        ICatalogoRequisitoRepository catalogo,
        IItemChecklistRepository itens,
        ISocioRepository socios)
    {
        _empresas = empresas;
        _obras = obras;
        _contratos = contratos;
        _processos = processos;
        _catalogo = catalogo;
        _itens = itens;
        _socios = socios;
    }

    public async Task<ProcessoContratacaoResponse> ExecuteAsync(
        CreateProcessoContratacaoRequest request,
        Guid abertoPorUsuarioId,
        CancellationToken cancellationToken = default)
    {
        var empresa = await _empresas.GetByIdAsync(request.EmpresaId, cancellationToken)
            ?? throw new EmpresaException("Empresa não encontrada.");
        var obra = await _obras.GetByIdAsync(request.ObraId, cancellationToken)
            ?? throw new ObraException("Obra não encontrada.");

        if (empresa.Tipo == TipoEmpresa.Materiais && request.MobilizaTrabalhadores)
        {
            throw new ProcessoException("Empresa de materiais não pode mobilizar trabalhadores.");
        }

        Contrato contrato;
        if (request.ContratoId is { } contratoId)
        {
            contrato = await _contratos.GetByIdAsync(contratoId, cancellationToken)
                ?? throw new ProcessoException("Contrato não encontrado.");
            if (contrato.EmpresaId != empresa.Id || contrato.ObraId != obra.Id)
            {
                throw new ProcessoException("Contrato não pertence à empresa e obra informadas.");
            }
        }
        else
        {
            try
            {
                contrato = new Contrato(
                    empresa.Id,
                    obra.Id,
                    request.ServicoContratado,
                    request.NumeroContrato);
                await _contratos.AddAsync(contrato, cancellationToken);
            }
            catch (ArgumentException ex)
            {
                throw new ProcessoException(ex.Message);
            }
        }

        try
        {
            var processo = new ProcessoContratacao(
                contrato.Id,
                abertoPorUsuarioId,
                request.ServicoContratado,
                request.TransportaResiduos,
                request.ControleTecnologico,
                request.OptanteSimples,
                request.ExigeArt,
                request.QuantidadeSocios,
                request.MobilizaTrabalhadores);

            var requisitos = await _catalogo.ListAsync(cancellationToken);
            var drafts = GeracaoChecklist.Gerar(processo, empresa.Tipo, requisitos);
            await _processos.AddAsync(processo, cancellationToken);

            var itens = drafts.Select(d => new ItemChecklist(
                processo.Id,
                d.CatalogoRequisitoId,
                d.Titular,
                d.Obrigatorio,
                titularOrdem: d.TitularOrdem)).ToList();
            await _itens.AddRangeAsync(itens, cancellationToken);
            await SocioChecklistBinder.BindExistingAsync(empresa.Id, itens, _socios, _itens, cancellationToken);
            var atualizados = await _itens.ListByProcessoAsync(processo.Id, cancellationToken);
            return ChecklistMapper.ToResponse(processo, contrato, requisitos, atualizados);
        }
        catch (ArgumentException ex)
        {
            throw new ProcessoException(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new ProcessoException(ex.Message);
        }
    }
}

public sealed class ListProcessosContratacaoUseCase
{
    private readonly IProcessoContratacaoRepository _processos;
    private readonly IContratoRepository _contratos;
    private readonly ICatalogoRequisitoRepository _catalogo;
    private readonly IItemChecklistRepository _itens;

    public ListProcessosContratacaoUseCase(
        IProcessoContratacaoRepository processos,
        IContratoRepository contratos,
        ICatalogoRequisitoRepository catalogo,
        IItemChecklistRepository itens)
    {
        _processos = processos;
        _contratos = contratos;
        _catalogo = catalogo;
        _itens = itens;
    }

    public async Task<IReadOnlyList<ProcessoContratacaoResponse>> ExecuteAsync(
        Guid? empresaId,
        CancellationToken cancellationToken = default)
    {
        var processos = await _processos.ListAsync(empresaId, cancellationToken);
        var catalogo = await _catalogo.ListAsync(cancellationToken);
        var resultado = new List<ProcessoContratacaoResponse>();
        foreach (var processo in processos)
        {
            var contrato = await _contratos.GetByIdAsync(processo.ContratoId, cancellationToken);
            if (contrato is null)
            {
                continue;
            }

            if (empresaId is not null && contrato.EmpresaId != empresaId)
            {
                continue;
            }

            var itens = await _itens.ListByProcessoAsync(processo.Id, cancellationToken);
            resultado.Add(ChecklistMapper.ToResponse(processo, contrato, catalogo, itens));
        }

        return resultado;
    }
}

public sealed class GetProcessoContratacaoUseCase
{
    private readonly IProcessoContratacaoRepository _processos;
    private readonly IContratoRepository _contratos;
    private readonly ICatalogoRequisitoRepository _catalogo;
    private readonly IItemChecklistRepository _itens;
    private readonly RecalcularSituacaoProcessoUseCase _recalcular;

    public GetProcessoContratacaoUseCase(
        IProcessoContratacaoRepository processos,
        IContratoRepository contratos,
        ICatalogoRequisitoRepository catalogo,
        IItemChecklistRepository itens,
        RecalcularSituacaoProcessoUseCase recalcular)
    {
        _processos = processos;
        _contratos = contratos;
        _catalogo = catalogo;
        _itens = itens;
        _recalcular = recalcular;
    }

    public async Task<ProcessoContratacaoResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var processo = await _processos.GetByIdAsync(id, cancellationToken)
            ?? throw new ProcessoException("Processo não encontrado.");
        await _recalcular.ExecuteAsync(id, cancellationToken);
        processo = await _processos.GetByIdAsync(id, cancellationToken)
            ?? throw new ProcessoException("Processo não encontrado.");
        var contrato = await _contratos.GetByIdAsync(processo.ContratoId, cancellationToken)
            ?? throw new ProcessoException("Contrato do processo não encontrado.");
        var catalogo = await _catalogo.ListAsync(cancellationToken);
        var itens = await _itens.ListByProcessoAsync(processo.Id, cancellationToken);
        return ChecklistMapper.ToResponse(processo, contrato, catalogo, itens);
    }
}

public sealed class ListChecklistProcessoUseCase
{
    private readonly GetProcessoContratacaoUseCase _get;

    public ListChecklistProcessoUseCase(GetProcessoContratacaoUseCase get) => _get = get;

    public async Task<IReadOnlyList<ItemChecklistResponse>> ExecuteAsync(
        Guid processoId,
        CancellationToken cancellationToken = default)
    {
        var processo = await _get.ExecuteAsync(processoId, cancellationToken);
        return processo.Checklist;
    }
}

public sealed class UpdateProcessoContratacaoUseCase
{
    private readonly IProcessoContratacaoRepository _processos;
    private readonly IContratoRepository _contratos;
    private readonly IEmpresaRepository _empresas;
    private readonly ICatalogoRequisitoRepository _catalogo;
    private readonly IItemChecklistRepository _itens;
    private readonly ISocioRepository _socios;

    public UpdateProcessoContratacaoUseCase(
        IProcessoContratacaoRepository processos,
        IContratoRepository contratos,
        IEmpresaRepository empresas,
        ICatalogoRequisitoRepository catalogo,
        IItemChecklistRepository itens,
        ISocioRepository socios)
    {
        _processos = processos;
        _contratos = contratos;
        _empresas = empresas;
        _catalogo = catalogo;
        _itens = itens;
        _socios = socios;
    }

    public async Task<ProcessoContratacaoResponse> ExecuteAsync(
        Guid id,
        UpdateProcessoContratacaoRequest request,
        CancellationToken cancellationToken = default)
    {
        var processo = await _processos.GetByIdAsync(id, cancellationToken)
            ?? throw new ProcessoException("Processo não encontrado.");
        var contrato = await _contratos.GetByIdAsync(processo.ContratoId, cancellationToken)
            ?? throw new ProcessoException("Contrato do processo não encontrado.");
        var empresa = await _empresas.GetByIdAsync(contrato.EmpresaId, cancellationToken)
            ?? throw new EmpresaException("Empresa não encontrada.");

        if (empresa.Tipo == TipoEmpresa.Materiais && request.MobilizaTrabalhadores)
        {
            throw new ProcessoException("Empresa de materiais não pode mobilizar trabalhadores.");
        }

        try
        {
            processo.AtualizarQuestionario(
                request.ServicoContratado,
                request.TransportaResiduos,
                request.ControleTecnologico,
                request.OptanteSimples,
                request.ExigeArt,
                request.QuantidadeSocios,
                request.MobilizaTrabalhadores);
        }
        catch (ArgumentException ex)
        {
            throw new ProcessoException(ex.Message);
        }

        var requisitos = await _catalogo.ListAsync(cancellationToken);
        IReadOnlyList<ItemChecklistDraft> drafts;
        try
        {
            drafts = GeracaoChecklist.Gerar(processo, empresa.Tipo, requisitos);
        }
        catch (InvalidOperationException ex)
        {
            throw new ProcessoException(ex.Message);
        }

        var existentes = (await _itens.ListByProcessoAsync(processo.Id, cancellationToken)).ToList();
        foreach (var item in existentes)
        {
            var aindaAplica = drafts.Any(d => item.MesmaChave(d.CatalogoRequisitoId, d.Titular, d.TitularOrdem));
            if (aindaAplica)
            {
                item.Reativar();
            }
            else
            {
                item.Desativar();
            }

            await _itens.UpdateAsync(item, cancellationToken);
        }

        var novos = drafts
            .Where(d => existentes.All(e => !e.MesmaChave(d.CatalogoRequisitoId, d.Titular, d.TitularOrdem)))
            .Select(d => new ItemChecklist(
                processo.Id,
                d.CatalogoRequisitoId,
                d.Titular,
                d.Obrigatorio,
                titularOrdem: d.TitularOrdem))
            .ToList();
        if (novos.Count > 0)
        {
            await _itens.AddRangeAsync(novos, cancellationToken);
        }

        await _processos.UpdateAsync(processo, cancellationToken);
        var atualizados = await _itens.ListByProcessoAsync(processo.Id, cancellationToken);
        await SocioChecklistBinder.BindExistingAsync(empresa.Id, atualizados, _socios, _itens, cancellationToken);
        atualizados = await _itens.ListByProcessoAsync(processo.Id, cancellationToken);
        return ChecklistMapper.ToResponse(processo, contrato, requisitos, atualizados);
    }
}

public sealed class EncaminharProcessoSetorContratosUseCase
{
    private readonly GetProcessoContratacaoUseCase _get;
    private readonly IProcessoContratacaoRepository _processos;

    public EncaminharProcessoSetorContratosUseCase(
        GetProcessoContratacaoUseCase get,
        IProcessoContratacaoRepository processos)
    {
        _get = get;
        _processos = processos;
    }

    public async Task<ProcessoContratacaoResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var processo = await _processos.GetByIdAsync(id, cancellationToken)
            ?? throw new ProcessoException("Processo não encontrado.");
        processo.EncaminharSetorContratos();
        await _processos.UpdateAsync(processo, cancellationToken);
        return await _get.ExecuteAsync(id, cancellationToken);
    }
}

internal static class ChecklistMapper
{
    public static ProcessoContratacaoResponse ToResponse(
        ProcessoContratacao processo,
        Contrato contrato,
        IReadOnlyList<CatalogoRequisito> catalogo,
        IReadOnlyList<ItemChecklist> itens)
    {
        var porId = catalogo.ToDictionary(c => c.Id);
        var checklist = itens
            .OrderBy(i => i.TitularTipo)
            .ThenBy(i => i.TitularOrdem ?? 0)
            .ThenBy(i => porId.TryGetValue(i.CatalogoRequisitoId, out var r) ? r.Codigo : string.Empty)
            .Select(i =>
            {
                porId.TryGetValue(i.CatalogoRequisitoId, out var req);
                return new ItemChecklistResponse(
                    i.Id,
                    i.CatalogoRequisitoId,
                    req?.Codigo ?? string.Empty,
                    req?.Nome ?? string.Empty,
                    i.TitularTipo,
                    i.TitularId,
                    i.TitularOrdem,
                    i.Obrigatorio,
                    i.Ativo,
                    i.Situacao);
            })
            .ToList();

        return new ProcessoContratacaoResponse(
            processo.Id,
            processo.ContratoId,
            contrato.EmpresaId,
            contrato.ObraId,
            processo.ServicoContratado,
            processo.TransportaResiduos,
            processo.ControleTecnologico,
            processo.OptanteSimples,
            processo.ExigeArt,
            processo.QuantidadeSociosInformada,
            processo.MobilizaTrabalhadores,
            processo.AbertoEm,
            processo.EncaminhadoSetorContratosEm,
            processo.Situacao,
            checklist);
    }
}
