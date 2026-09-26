using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Validacao;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Validacao;

public sealed class ListValidacaoFilaUseCase
{
    private readonly IDocumentoEmpresaRepository _documentosEmpresa;
    private readonly IDocumentoFuncionarioRepository _documentosFuncionario;
    private readonly IEmpresaRepository _empresas;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IDocumentoVersaoRepository _versoes;
    private readonly IItemChecklistRepository _itens;
    private readonly IProcessoContratacaoRepository _processos;
    private readonly IContratoRepository _contratos;
    private readonly ICatalogoRequisitoRepository _catalogo;

    public ListValidacaoFilaUseCase(
        IDocumentoEmpresaRepository documentosEmpresa,
        IDocumentoFuncionarioRepository documentosFuncionario,
        IEmpresaRepository empresas,
        IFuncionarioRepository funcionarios,
        IDocumentoVersaoRepository versoes,
        IItemChecklistRepository itens,
        IProcessoContratacaoRepository processos,
        IContratoRepository contratos,
        ICatalogoRequisitoRepository catalogo)
    {
        _documentosEmpresa = documentosEmpresa;
        _documentosFuncionario = documentosFuncionario;
        _empresas = empresas;
        _funcionarios = funcionarios;
        _versoes = versoes;
        _itens = itens;
        _processos = processos;
        _contratos = contratos;
        _catalogo = catalogo;
    }

    public async Task<IReadOnlyList<ValidacaoDocumentoItemResponse>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var empresas = await _empresas.ListAsync(cancellationToken);
        var nomesEmpresa = empresas.ToDictionary(e => e.Id, e => e.RazaoSocial);

        var empresaDocs = await _documentosEmpresa.ListPendentesAsync(cancellationToken);
        var funcionarioDocs = await _documentosFuncionario.ListPendentesAsync(cancellationToken);

        var items = new List<ValidacaoDocumentoItemResponse>(empresaDocs.Count + funcionarioDocs.Count);

        foreach (var doc in empresaDocs)
        {
            if (doc.Status == StatusDocumento.Pendente)
            {
                doc.IniciarAnalise();
                await _documentosEmpresa.UpdateAsync(doc, cancellationToken);
            }

            items.Add(new ValidacaoDocumentoItemResponse(
                "empresa",
                doc.Id,
                doc.EmpresaId,
                nomesEmpresa.GetValueOrDefault(doc.EmpresaId, "—"),
                null,
                null,
                DocumentoEmpresaResponse.TipoLabel(doc.Tipo),
                doc.NomeArquivo,
                doc.TamanhoBytes,
                doc.Status,
                DocumentoEmpresaResponse.StatusLabel(doc.Status),
                doc.EnviadoEm));
        }

        foreach (var doc in funcionarioDocs)
        {
            var funcionario = await _funcionarios.GetByIdAsync(doc.FuncionarioId, cancellationToken);
            if (funcionario is null)
            {
                continue;
            }

            if (doc.Status == StatusDocumento.Pendente)
            {
                doc.IniciarAnalise();
                await _documentosFuncionario.UpdateAsync(doc, cancellationToken);
            }

            items.Add(new ValidacaoDocumentoItemResponse(
                "funcionario",
                doc.Id,
                funcionario.EmpresaId,
                nomesEmpresa.GetValueOrDefault(funcionario.EmpresaId, "—"),
                funcionario.Id,
                funcionario.Nome,
                DocumentoFuncionarioResponse.TipoLabel(doc.Tipo),
                doc.NomeArquivo,
                doc.TamanhoBytes,
                doc.Status,
                DocumentoFuncionarioResponse.StatusLabel(doc.Status),
                doc.EnviadoEm));
        }

        var catalogo = (await _catalogo.ListAsync(cancellationToken)).ToDictionary(c => c.Id);
        var versoesPendentes = await _versoes.ListVigentesPendentesAsync(cancellationToken);
        foreach (var versao in versoesPendentes)
        {
            var item = await _itens.GetByIdAsync(versao.ItemChecklistId, cancellationToken);
            if (item is null)
            {
                continue;
            }

            var processo = await _processos.GetByIdAsync(item.ProcessoId, cancellationToken);
            if (processo is null)
            {
                continue;
            }

            var contrato = await _contratos.GetByIdAsync(processo.ContratoId, cancellationToken);
            if (contrato is null)
            {
                continue;
            }

            catalogo.TryGetValue(item.CatalogoRequisitoId, out var requisito);
            items.Add(new ValidacaoDocumentoItemResponse(
                "versao",
                versao.Id,
                contrato.EmpresaId,
                nomesEmpresa.GetValueOrDefault(contrato.EmpresaId, "—"),
                null,
                item.TitularTipo == TitularRequisito.Socio
                    ? $"Sócio {item.TitularOrdem}"
                    : item.TitularTipo == TitularRequisito.Contrato
                        ? "Contrato"
                        : null,
                requisito?.Nome ?? "Requisito",
                versao.NomeArquivo ?? "formulário",
                versao.TamanhoBytes ?? 0,
                StatusDocumento.EmAnalise,
                DocumentoEmpresaResponse.StatusLabel(StatusDocumento.EmAnalise),
                versao.EnviadoEm,
                processo.Id,
                item.Id,
                versao.Id,
                requisito?.Codigo));
        }

