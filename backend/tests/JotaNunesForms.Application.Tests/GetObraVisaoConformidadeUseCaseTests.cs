using JotaNunesForms.Application.Obras;
using JotaNunesForms.Application.UseCases.Obras;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.Tests;

public sealed class GetObraVisaoConformidadeUseCaseTests
{
    private static readonly DateTime Agora = new(2026, 9, 28, 12, 0, 0, DateTimeKind.Utc);

    private readonly Obra _obra = new("Residencial Atlântico", "OB-019");
    private readonly Empresa _empresaA = new("Alfa Mão de Obra", "12345678000190", TipoEmpresa.MaoDeObra);
    private readonly Empresa _empresaB = new("Beta Serviços", "98765432000110", TipoEmpresa.MaoDeObra);

    [Fact]
    public async Task GroupsEmployeesByCompanyOrderedByName()
    {
        var zeca = new Funcionario(_empresaB.Id, "Zeca", "39053344705", "Pedreiro");
        var ana = new Funcionario(_empresaA.Id, "Ana", "52998224725", "Servente");
        var bruno = new Funcionario(_empresaA.Id, "Bruno", "11144477735", "Armador");

        var result = await CreateUseCase([zeca, ana, bruno]).ExecuteAsync(_obra.Id, Agora);

        Assert.Equal("OB-019", result.Obra.Codigo);
        Assert.Equal(3, result.TotalFuncionarios);
        Assert.Equal(new[] { "Alfa Mão de Obra", "Beta Serviços" }, result.Empresas.Select(e => e.RazaoSocial));
        Assert.Equal(new[] { "Ana", "Bruno" }, result.Empresas[0].Funcionarios.Select(f => f.Nome));
    }

    [Fact]
    public async Task EmptyObraReturnsNoGroups()
    {
        var result = await CreateUseCase([]).ExecuteAsync(_obra.Id, Agora);

        Assert.Equal(0, result.TotalFuncionarios);
        Assert.Empty(result.Empresas);
    }

    [Fact]
    public async Task UnknownObraThrows()
    {
        await Assert.ThrowsAsync<ObraException>(() => CreateUseCase([]).ExecuteAsync(Guid.NewGuid(), Agora));
    }

    [Fact]
    public async Task DocumentSituationUsesWorstStatus()
    {
        var semDocs = new Funcionario(_empresaA.Id, "Sem Docs", "52998224725", "Servente");
        var regular = new Funcionario(_empresaA.Id, "Regular", "11144477735", "Servente");
        var emAnalise = new Funcionario(_empresaA.Id, "Em Analise", "39053344705", "Servente");
        var irregular = new Funcionario(_empresaA.Id, "Irregular", "86288366757", "Servente");

        var documentos = new List<DocumentoFuncionario>
        {
            Aprovado(regular.Id),
            Aprovado(emAnalise.Id),
            Documento(emAnalise.Id),
            Aprovado(irregular.Id),
            Rejeitado(irregular.Id),
            Documento(irregular.Id),
        };

        var result = await CreateUseCase([semDocs, regular, emAnalise, irregular], documentos)
            .ExecuteAsync(_obra.Id, DateTime.UtcNow);
        var porNome = result.Empresas.Single().Funcionarios.ToDictionary(f => f.Nome, f => f.Documentos);

        Assert.Equal("sem_documentos", porNome["Sem Docs"].Situacao);
        Assert.Equal("regular", porNome["Regular"].Situacao);
        Assert.Equal("em_analise", porNome["Em Analise"].Situacao);
        Assert.Equal("irregular", porNome["Irregular"].Situacao);
        Assert.Equal(3, porNome["Irregular"].Total);
        Assert.Equal(1, porNome["Irregular"].Aprovados);
        Assert.Equal(1, porNome["Irregular"].EmAnalise);
        Assert.Equal(1, porNome["Irregular"].Irregulares);
    }

    [Fact]
    public async Task ExpiredApprovedDocumentCountsAsIrregularWithoutPersisting()
    {
        var funcionario = new Funcionario(_empresaA.Id, "Ana", "52998224725", "Servente");
        var documento = Aprovado(funcionario.Id);

        var result = await CreateUseCase([funcionario], [documento])
            .ExecuteAsync(_obra.Id, DateTime.UtcNow.AddDays(400));

        var resumo = result.Empresas.Single().Funcionarios.Single().Documentos;
        Assert.Equal("irregular", resumo.Situacao);
        Assert.Equal(StatusDocumento.Aprovado, documento.Status);
    }

