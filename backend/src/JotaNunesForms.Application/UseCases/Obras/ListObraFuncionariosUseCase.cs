using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Obras;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Obras;

public sealed class ListObraFuncionariosUseCase
{
    private readonly IObraRepository _obras;
    private readonly IFuncionarioObraRepository _vinculos;
    private readonly IEmpresaRepository _empresas;

    public ListObraFuncionariosUseCase(
        IObraRepository obras,
        IFuncionarioObraRepository vinculos,
        IEmpresaRepository empresas)
    {
        _obras = obras;
        _vinculos = vinculos;
        _empresas = empresas;
    }

    public async Task<IReadOnlyList<ObraFuncionarioAlocacaoResponse>> ExecuteAsync(
        Guid obraId,
        CancellationToken cancellationToken = default)
    {
        var obra = await _obras.GetByIdAsync(obraId, cancellationToken);
        if (obra is null)
        {
            throw new ObraException("Obra não encontrada.");
        }

        var funcionarios = await _vinculos.ListFuncionariosByObraIdAsync(obraId, cancellationToken);
        var empresas = await _empresas.ListAsync(cancellationToken);
        var nomes = empresas.ToDictionary(e => e.Id, e => e.RazaoSocial);

        return funcionarios
            .Select(f => new ObraFuncionarioAlocacaoResponse(
                f.Id,
                f.Nome,
                f.Cpf,
                f.Cargo,
                f.Ativo,
                nomes.GetValueOrDefault(f.EmpresaId, "—")))
            .ToList();
    }
}
