using JotaNunesForms.Application.UseCases.Mobilizacoes;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Domain.Services;

namespace JotaNunesForms.Application.Tests;

public sealed class RecalcularLiberacaoMobilizacaoUseCaseTests
{
    [Fact]
    public async Task SynchronizesMobCadastroItem_WhenParentDataIsComplete()
    {
        var mobilizacaoId = Guid.NewGuid();
        var processoId = Guid.NewGuid();
        var mobCadastro = CatalogoMvpFactory.CriarRequisitos()
            .First(r => r.Codigo == CatalogoRequisitoCodigos.MobCadastro);
        var mobilizacao = CreateMobilizacao(mobilizacaoId, processoId);
        var funcionario = new Funcionario(Guid.NewGuid(), "João da Silva", "52998224725", "Pedreiro");
        var item = new ItemChecklist(processoId, mobCadastro.Id, TitularRequisito.Trabalhador, true, mobilizacaoId);

        var mobilizacoes = new FakeMobilizacaoRepository(mobilizacao);
        var itens = new FakeItemChecklistRepository([item]);
        var catalogo = new FakeCatalogoRepository([mobCadastro]);
        var useCase = BuildUseCase(mobilizacoes, itens, catalogo);

        var resultado = await useCase.ExecuteAsync(mobilizacaoId, funcionario);

        Assert.Equal(SituacaoItemChecklist.Aprovado, item.Situacao);
        Assert.Contains(resultado.Impedimentos, i => i.Codigo == CodigoImpedimentoLiberacao.IdentidadePendente);
    }

    [Fact]
    public async Task ReusesExistingTransaction_WhenExecutorAlreadyHasOne()
    {
        var executor = new RecordingTransactionalExecutor();
        var mobilizacaoId = Guid.NewGuid();
        var processoId = Guid.NewGuid();
        var mobilizacao = CreateMobilizacao(mobilizacaoId, processoId);
        var funcionario = new Funcionario(Guid.NewGuid(), "Maria", "39053344705", "Servente");
        var mobilizacoes = new FakeMobilizacaoRepository(mobilizacao);
        var itens = new FakeItemChecklistRepository([]);
        var catalogo = new FakeCatalogoRepository([]);
        var useCase = BuildUseCase(mobilizacoes, itens, catalogo, executor);

        await executor.ExecuteAsync(ct => useCase.ExecuteAsync(mobilizacaoId, funcionario, ct));

        Assert.Equal(1, executor.NestedInvocations);
    }

