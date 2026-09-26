namespace JotaNunesForms.Api.Tests.Security;

public sealed class AccessMatrixFixtureTests
{
    [Fact]
    public void MatrixIsValidAndContainsUniqueOperationsWithTenantDenialContracts()
    {
        var fixture = new AccessMatrixFixture();

        Assert.Equal(1, fixture.Matrix.SchemaVersion);
        Assert.NotEmpty(fixture.Matrix.Operations);
        Assert.Equal(69, fixture.Matrix.Operations.Count);
    }
}
