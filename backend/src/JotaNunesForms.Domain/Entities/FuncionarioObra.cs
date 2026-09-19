namespace JotaNunesForms.Domain.Entities;

public sealed class FuncionarioObra
{
    public Guid FuncionarioId { get; private set; }

    public Guid ObraId { get; private set; }

    public DateTime VinculadoEm { get; private set; }

    private FuncionarioObra()
    {
    }

    public FuncionarioObra(Guid funcionarioId, Guid obraId)
    {
        if (funcionarioId == Guid.Empty)
        {
            throw new ArgumentException("Funcionário é obrigatório.", nameof(funcionarioId));
        }

        if (obraId == Guid.Empty)
        {
            throw new ArgumentException("Obra é obrigatória.", nameof(obraId));
        }

        FuncionarioId = funcionarioId;
        ObraId = obraId;
        VinculadoEm = DateTime.UtcNow;
    }
}
