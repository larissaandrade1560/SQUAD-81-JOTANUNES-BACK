using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.Documentos.Validadores;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Processos;
using JotaNunesForms.Application.UseCases.Auditoria;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Domain.Services;
using Microsoft.Extensions.Logging;

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
    private readonly AccessScopeGuard _scopeGuard;
    private readonly ITransactionalExecutor _transactions;
    private readonly IMobilizacaoRepository _mobilizacoes;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;
    private readonly ValidadorAdmissionalDispatcher _validadorAdmissional;
    private readonly AuditoriaDocumentoService _auditoria;
    private readonly ILogger<EnviarVersaoDocumentoUseCase> _logger;

    public EnviarVersaoDocumentoUseCase(
        IItemChecklistRepository itens,
        IProcessoContratacaoRepository processos,
        IContratoRepository contratos,
        ICatalogoRequisitoRepository catalogo,
        IDocumentoVersaoRepository versoes,
        IObjectStorage storage,
        AccessScopeGuard scopeGuard,
        ITransactionalExecutor transactions,
        IMobilizacaoRepository mobilizacoes,
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas,
        ValidadorAdmissionalDispatcher validadorAdmissional,
        AuditoriaDocumentoService auditoria,
        ILogger<EnviarVersaoDocumentoUseCase> logger)
    {
        _itens = itens;
        _processos = processos;
        _contratos = contratos;
        _catalogo = catalogo;
        _versoes = versoes;
        _storage = storage;
        _scopeGuard = scopeGuard;
        _transactions = transactions;
        _mobilizacoes = mobilizacoes;
        _funcionarios = funcionarios;
        _empresas = empresas;
        _validadorAdmissional = validadorAdmissional;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<DocumentoVersaoResponse> ExecuteAsync(
        Guid itemId,
        Guid enviadoPorUsuarioId,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        string? nomeArquivo,
        string? contentType,
        long? tamanhoBytes,
        Stream? conteudo,
        string? camposJson,
        CancellationToken cancellationToken = default)
    {
        var item = await _itens.GetByIdAsync(itemId, cancellationToken)
            ?? throw new DocumentoVersaoException("Recurso não encontrado.", 404);

        var processo = await _processos.GetByIdAsync(item.ProcessoId, cancellationToken)
            ?? throw new DocumentoVersaoException("Recurso não encontrado.", 404);
        var contrato = await _contratos.GetByIdAsync(processo.ContratoId, cancellationToken)
            ?? throw new DocumentoVersaoException("Recurso não encontrado.", 404);

        if (!await _scopeGuard.AllowsCompanyAsync(scope, contrato.EmpresaId, securityRequest, cancellationToken))
        {
            throw new DocumentoVersaoException("Recurso não encontrado.", 404);
        }

        if (!item.Ativo)
        {
            throw new DocumentoVersaoException("Item inativo não recebe novas versões.", 409);
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

        if (!string.IsNullOrWhiteSpace(camposJson)
            && item.TitularTipo == TitularRequisito.Trabalhador
            && item.TitularId is Guid mobilizacaoId
            && ValidadorAdmissionalDispatcher.EhCodigoAdmissional(requisito.Codigo))
        {
            camposJson = await ValidarAdmissionalAsync(
                requisito.Codigo,
                camposJson,
                mobilizacaoId,
                cancellationToken);
        }

        string? storageKey = null;
        string? hash = null;
        string? nome = null;
        string? tipo = null;
        long? tamanho = null;
        var versaoId = Guid.NewGuid();

        var uploaded = false;
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
            uploaded = true;
            nome = nomeArquivo;
            tipo = "application/pdf";
            tamanho = tamanhoBytes;
        }

        try
        {
            var versao = await _transactions.ExecuteAsync(async transactionToken =>
            {
                var currentItem = await _itens.GetByIdForUpdateAsync(item.Id, transactionToken)
                    ?? throw new DocumentoVersaoException("Recurso não encontrado.", 404);
                if (!currentItem.Ativo)
                {
                    throw new DocumentoVersaoException("Item inativo não recebe novas versões.", 409);
                }

                var existentes = await _versoes.ListByItemAsync(currentItem.Id, transactionToken);
                var anterior = existentes.FirstOrDefault(version => version.Vigente);
                var reenviar = existentes.Count > 0;
                if (reenviar && currentItem.Situacao is not SituacaoItemChecklist.Rejeitado and not SituacaoItemChecklist.Vencido)
                {
                    throw new DocumentoVersaoException(AuditoriaConflictException.StableCode, 409);
                }

                if (reenviar && anterior is null)
                {
                    throw new DocumentoVersaoException("Versão vigente não encontrada.", 409);
                }

                var numero = existentes.Count == 0 ? 1 : existentes.Max(version => version.Numero) + 1;
                var next = new DocumentoVersao(
                    currentItem.Id,
                    numero,
                    enviadoPorUsuarioId,
                    nome,
                    storageKey,
                    tipo,
                    tamanho,
                    hash,
                    camposJson,
                    versaoId);

                if (anterior is not null)
                {
                    anterior.MarcarNaoVigente();
                    await _versoes.UpdateAsync(anterior, transactionToken);
                }

                await _versoes.AddAsync(next, transactionToken);
                currentItem.DefinirSituacao(SituacaoItemChecklist.PendenteAnalise);
                await _itens.UpdateAsync(currentItem, transactionToken);
                await _auditoria.RegisterAsync(new RegistrarEventoAuditoriaDocumento(
                    reenviar ? CodigoAuditoriaDocumento.DocumentoReenviado : CodigoAuditoriaDocumento.DocumentoEnviado,
                    enviadoPorUsuarioId,
                    OrigemDocumentoAuditoria.DocumentoVersao,
                    currentItem.Id,
                    next.Id,
                    next.Numero,
                    anterior?.Id,
                    anterior?.Numero,
                    OcorreuEm: next.EnviadoEm), transactionToken);
                return next;
            }, cancellationToken);

            return DocumentoVersaoResponse.FromEntity(versao);
        }
        catch (Exception failure)
        {
            if (uploaded && storageKey is not null)
            {
                await TryCompensateAsync(storageKey);
            }

            if (failure is OperationCanceledException or DocumentoVersaoException)
            {
                throw;
            }

            if (failure is AuditoriaConflictException)
            {
                throw new DocumentoVersaoException(AuditoriaConflictException.StableCode, 409);
            }

            throw new DocumentoVersaoException("Não foi possível concluir o registro da versão.", 500);
        }
    }

    private async Task TryCompensateAsync(string storageKey)
    {
        try
        {
            await _storage.DeleteAsync(storageKey, CancellationToken.None);
        }
        catch (Exception failure)
        {
            _logger.LogWarning(
                "Compensating storage cleanup failed. FailureType={FailureType}",
                failure.GetType().Name);
        }
    }

    private async Task<string> ValidarAdmissionalAsync(
        string requisitoCodigo,
        string camposJson,
        Guid mobilizacaoId,
        CancellationToken cancellationToken)
    {
        var mobilizacao = await _mobilizacoes.GetByIdAsync(mobilizacaoId, cancellationToken)
            ?? throw new DocumentoVersaoException("Mobilização do item não encontrada.", 404);
        var funcionario = await _funcionarios.GetByIdAsync(mobilizacao.FuncionarioId, cancellationToken)
            ?? throw new DocumentoVersaoException("Trabalhador não encontrado.", 404);
        var empresa = await _empresas.GetByIdAsync(funcionario.EmpresaId, cancellationToken)
            ?? throw new DocumentoVersaoException("Empresa não encontrada.", 404);

        var resultado = await _validadorAdmissional.ValidarAsync(
            requisitoCodigo,
            camposJson,
            mobilizacao,
            funcionario,
            empresa,
            cancellationToken);

        if (!resultado.Valido)
        {
            var mensagem = resultado.Erros.Count == 1
                ? resultado.Erros[0].Mensagem
                : string.Join("; ", resultado.Erros.Select(e => $"{e.Campo}: {e.Mensagem}"));
            throw new DocumentoVersaoException(mensagem, 400);
        }

        return resultado.CamposJsonCanonico
            ?? throw new DocumentoVersaoException("Não foi possível canonicalizar os metadados.", 500);
    }
}