    [Fact]
    public async Task RollsBack_WhenPersistenceFails()
    {
        var mobilizacaoId = Guid.NewGuid();
        var processoId = Guid.NewGuid();
        var mobCadastro = CatalogoMvpFactory.CriarRequisitos()
            .First(r => r.Codigo == CatalogoRequisitoCodigos.MobCadastro);
        var mobilizacao = CreateMobilizacao(mobilizacaoId, processoId);
        var funcionario = new Funcionario(Guid.NewGuid(), "Maria", "39053344705", "Servente");
        var item = new ItemChecklist(processoId, mobCadastro.Id, TitularRequisito.Trabalhador, true, mobilizacaoId);
        var mobilizacoes = new FakeMobilizacaoRepository(mobilizacao);
        var itens = new ThrowingItemChecklistRepository([item]);
        var catalogo = new FakeCatalogoRepository([mobCadastro]);
        var useCase = BuildUseCase(mobilizacoes, itens, catalogo);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(mobilizacaoId, funcionario));
    }

    private static Mobilizacao CreateMobilizacao(Guid mobilizacaoId, Guid processoId)
    {
        var mobilizacao = new Mobilizacao(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            processoId,
            "Pedreiro");
        typeof(Mobilizacao).GetProperty(nameof(Mobilizacao.Id))!
            .SetValue(mobilizacao, mobilizacaoId);
        return mobilizacao;
    }

    private static RecalcularLiberacaoMobilizacaoUseCase BuildUseCase(
        IMobilizacaoRepository mobilizacoes,
        IItemChecklistRepository itens,
        ICatalogoRequisitoRepository catalogo,
        ITransactionalExecutor? executor = null)
    {
        var movimentos = new FakeMovimentoEpiRepository();
        var integracoes = new FakeIntegracaoObraRepository();
        return new RecalcularLiberacaoMobilizacaoUseCase(
            mobilizacoes,
            itens,
            catalogo,
            movimentos,
            integracoes,
            new LiberacaoSnapshotBuilder(catalogo, new FakeDocumentoVersaoRepository(), movimentos, integracoes),
            executor ?? new RecordingTransactionalExecutor());
    }

    private sealed class FakeMobilizacaoRepository : IMobilizacaoRepository
    {
        private readonly Mobilizacao _mobilizacao;
        private readonly bool _failOnUpdate;

        public FakeMobilizacaoRepository(Mobilizacao mobilizacao, bool failOnUpdate = false)
        {
            _mobilizacao = mobilizacao;
            _failOnUpdate = failOnUpdate;
        }

        public Task<Mobilizacao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Mobilizacao?>(id == _mobilizacao.Id ? _mobilizacao : null);

        public Task<Mobilizacao?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
            GetByIdAsync(id, cancellationToken);

        public Task<IReadOnlyList<Mobilizacao>> ListAsync(Guid? empresaId = null, Guid? obraId = null, Guid? contratoId = null, SituacaoMobilizacao? situacao = null, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Mobilizacao>>([_mobilizacao]);

        public Task AddAsync(Mobilizacao mobilizacao, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateAsync(Mobilizacao mobilizacao, CancellationToken cancellationToken = default)
        {
            if (_failOnUpdate)
            {
                throw new InvalidOperationException("falha de persistência");
            }

            return Task.CompletedTask;
        }

        public Task AddLotacaoAsync(HistoricoLotacao lotacao, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<HistoricoLotacao>> ListLotacoesAsync(Guid mobilizacaoId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<HistoricoLotacao>>([]);
    }

    private sealed class ThrowingItemChecklistRepository : FakeItemChecklistRepository
    {
        public ThrowingItemChecklistRepository(IEnumerable<ItemChecklist> items) : base(items)
        {
        }

        public override Task UpdateAsync(ItemChecklist item, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("falha de persistência");
    }

    private class FakeItemChecklistRepository : IItemChecklistRepository
    {
        private readonly List<ItemChecklist> _items;

        public FakeItemChecklistRepository(IEnumerable<ItemChecklist> items) => _items = items.ToList();

        public Task<ItemChecklist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.FirstOrDefault(i => i.Id == id));

        public Task<ItemChecklist?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
            GetByIdAsync(id, cancellationToken);

        public Task<IReadOnlyList<ItemChecklist>> ListByProcessoAsync(Guid processoId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ItemChecklist>>(_items.Where(i => i.ProcessoId == processoId).ToList());

        public Task<IReadOnlyList<ItemChecklist>> ListActiveWorkerItemsByMobilizacaoAsync(Guid mobilizacaoId, Guid processoId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ItemChecklist>>(_items.Where(i => i.TitularId == mobilizacaoId).ToList());

        public Task AddRangeAsync(IEnumerable<ItemChecklist> itens, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public virtual Task UpdateAsync(ItemChecklist item, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeCatalogoRepository : ICatalogoRequisitoRepository
    {
        private readonly IReadOnlyList<CatalogoRequisito> _items;

        public FakeCatalogoRepository(IReadOnlyList<CatalogoRequisito> items) => _items = items;

        public Task<CatalogoRequisito?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.FirstOrDefault(i => i.Id == id));

        public Task<CatalogoRequisito?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.FirstOrDefault(i => i.Codigo == codigo));

        public Task<IReadOnlyList<CatalogoRequisito>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_items);

        public Task AddAsync(CatalogoRequisito requisito, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateAsync(CatalogoRequisito requisito, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<ParametroNormativo>> ListParametrosAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ParametroNormativo>>([]);

        public Task<ParametroNormativo?> GetParametroByChaveAsync(string chave, CancellationToken cancellationToken = default) =>
            Task.FromResult<ParametroNormativo?>(null);

        public Task AddParametroAsync(ParametroNormativo parametro, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateParametroAsync(ParametroNormativo parametro, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<EscopoArt>> ListEscoposArtAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<EscopoArt>>([]);
    }

    private sealed class FakeMovimentoEpiRepository : IMovimentoEpiRepository
    {
        public Task<MovimentoEpi?> GetByMobilizacaoAndIdempotencyKeyAsync(Guid mobilizacaoId, string idempotencyKey, CancellationToken cancellationToken = default) =>
            Task.FromResult<MovimentoEpi?>(null);

        public Task<IReadOnlyList<MovimentoEpi>> ListByMobilizacaoAsync(Guid mobilizacaoId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<MovimentoEpi>>([]);

        public Task AddAsync(MovimentoEpi movimento, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeIntegracaoObraRepository : IIntegracaoObraRepository
    {
        public Task<IntegracaoObra?> GetByMobilizacaoAndIdempotencyKeyAsync(Guid mobilizacaoId, string idempotencyKey, CancellationToken cancellationToken = default) =>
            Task.FromResult<IntegracaoObra?>(null);

        public Task<IReadOnlyList<IntegracaoObra>> ListByMobilizacaoAsync(Guid mobilizacaoId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<IntegracaoObra>>([]);

        public Task AddAsync(IntegracaoObra integracao, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateAsync(IntegracaoObra integracao, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeDocumentoVersaoRepository : IDocumentoVersaoRepository
    {
        public Task<DocumentoVersao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<DocumentoVersao?>(null);

        public Task<DocumentoVersao?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<DocumentoVersao?>(null);

        public Task<IReadOnlyList<DocumentoVersao>> ListByItemAsync(Guid itemChecklistId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DocumentoVersao>>([]);

        public Task<IReadOnlyList<DocumentoVersao>> ListVigentesPendentesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DocumentoVersao>>([]);

        public Task AddAsync(DocumentoVersao versao, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateAsync(DocumentoVersao versao, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task AddAnaliseAsync(AnaliseDocumento analise, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<AnaliseDocumento>> ListAnalisesByVersaoAsync(Guid documentoVersaoId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AnaliseDocumento>>([]);
    }

    private sealed class RecordingTransactionalExecutor : ITransactionalExecutor
    {
        public int NestedInvocations { get; private set; }

        public Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
        {
            NestedInvocations++;
            return operation(cancellationToken);
        }
    }
}
