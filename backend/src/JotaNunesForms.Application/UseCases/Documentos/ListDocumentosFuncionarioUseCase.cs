using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class ListDocumentosFuncionarioUseCase
{
    private readonly IDocumentoFuncionarioRepository _documentos;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;

    public ListDocumentosFuncionarioUseCase(
        IDocumentoFuncionarioRepository documentos,
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas)
    {
        _documentos = documentos;
        _funcionarios = funcionarios;
        _empresas = empresas;
    }

    public async Task<IReadOnlyList<DocumentoFuncionarioResponse>> ExecuteAsync(
        Guid funcionarioId,
        Guid? scopeEmpresaId,
        CancellationToken cancellationToken = default)
    {
        var funcionario = await _funcionarios.GetByIdAsync(funcionarioId, cancellationToken);
        if (funcionario is null)
        {
            throw new DocumentoFuncionarioException("Funcionário não encontrado.");
        }

        if (scopeEmpresaId is not null && funcionario.EmpresaId != scopeEmpresaId.Value)
        {
            throw new DocumentoFuncionarioException("Sem permissão para acessar este funcionário.");
        }

        var empresa = await _empresas.GetByIdAsync(funcionario.EmpresaId, cancellationToken);
        var empresaNome = empresa?.RazaoSocial ?? "—";

        var list = await _documentos.ListByFuncionarioAsync(funcionarioId, cancellationToken);
        return list
            .Select(d => DocumentoFuncionarioResponse.FromEntity(d, funcionario, empresaNome))
            .ToList();
    }
}
