namespace JotaNunesForms.Domain.Entities;

public sealed class CatalogoRequisito
{
    public Guid Id { get; private set; }

    public string Codigo { get; private set; } = string.Empty;

    public string Nome { get; private set; } = string.Empty;

    public TitularRequisito Titular { get; private set; }

    public AplicacaoRequisito Aplicacao { get; private set; }

    public CondicaoRequisito? Condicao { get; private set; }

    public TipoEntregaRequisito TipoEntrega { get; private set; }

    public CamadaRequisito Camada { get; private set; }

    public bool ExigeValidade { get; private set; }

    public bool PermiteVencerComoDocumento { get; private set; }

    public bool Ativo { get; private set; }

    private CatalogoRequisito()
    {
    }

    public CatalogoRequisito(
        string codigo,
        string nome,
        TitularRequisito titular,
        AplicacaoRequisito aplicacao,
        TipoEntregaRequisito tipoEntrega,
        CamadaRequisito camada,
        CondicaoRequisito? condicao = null,
        bool exigeValidade = true,
        bool permiteVencerComoDocumento = true)
        : this(
            Guid.NewGuid(),
            codigo,
            nome,
            titular,
            aplicacao,
            tipoEntrega,
            camada,
            condicao,
            exigeValidade,
            permiteVencerComoDocumento)
    {
    }

    public CatalogoRequisito(
        Guid id,
        string codigo,
        string nome,
        TitularRequisito titular,
        AplicacaoRequisito aplicacao,
        TipoEntregaRequisito tipoEntrega,
        CamadaRequisito camada,
        CondicaoRequisito? condicao = null,
        bool exigeValidade = true,
        bool permiteVencerComoDocumento = true)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id é obrigatório.", nameof(id));
        }

        Id = id;
        Codigo = NormalizeCodigo(codigo);
        Nome = NormalizeNome(nome);
        Titular = titular;
        Aplicacao = aplicacao;
        Condicao = condicao;
        TipoEntrega = tipoEntrega;
        Camada = camada;
        ExigeValidade = exigeValidade;
        PermiteVencerComoDocumento = permiteVencerComoDocumento;
        Ativo = true;
    }

    public void Atualizar(string nome, bool ativo, CondicaoRequisito? condicao, bool exigeValidade)
    {
        Nome = NormalizeNome(nome);
        Ativo = ativo;
        Condicao = condicao;
        ExigeValidade = exigeValidade;
    }

    private static string NormalizeCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("Código é obrigatório.", nameof(codigo));
        }

        return codigo.Trim().ToUpperInvariant();
    }

    private static string NormalizeNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome é obrigatório.", nameof(nome));
        }

        return nome.Trim();
    }
}