    [Fact]
    public async Task PaymentSituationUsesWorstReceiptStatus()
    {
        var semPagamentos = new Funcionario(_empresaA.Id, "Sem Pagamentos", "52998224725", "Servente");
        var pendente = new Funcionario(_empresaA.Id, "Pendente", "11144477735", "Servente");
        var atrasado = new Funcionario(_empresaA.Id, "Atrasado", "39053344705", "Servente");
        var hoje = DateOnly.FromDateTime(Agora);

        var pagamentos = new List<PagamentoFuncionario>
        {
            new(pendente.Id, _empresaA.Id, new DateOnly(2026, 9, 1), hoje.AddDays(-1)),
            new(atrasado.Id, _empresaA.Id, new DateOnly(2026, 8, 1), hoje.AddDays(-10)),
            new(atrasado.Id, _empresaA.Id, new DateOnly(2026, 9, 1), hoje),
        };

        var result = await CreateUseCase([semPagamentos, pendente, atrasado], pagamentos: pagamentos)
            .ExecuteAsync(_obra.Id, Agora);
        var porNome = result.Empresas.Single().Funcionarios.ToDictionary(f => f.Nome, f => f.Pagamentos);

        Assert.Equal("sem_pagamentos", porNome["Sem Pagamentos"].Situacao);
        Assert.Equal("pendente", porNome["Pendente"].Situacao);
        Assert.Equal("em_atraso", porNome["Atrasado"].Situacao);
        Assert.Equal(2, porNome["Atrasado"].Total);
        Assert.Equal(1, porNome["Atrasado"].EmAtraso);
        Assert.Equal(1, porNome["Atrasado"].Pendentes);
    }

    private GetObraVisaoConformidadeUseCase CreateUseCase(
        IReadOnlyList<Funcionario> funcionarios,
        IReadOnlyList<DocumentoFuncionario>? documentos = null,
        IReadOnlyList<PagamentoFuncionario>? pagamentos = null) =>
        new(
            new FakeObraRepository(_obra),
            new FakeFuncionarioObraRepository(_obra.Id, funcionarios),
            new FakeEmpresaRepository([_empresaA, _empresaB]),
            new FakeDocumentoFuncionarioRepository(documentos ?? []),
            new FakePagamentoFuncionarioRepository(pagamentos ?? []));

    private static DocumentoFuncionario Documento(Guid funcionarioId) =>
        new(funcionarioId, TipoDocumentoFuncionario.Aso, "aso.pdf", $"key/{Guid.NewGuid()}", "application/pdf", 10);

    private static DocumentoFuncionario Aprovado(Guid funcionarioId)
    {
        var documento = Documento(funcionarioId);
        documento.Aprovar();
        return documento;
    }

    private static DocumentoFuncionario Rejeitado(Guid funcionarioId)
    {
        var documento = Documento(funcionarioId);
        documento.Rejeitar("Ilegível");
        return documento;
    }

    private sealed class FakeObraRepository(Obra seed) : IObraRepository
    {
        public Task<Obra?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(id == seed.Id ? seed : null);

        public Task<IReadOnlyList<Obra>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Obra>>([seed]);

        public Task<bool> ExistsCodigoAsync(
            string codigo,
            Guid? excludeObraId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task AddAsync(Obra obra, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(Obra obra, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeFuncionarioObraRepository(Guid obraId, IReadOnlyList<Funcionario> funcionarios)
        : IFuncionarioObraRepository
    {
        public Task<IReadOnlyDictionary<Guid, IReadOnlyList<Obra>>> ListObrasByFuncionarioIdsAsync(
            IReadOnlyCollection<Guid> funcionarioIds,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task ReplaceForFuncionarioAsync(
            Guid funcionarioId,
            IReadOnlyList<Guid> obraIds,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task EnsureVinculoAsync(
            Guid funcionarioId,
            Guid obraId,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<Funcionario>> ListFuncionariosByObraIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Funcionario>>(id == obraId ? funcionarios : []);
    }

    private sealed class FakeEmpresaRepository(IReadOnlyList<Empresa> seed) : IEmpresaRepository
    {
        public Task<Empresa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(seed.FirstOrDefault(e => e.Id == id));

        public Task<IReadOnlyList<Empresa>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(seed);

        public Task<bool> ExistsCnpjAsync(
            string cnpj,
            Guid? excludeEmpresaId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task AddAsync(Empresa empresa, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(Empresa empresa, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeDocumentoFuncionarioRepository(IReadOnlyList<DocumentoFuncionario> seed)
        : IDocumentoFuncionarioRepository
    {
        public Task<IReadOnlyList<DocumentoFuncionario>> ListByFuncionarioIdsAsync(
            IReadOnlyCollection<Guid> funcionarioIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DocumentoFuncionario>>(
                seed.Where(d => funcionarioIds.Contains(d.FuncionarioId)).ToList());

        public Task<DocumentoFuncionario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<DocumentoFuncionario?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<DocumentoFuncionario>> ListByFuncionarioAsync(
            Guid funcionarioId,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<DocumentoFuncionario>> ListAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<DocumentoFuncionario>> ListPendentesAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task AddAsync(DocumentoFuncionario documento, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task UpdateAsync(DocumentoFuncionario documento, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("A visão de conformidade não deve persistir documentos.");
    }

    private sealed class FakePagamentoFuncionarioRepository(IReadOnlyList<PagamentoFuncionario> seed)
        : IPagamentoFuncionarioRepository
    {
        public Task<IReadOnlyList<PagamentoFuncionario>> ListByFuncionarioIdsAsync(
            IReadOnlyCollection<Guid> funcionarioIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PagamentoFuncionario>>(
                seed.Where(p => funcionarioIds.Contains(p.FuncionarioId)).ToList());

        public Task<IReadOnlyList<PagamentoFuncionario>> ListAsync(
            Guid? empresaId = null,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<bool> ExistsCompetenciaAsync(
            Guid funcionarioId,
            DateOnly competencia,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task AddAsync(PagamentoFuncionario pagamento, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<PagamentoFuncionario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task UpdateAsync(PagamentoFuncionario pagamento, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
