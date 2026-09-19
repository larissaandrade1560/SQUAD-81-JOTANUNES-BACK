using JotaNunesForms.Application.UseCases.Formularios;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.Tests;

public sealed class ListFormulariosUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_MapsDomainEntitiesToResponses()
    {
        var repository = new FakeFormularioRepository(
        [
            new Formulario("Primeiro"),
            new Formulario("Segundo")
        ]);
        var useCase = new ListFormulariosUseCase(repository);

        var result = await useCase.ExecuteAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Primeiro", result[0].Titulo);
        Assert.Equal("Segundo", result[1].Titulo);
    }

    private sealed class FakeFormularioRepository : IFormularioRepository
    {
        private readonly IReadOnlyList<Formulario> _formularios;

        public FakeFormularioRepository(IReadOnlyList<Formulario> formularios)
        {
            _formularios = formularios;
        }

        public Task<IReadOnlyList<Formulario>> ListAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_formularios);
        }
    }
}
