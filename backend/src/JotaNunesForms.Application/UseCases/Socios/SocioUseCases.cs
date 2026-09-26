using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Application.Socios;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Socios;

public sealed class ListSociosUseCase
{
    private readonly IEmpresaRepository _empresas;
    private readonly ISocioRepository _socios;

    public ListSociosUseCase(IEmpresaRepository empresas, ISocioRepository socios)
    {
        _empresas = empresas;
        _socios = socios;
    }

    public async Task<IReadOnlyList<SocioResponse>> ExecuteAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        var empresa = await _empresas.GetByIdAsync(empresaId, cancellationToken)
            ?? throw new EmpresaException("Empresa não encontrada.");
        _ = empresa;
        var socios = await _socios.ListByEmpresaAsync(empresaId, cancellationToken);
        return socios.OrderBy(s => s.CriadoEm).Select(SocioResponse.FromEntity).ToList();
    }
}

public sealed class CreateSocioUseCase
{
    private readonly IEmpresaRepository _empresas;
    private readonly ISocioRepository _socios;
    private readonly IProcessoContratacaoRepository _processos;
    private readonly IItemChecklistRepository _itens;

    public CreateSocioUseCase(
        IEmpresaRepository empresas,
        ISocioRepository socios,
        IProcessoContratacaoRepository processos,
        IItemChecklistRepository itens)
    {
        _empresas = empresas;
        _socios = socios;
        _processos = processos;
        _itens = itens;
    }

    public async Task<SocioResponse> ExecuteAsync(
        Guid empresaId,
        CreateSocioRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresa = await _empresas.GetByIdAsync(empresaId, cancellationToken)
            ?? throw new EmpresaException("Empresa não encontrada.");

        string cpf;
        try
        {
            cpf = Socio.NormalizeCpf(request.Cpf);
        }
        catch (ArgumentException ex)
        {
            throw new SocioException(ex.Message);
        }

        if (await _socios.ExistsCpfAsync(empresa.Id, cpf, excludeId: null, cancellationToken))
        {
            throw new SocioException("Já existe sócio com este CPF nesta empresa.", 409);
        }

        Socio socio;
        try
        {
            socio = new Socio(empresa.Id, request.Nome, cpf);
        }
        catch (ArgumentException ex)
        {
            throw new SocioException(ex.Message);
        }

        await _socios.AddAsync(socio, cancellationToken);
        await SocioChecklistBinder.AssignNextSlotAsync(socio, _processos, _itens, cancellationToken);
        return SocioResponse.FromEntity(socio);
    }
}

public sealed class UpdateSocioUseCase
{
    private readonly IEmpresaRepository _empresas;
    private readonly ISocioRepository _socios;

    public UpdateSocioUseCase(IEmpresaRepository empresas, ISocioRepository socios)
    {
        _empresas = empresas;
        _socios = socios;
    }

    public async Task<SocioResponse> ExecuteAsync(
        Guid empresaId,
        Guid socioId,
        UpdateSocioRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresa = await _empresas.GetByIdAsync(empresaId, cancellationToken)
            ?? throw new EmpresaException("Empresa não encontrada.");

        var socio = await _socios.GetByIdAsync(socioId, cancellationToken)
            ?? throw new SocioException("Sócio não encontrado.", 404);

        if (socio.EmpresaId != empresa.Id)
        {
            throw new SocioException("Sócio não pertence à empresa.", 404);
        }

        try
        {
            socio.Atualizar(request.Nome, request.Ativo);
        }
        catch (ArgumentException ex)
        {
            throw new SocioException(ex.Message);
        }

        await _socios.UpdateAsync(socio, cancellationToken);
        return SocioResponse.FromEntity(socio);
    }
}

internal static class SocioChecklistBinder
{
    public static async Task BindExistingAsync(
        Guid empresaId,
        IReadOnlyList<ItemChecklist> itens,
        ISocioRepository socios,
        IItemChecklistRepository itensRepo,
        CancellationToken cancellationToken)
    {
        var quadro = (await socios.ListByEmpresaAsync(empresaId, cancellationToken))
            .Where(s => s.Ativo)
            .OrderBy(s => s.CriadoEm)
            .ToList();

        for (var i = 0; i < quadro.Count; i++)
        {
            var ordem = i + 1;
            var socio = quadro[i];
            foreach (var item in itens.Where(it =>
                         it.Ativo
                         && it.TitularTipo == TitularRequisito.Socio
                         && it.TitularOrdem == ordem
                         && it.TitularId is null))
            {
                item.VincularTitular(socio.Id);
                await itensRepo.UpdateAsync(item, cancellationToken);
            }
        }
    }

    public static async Task AssignNextSlotAsync(
        Socio socio,
        IProcessoContratacaoRepository processos,
        IItemChecklistRepository itens,
        CancellationToken cancellationToken)
    {
        var lista = await processos.ListAsync(socio.EmpresaId, cancellationToken);
        foreach (var processo in lista.Where(p => p.Situacao != SituacaoProcesso.Encerrado))
        {
            var doProcesso = await itens.ListByProcessoAsync(processo.Id, cancellationToken);
            var freeOrdem = doProcesso
                .Where(i => i.Ativo
                            && i.TitularTipo == TitularRequisito.Socio
                            && i.TitularId is null
                            && i.TitularOrdem is not null)
                .Select(i => i.TitularOrdem!.Value)
                .Distinct()
                .OrderBy(o => o)
                .Cast<int?>()
                .FirstOrDefault();

            if (freeOrdem is null)
            {
                continue;
            }

            foreach (var item in doProcesso.Where(i =>
                         i.Ativo
                         && i.TitularTipo == TitularRequisito.Socio
                         && i.TitularOrdem == freeOrdem))
            {
                item.VincularTitular(socio.Id);
                await itens.UpdateAsync(item, cancellationToken);
            }
        }
    }
}
