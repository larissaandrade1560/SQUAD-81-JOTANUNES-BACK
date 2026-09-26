using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Application.Processos;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Domain.Services;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class EnviarVersaoDocumentoUseCase
{
    private const long MaxBytes = 10 * 1024 * 1024;

    private readonly IItemChecklistRepository _itens;
    private readonly IProcessoContratacaoRepository _processos;
    private readonly IContratoRepository _contratos;
    private readonly ICatalogoRequisitoRepository _catalogo;
    private readonly IDocumentoVersaoRepository _versoes;
    private readonly IObjectStorage _storage;

    public EnviarVersaoDocumentoUseCase(
        IItemChecklistRepository itens,
        IProcessoContratacaoRepository processos,
        IContratoRepository contratos,
        ICatalogoRequisitoRepository catalogo,
        IDocumentoVersaoRepository versoes,
        IObjectStorage storage)
    {
        _itens = itens;
        _processos = processos;
        _contratos = contratos;
        _catalogo = catalogo;
        _versoes = versoes;
        _storage = storage;
    }

    public async Task<DocumentoVersaoResponse> ExecuteAsync(
        Guid itemId,
        Guid enviadoPorUsuarioId,
        Guid? scopeEmpresaId,
        string? nomeArquivo,
        string? contentType,
        long? tamanhoBytes,
        Stream? conteudo,
        string? camposJson,
        CancellationToken cancellationToken = default)
    {
        var item = await _itens.GetByIdAsync(itemId, cancellationToken)
            ?? throw new DocumentoVersaoException("Item de checklist não encontrado.", 404);

        if (!item.Ativo)
        {
            throw new DocumentoVersaoException("Item inativo não recebe novas versões.", 409);
        }

        var processo = await _processos.GetByIdAsync(item.ProcessoId, cancellationToken)
            ?? throw new ProcessoException("Processo não encontrado.");
        var contrato = await _contratos.GetByIdAsync(processo.ContratoId, cancellationToken)
            ?? throw new ProcessoException("Contrato do processo não encontrado.");

        if (scopeEmpresaId is not null && contrato.EmpresaId != scopeEmpresaId.Value)
        {
            throw new EmpresaException("Sem permissão para este item.");
        }

        var requisito = await _catalogo.GetByIdAsync(item.CatalogoRequisitoId, cancellationToken)
            ?? throw new DocumentoVersaoException("Requisito do catálogo não encontrado.");

        if (requisito.TipoEntrega == TipoEntregaRequisito.Movimento)
        {
            throw new DocumentoVersaoException("Este requisito é registrado por movimento, não por upload.");
        }

        var temArquivo = conteudo is not null && tamanhoBytes is > 0 && !string.IsNullOrWhiteSpace(nomeArquivo);
        if (requisito.TipoEntrega == TipoEntregaRequisito.Upload && !temArquivo)
        {
            throw new DocumentoVersaoException("Envie um arquivo PDF.");
        }

        if (requisito.TipoEntrega == TipoEntregaRequisito.Formulario && string.IsNullOrWhiteSpace(camposJson) && !temArquivo)
        {
            throw new DocumentoVersaoException("Informe os campos do formulário.");
        }

        string? storageKey = null;
        string? hash = null;
        string? nome = null;
        string? tipo = null;
        long? tamanho = null;
        var versaoId = Guid.NewGuid();

        if (temArquivo)
        {
            if (!_storage.IsConfigured)
            {
                throw new DocumentoVersaoException("Armazenamento de documentos não configurado (R2).");
            }

            if (tamanhoBytes is <= 0 or > MaxBytes)
            {
                throw new DocumentoVersaoException("Arquivo deve ter no máximo 10 MB.");
            }

            if (!string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
                && !(nomeArquivo?.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                throw new DocumentoVersaoException("Envie apenas arquivos PDF.");
            }

            await using var buffer = new MemoryStream();
            await conteudo!.CopyToAsync(buffer, cancellationToken);
            buffer.Position = 0;
            hash = FileHash.Sha256Hex(buffer);
            buffer.Position = 0;

            storageKey = $"processos/{processo.Id:D}/itens/{item.Id:D}/versoes/{versaoId:D}.pdf";
            await _storage.UploadAsync(storageKey, buffer, "application/pdf", cancellationToken);
            nome = nomeArquivo;
            tipo = "application/pdf";
            tamanho = tamanhoBytes;
        }

        var existentes = await _versoes.ListByItemAsync(item.Id, cancellationToken);
        foreach (var anterior in existentes.Where(v => v.Vigente))
        {
            anterior.MarcarNaoVigente();
            await _versoes.UpdateAsync(anterior, cancellationToken);
        }

        var numero = existentes.Count == 0 ? 1 : existentes.Max(v => v.Numero) + 1;
        var versao = new DocumentoVersao(
            item.Id,
            numero,
            enviadoPorUsuarioId,
            nome,
            storageKey,
            tipo,
            tamanho,
            hash,
            camposJson,
            versaoId);

        await _versoes.AddAsync(versao, cancellationToken);
        item.DefinirSituacao(SituacaoItemChecklist.PendenteAnalise);
        await _itens.UpdateAsync(item, cancellationToken);

        return DocumentoVersaoResponse.FromEntity(versao);
    }
}

public sealed class ListVersoesItemUseCase
{
    private readonly IItemChecklistRepository _itens;
    private readonly IProcessoContratacaoRepository _processos;
    private readonly IContratoRepository _contratos;
    private readonly IDocumentoVersaoRepository _versoes;

    public ListVersoesItemUseCase(
        IItemChecklistRepository itens,
        IProcessoContratacaoRepository processos,
        IContratoRepository contratos,
        IDocumentoVersaoRepository versoes)
    {
        _itens = itens;
        _processos = processos;
        _contratos = contratos;
        _versoes = versoes;
    }

    public async Task<IReadOnlyList<DocumentoVersaoResponse>> ExecuteAsync(
        Guid itemId,
        Guid? scopeEmpresaId,
        CancellationToken cancellationToken = default)
    {
        var item = await _itens.GetByIdAsync(itemId, cancellationToken)
            ?? throw new DocumentoVersaoException("Item de checklist não encontrado.", 404);
        var processo = await _processos.GetByIdAsync(item.ProcessoId, cancellationToken)
            ?? throw new ProcessoException("Processo não encontrado.");
        var contrato = await _contratos.GetByIdAsync(processo.ContratoId, cancellationToken)
            ?? throw new ProcessoException("Contrato do processo não encontrado.");

        if (scopeEmpresaId is not null && contrato.EmpresaId != scopeEmpresaId.Value)
        {
            throw new EmpresaException("Sem permissão para este item.");
        }

        var versoes = await _versoes.ListByItemAsync(itemId, cancellationToken);
        var resultado = new List<DocumentoVersaoResponse>(versoes.Count);
        foreach (var versao in versoes)
        {
            var analises = await _versoes.ListAnalisesByVersaoAsync(versao.Id, cancellationToken);
            resultado.Add(DocumentoVersaoResponse.FromEntity(versao, analises));
        }

        return resultado;
    }
}

public sealed class GetVersaoDownloadUseCase
{
    private readonly IDocumentoVersaoRepository _versoes;
    private readonly IItemChecklistRepository _itens;
    private readonly IProcessoContratacaoRepository _processos;
    private readonly IContratoRepository _contratos;
    private readonly IObjectStorage _storage;

    public GetVersaoDownloadUseCase(
        IDocumentoVersaoRepository versoes,
        IItemChecklistRepository itens,
        IProcessoContratacaoRepository processos,
        IContratoRepository contratos,
        IObjectStorage storage)
    {
        _versoes = versoes;
        _itens = itens;
        _processos = processos;
        _contratos = contratos;
        _storage = storage;
    }

    public async Task<DocumentoDownloadResponse> ExecuteAsync(
        Guid versaoId,
        Guid? scopeEmpresaId,
        CancellationToken cancellationToken = default)
    {
        if (!_storage.IsConfigured)
        {
            throw new DocumentoVersaoException("Armazenamento de documentos não configurado (R2).");
        }

        var versao = await _versoes.GetByIdAsync(versaoId, cancellationToken)
            ?? throw new DocumentoVersaoException("Versão não encontrada.", 404);

        if (string.IsNullOrWhiteSpace(versao.StorageKey))
        {
            throw new DocumentoVersaoException("Esta versão não possui arquivo para download.");
        }

        var item = await _itens.GetByIdAsync(versao.ItemChecklistId, cancellationToken)
            ?? throw new DocumentoVersaoException("Item de checklist não encontrado.", 404);
        var processo = await _processos.GetByIdAsync(item.ProcessoId, cancellationToken)
            ?? throw new ProcessoException("Processo não encontrado.");
        var contrato = await _contratos.GetByIdAsync(processo.ContratoId, cancellationToken)
            ?? throw new ProcessoException("Contrato do processo não encontrado.");

        if (scopeEmpresaId is not null && contrato.EmpresaId != scopeEmpresaId.Value)
        {
            throw new EmpresaException("Sem permissão para este documento.");
        }

        var validFor = TimeSpan.FromMinutes(15);
        var url = await _storage.GetDownloadUrlAsync(versao.StorageKey, validFor, cancellationToken);
        return new DocumentoDownloadResponse(url, DateTime.UtcNow.Add(validFor));
    }
}
