using JotaNunesForms.Application.Documentos.Validadores;
using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.Tests;

public sealed class ValidacaoAdmissionalUseCaseTests
{
    [Fact]
    public void Dispatcher_ReconheceCodigosAdmissionais()
    {
        Assert.True(ValidadorAdmissionalDispatcher.EhCodigoAdmissional(CatalogoRequisitoCodigos.DocOficialFoto));
        Assert.False(ValidadorAdmissionalDispatcher.EhCodigoAdmissional(CatalogoRequisitoCodigos.MobCadastro));
    }
}