        return items.OrderBy(i => i.EnviadoEm).ToList();
    }
}

public sealed class AprovarDocumentoEmpresaValidacaoUseCase
{
    private readonly IDocumentoEmpresaRepository _documentos;
    private readonly IEmpresaRepository _empresas;

    public AprovarDocumentoEmpresaValidacaoUseCase(
        IDocumentoEmpresaRepository documentos,
        IEmpresaRepository empresas)
    {
        _documentos = documentos;
        _empresas = empresas;
    }

    public async Task<DocumentoEmpresaResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var documento = await _documentos.GetByIdAsync(id, cancellationToken);
        if (documento is null)
        {
            throw new ValidacaoException("Documento não encontrado.");
        }

        try
        {
            documento.Aprovar();
        }
        catch (InvalidOperationException ex)
        {
            throw new ValidacaoException(ex.Message);
        }

        await _documentos.UpdateAsync(documento, cancellationToken);
        var empresa = await _empresas.GetByIdAsync(documento.EmpresaId, cancellationToken);
        return DocumentoEmpresaResponse.FromEntity(documento, empresa?.RazaoSocial ?? "—");
    }
}

public sealed class RejeitarDocumentoEmpresaValidacaoUseCase
{
    private readonly IDocumentoEmpresaRepository _documentos;
    private readonly IEmpresaRepository _empresas;

    public RejeitarDocumentoEmpresaValidacaoUseCase(
        IDocumentoEmpresaRepository documentos,
        IEmpresaRepository empresas)
    {
        _documentos = documentos;
        _empresas = empresas;
    }

    public async Task<DocumentoEmpresaResponse> ExecuteAsync(
        Guid id,
        RejeitarDocumentoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
        {
            throw new ValidacaoException("Informe o motivo da rejeição (RF10).");
        }

        var documento = await _documentos.GetByIdAsync(id, cancellationToken);
        if (documento is null)
        {
            throw new ValidacaoException("Documento não encontrado.");
        }

        try
        {
            documento.Rejeitar(request.Motivo);
        }
        catch (ArgumentException ex)
        {
            throw new ValidacaoException(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new ValidacaoException(ex.Message);
        }

        await _documentos.UpdateAsync(documento, cancellationToken);
        var empresa = await _empresas.GetByIdAsync(documento.EmpresaId, cancellationToken);
        return DocumentoEmpresaResponse.FromEntity(documento, empresa?.RazaoSocial ?? "—");
    }
}

public sealed class AprovarDocumentoFuncionarioValidacaoUseCase
{
    private readonly IDocumentoFuncionarioRepository _documentos;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;

    public AprovarDocumentoFuncionarioValidacaoUseCase(
        IDocumentoFuncionarioRepository documentos,
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas)
    {
        _documentos = documentos;
        _funcionarios = funcionarios;
        _empresas = empresas;
    }

    public async Task<DocumentoFuncionarioResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var documento = await _documentos.GetByIdAsync(id, cancellationToken);
        if (documento is null)
        {
            throw new ValidacaoException("Documento não encontrado.");
        }

        var funcionario = await _funcionarios.GetByIdAsync(documento.FuncionarioId, cancellationToken);
        if (funcionario is null)
        {
            throw new ValidacaoException("Funcionário não encontrado.");
        }

        try
        {
            documento.Aprovar();
        }
        catch (InvalidOperationException ex)
        {
            throw new ValidacaoException(ex.Message);
        }

        await _documentos.UpdateAsync(documento, cancellationToken);
        var empresa = await _empresas.GetByIdAsync(funcionario.EmpresaId, cancellationToken);
        return DocumentoFuncionarioResponse.FromEntity(documento, funcionario, empresa?.RazaoSocial ?? "—");
    }
}

public sealed class RejeitarDocumentoFuncionarioValidacaoUseCase
{
    private readonly IDocumentoFuncionarioRepository _documentos;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;

    public RejeitarDocumentoFuncionarioValidacaoUseCase(
        IDocumentoFuncionarioRepository documentos,
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas)
    {
        _documentos = documentos;
        _funcionarios = funcionarios;
        _empresas = empresas;
    }

    public async Task<DocumentoFuncionarioResponse> ExecuteAsync(
        Guid id,
        RejeitarDocumentoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
        {
            throw new ValidacaoException("Informe o motivo da rejeição (RF10).");
        }

        var documento = await _documentos.GetByIdAsync(id, cancellationToken);
        if (documento is null)
        {
            throw new ValidacaoException("Documento não encontrado.");
        }

        var funcionario = await _funcionarios.GetByIdAsync(documento.FuncionarioId, cancellationToken);
        if (funcionario is null)
        {
            throw new ValidacaoException("Funcionário não encontrado.");
        }

        try
        {
            documento.Rejeitar(request.Motivo);
        }
        catch (ArgumentException ex)
        {
            throw new ValidacaoException(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new ValidacaoException(ex.Message);
        }

        await _documentos.UpdateAsync(documento, cancellationToken);
        var empresa = await _empresas.GetByIdAsync(funcionario.EmpresaId, cancellationToken);
        return DocumentoFuncionarioResponse.FromEntity(documento, funcionario, empresa?.RazaoSocial ?? "—");
    }
}
