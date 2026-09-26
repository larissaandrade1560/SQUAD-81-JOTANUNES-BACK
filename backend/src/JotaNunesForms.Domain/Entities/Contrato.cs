namespace JotaNunesForms.Domain.Entities;

public sealed class Contrato
{
    public Guid Id { get; private set; }

    public Guid EmpresaId { get; private set; }

    public Guid ObraId { get; private set; }

    public string EscopoServico { get; private set; } = string.Empty;

    public string? Numero { get; private set; }

    public SituacaoContrato Situacao { get; private set; }

    public DateOnly? Inicio { get; private set; }

    public DateOnly? Fim { get; private set; }

    public DateTime CriadoEm { get; private set; }

    private Contrato()
    {
    }

    public Contrato(Guid empresaId, Guid obraId, string escopoServico, string? numero = null)
    {
        if (empresaId == Guid.Empty)
        {
            throw new ArgumentException("Empresa é obrigatória.", nameof(empresaId));
        }

        if (obraId == Guid.Empty)
        {
            throw new ArgumentException("Obra é obrigatória.", nameof(obraId));
        }

        Id = Guid.NewGuid();
        EmpresaId = empresaId;
        ObraId = obraId;
        EscopoServico = NormalizeEscopo(escopoServico);
        Numero = NormalizeOptional(numero);
        Situacao = SituacaoContrato.Ativo;
        CriadoEm = DateTime.UtcNow;
    }

    public void Encerrar() => Situacao = SituacaoContrato.Encerrado;

    private static string NormalizeEscopo(string escopo)
    {
        if (string.IsNullOrWhiteSpace(escopo))
        {
            throw new ArgumentException("Escopo do serviço é obrigatório.", nameof(escopo));
        }

        return escopo.Trim();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
