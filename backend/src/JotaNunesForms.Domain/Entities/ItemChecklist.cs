namespace JotaNunesForms.Domain.Entities;

public sealed class ItemChecklist
{
    public Guid Id { get; private set; }

    public Guid ProcessoId { get; private set; }

    public Guid CatalogoRequisitoId { get; private set; }

    public TitularRequisito TitularTipo { get; private set; }

    public Guid? TitularId { get; private set; }

    public int? TitularOrdem { get; private set; }

    public bool Obrigatorio { get; private set; }

    public bool Ativo { get; private set; }

    public SituacaoItemChecklist Situacao { get; private set; }

    private ItemChecklist()
    {
    }

    public ItemChecklist(
        Guid processoId,
        Guid catalogoRequisitoId,
        TitularRequisito titularTipo,
        bool obrigatorio,
        Guid? titularId = null,
        int? titularOrdem = null)
    {
        if (processoId == Guid.Empty)
        {
            throw new ArgumentException("Processo é obrigatório.", nameof(processoId));
        }

        if (catalogoRequisitoId == Guid.Empty)
        {
            throw new ArgumentException("Requisito é obrigatório.", nameof(catalogoRequisitoId));
        }

        Id = Guid.NewGuid();
        ProcessoId = processoId;
        CatalogoRequisitoId = catalogoRequisitoId;
        TitularTipo = titularTipo;
        TitularId = titularId;
        TitularOrdem = titularOrdem;
        Obrigatorio = obrigatorio;
        Ativo = true;
        Situacao = SituacaoItemChecklist.NaoEnviado;
    }

    public void Desativar() => Ativo = false;

    public void Reativar() => Ativo = true;

    public void DefinirSituacao(SituacaoItemChecklist situacao) => Situacao = situacao;

    public void VincularTitular(Guid titularId) => TitularId = titularId;

    public bool MesmaChave(Guid catalogoRequisitoId, TitularRequisito titular, int? ordem) =>
        CatalogoRequisitoId == catalogoRequisitoId
        && TitularTipo == titular
        && TitularOrdem == ordem;
}