public sealed class ListVersoesItemUseCase
{
    private readonly IItemChecklistRepository _itens;
    private readonly IProcessoContratacaoRepository _processos;
    private readonly IContratoRepository _contratos;
    private readonly IDocumentoVersaoRepository _versoes;
    private readonly AccessScopeGuard _scopeGuard;

    public ListVersoesItemUseCase(
        IItemChecklistRepository itens,
        IProcessoContratacaoRepository processos,
        IContratoRepository contratos,
        IDocumentoVersaoRepository versoes,
        AccessScopeGuard scopeGuard)
    {
        _itens = itens;
        _processos = processos;
        _contratos = contratos;
        _versoes = versoes;
        _scopeGuard = scopeGuard;
    }

    public async Task<IReadOnlyList<DocumentoVersaoResponse>> ExecuteAsync(
        Guid itemId,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        CancellationToken cancellationToken = default)
    {
        var item = await _itens.GetByIdAsync(itemId, cancellationToken)
            ?? throw new DocumentoVersaoException("Recurso não encontrado.", 404);
        var processo = await _processos.GetByIdAsync(item.ProcessoId, cancellationToken)
            ?? throw new DocumentoVersaoException("Recurso não encontrado.", 404);
        var contrato = await _contratos.GetByIdAsync(processo.ContratoId, cancellationToken)
            ?? throw new DocumentoVersaoException("Recurso não encontrado.", 404);

        if (!await _scopeGuard.AllowsCompanyAsync(scope, contrato.EmpresaId, securityRequest, cancellationToken))
        {
            throw new DocumentoVersaoException("Recurso não encontrado.", 404);
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
    private readonly AccessScopeGuard _scopeGuard;

    public GetVersaoDownloadUseCase(
        IDocumentoVersaoRepository versoes,
        IItemChecklistRepository itens,
        IProcessoContratacaoRepository processos,
        IContratoRepository contratos,
        IObjectStorage storage,
        AccessScopeGuard scopeGuard)
    {
        _versoes = versoes;
        _itens = itens;
        _processos = processos;
        _contratos = contratos;
        _storage = storage;
        _scopeGuard = scopeGuard;
    }

    public async Task<DocumentoDownloadResponse> ExecuteAsync(
        Guid versaoId,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        CancellationToken cancellationToken = default)
    {
        var versao = await _versoes.GetByIdAsync(versaoId, cancellationToken)
            ?? throw new DocumentoVersaoException("Recurso não encontrado.", 404);

        var item = await _itens.GetByIdAsync(versao.ItemChecklistId, cancellationToken)
            ?? throw new DocumentoVersaoException("Recurso não encontrado.", 404);
        var processo = await _processos.GetByIdAsync(item.ProcessoId, cancellationToken)
            ?? throw new DocumentoVersaoException("Recurso não encontrado.", 404);
        var contrato = await _contratos.GetByIdAsync(processo.ContratoId, cancellationToken)
            ?? throw new DocumentoVersaoException("Recurso não encontrado.", 404);

        if (!await _scopeGuard.AllowsCompanyAsync(scope, contrato.EmpresaId, securityRequest, cancellationToken))
        {
            throw new DocumentoVersaoException("Recurso não encontrado.", 404);
        }

        if (!_storage.IsConfigured)
        {
            throw new DocumentoVersaoException("Armazenamento de documentos não configurado (R2).", 503);
        }

        if (string.IsNullOrWhiteSpace(versao.StorageKey))
        {
            throw new DocumentoVersaoException("Recurso não encontrado.", 404);
        }

        var validFor = TimeSpan.FromMinutes(15);
        var url = await _storage.GetDownloadUrlAsync(versao.StorageKey, validFor, cancellationToken);
        return new DocumentoDownloadResponse(url, DateTime.UtcNow.Add(validFor));
    }
}
