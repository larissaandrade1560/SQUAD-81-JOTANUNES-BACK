namespace JotaNunesForms.Domain.Entities;

public sealed class ProcessoContratacao
{
    public Guid Id { get; private set; }

    public Guid ContratoId { get; private set; }

    public Guid AbertoPorUsuarioId { get; private set; }

    public DateTime AbertoEm { get; private set; }

    public string ServicoContratado { get; private set; } = string.Empty;

    public bool TransportaResiduos { get; private set; }

    public bool ControleTecnologico { get; private set; }

    public bool OptanteSimples { get; private set; }

    public bool ExigeArt { get; private set; }

    public int QuantidadeSociosInformada { get; private set; }

    public bool MobilizaTrabalhadores { get; private set; }

    public DateTime? EncaminhadoSetorContratosEm { get; private set; }

    public SituacaoProcesso Situacao { get; private set; }

    private ProcessoContratacao()
    {
    }

    public ProcessoContratacao(
        Guid contratoId,
        Guid abertoPorUsuarioId,
        string servicoContratado,
        bool transportaResiduos,
        bool controleTecnologico,
        bool optanteSimples,
        bool exigeArt,
        int quantidadeSociosInformada,
        bool mobilizaTrabalhadores)
    {
        if (contratoId == Guid.Empty)
        {
            throw new ArgumentException("Contrato é obrigatório.", nameof(contratoId));
        }

        if (abertoPorUsuarioId == Guid.Empty)
        {
            throw new ArgumentException("Responsável pela abertura é obrigatório.", nameof(abertoPorUsuarioId));
        }

        if (quantidadeSociosInformada < 0)
        {
            throw new ArgumentException("Quantidade de sócios não pode ser negativa.", nameof(quantidadeSociosInformada));
        }

        Id = Guid.NewGuid();
        ContratoId = contratoId;
        AbertoPorUsuarioId = abertoPorUsuarioId;
        AbertoEm = DateTime.UtcNow;
        ServicoContratado = NormalizeServico(servicoContratado);
        TransportaResiduos = transportaResiduos;
        ControleTecnologico = controleTecnologico;
        OptanteSimples = optanteSimples;
        ExigeArt = exigeArt;
        QuantidadeSociosInformada = quantidadeSociosInformada;
        MobilizaTrabalhadores = mobilizaTrabalhadores;
        Situacao = SituacaoProcesso.Aberto;
    }

    public void AtualizarQuestionario(
        string servicoContratado,
        bool transportaResiduos,
        bool controleTecnologico,
        bool optanteSimples,
        bool exigeArt,
        int quantidadeSociosInformada,
        bool mobilizaTrabalhadores)
    {
        if (quantidadeSociosInformada < 0)
        {
            throw new ArgumentException("Quantidade de sócios não pode ser negativa.", nameof(quantidadeSociosInformada));
        }

        ServicoContratado = NormalizeServico(servicoContratado);
        TransportaResiduos = transportaResiduos;
        ControleTecnologico = controleTecnologico;
        OptanteSimples = optanteSimples;
        ExigeArt = exigeArt;
        QuantidadeSociosInformada = quantidadeSociosInformada;
        MobilizaTrabalhadores = mobilizaTrabalhadores;
    }

    public void EncaminharSetorContratos() => EncaminhadoSetorContratosEm = DateTime.UtcNow;

    public void DefinirSituacao(SituacaoProcesso situacao) => Situacao = situacao;

    public bool CondicaoAtendida(CondicaoRequisito condicao) =>
        condicao switch
        {
            CondicaoRequisito.TransporteResiduos => TransportaResiduos,
            CondicaoRequisito.ControleTecnologico => ControleTecnologico,
            CondicaoRequisito.OptanteSimples => OptanteSimples,
            CondicaoRequisito.Art => ExigeArt,
            CondicaoRequisito.Mobilizacao => MobilizaTrabalhadores,
            _ => false,
        };

    private static string NormalizeServico(string servico)
    {
        if (string.IsNullOrWhiteSpace(servico))
        {
            throw new ArgumentException("Serviço contratado é obrigatório.", nameof(servico));
        }

        return servico.Trim();
    }
}
