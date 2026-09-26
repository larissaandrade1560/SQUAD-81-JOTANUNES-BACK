namespace JotaNunesForms.Domain.Entities;

public static class CatalogoMvpFactory
{
    public static IReadOnlyList<CatalogoRequisito> CriarRequisitos() =>
    [
        Req("aaaaaaaa-0001-0001-0001-000000000001", CatalogoRequisitoCodigos.ContratoSocial, "Contrato social/requerimento", TitularRequisito.Empresa, AplicacaoRequisito.Sempre, TipoEntregaRequisito.Upload, CamadaRequisito.Corporativa),
        Req("aaaaaaaa-0001-0001-0001-000000000002", CatalogoRequisitoCodigos.ComprovanteCnpj, "Comprovante de CNPJ", TitularRequisito.Empresa, AplicacaoRequisito.Sempre, TipoEntregaRequisito.Upload, CamadaRequisito.Corporativa),
        Req("aaaaaaaa-0001-0001-0001-000000000003", CatalogoRequisitoCodigos.EnderecoComercial, "Endereço comercial", TitularRequisito.Empresa, AplicacaoRequisito.Sempre, TipoEntregaRequisito.Upload, CamadaRequisito.Corporativa),
        Req("aaaaaaaa-0001-0001-0001-000000000004", CatalogoRequisitoCodigos.CrfFgts, "CRF/FGTS", TitularRequisito.Empresa, AplicacaoRequisito.Sempre, TipoEntregaRequisito.Upload, CamadaRequisito.Corporativa),
        Req("aaaaaaaa-0001-0001-0001-000000000005", CatalogoRequisitoCodigos.CndFederal, "CND federal", TitularRequisito.Empresa, AplicacaoRequisito.Sempre, TipoEntregaRequisito.Upload, CamadaRequisito.Corporativa),
        Req("aaaaaaaa-0001-0001-0001-000000000006", CatalogoRequisitoCodigos.OpcaoSimples, "Opção pelo Simples", TitularRequisito.Empresa, AplicacaoRequisito.Condicional, TipoEntregaRequisito.Upload, CamadaRequisito.Corporativa, CondicaoRequisito.OptanteSimples),
        Req("aaaaaaaa-0001-0001-0001-000000000007", CatalogoRequisitoCodigos.LicencaMunicipalResiduos, "Licença municipal (resíduos)", TitularRequisito.Empresa, AplicacaoRequisito.Condicional, TipoEntregaRequisito.Upload, CamadaRequisito.ContratoObra, CondicaoRequisito.TransporteResiduos),
        Req("aaaaaaaa-0001-0001-0001-000000000008", CatalogoRequisitoCodigos.CgcreInmetro, "CGCRE/INMETRO", TitularRequisito.Empresa, AplicacaoRequisito.Condicional, TipoEntregaRequisito.Upload, CamadaRequisito.ContratoObra, CondicaoRequisito.ControleTecnologico),
        Req("aaaaaaaa-0001-0001-0001-000000000009", CatalogoRequisitoCodigos.Art, "ART", TitularRequisito.Contrato, AplicacaoRequisito.Condicional, TipoEntregaRequisito.Upload, CamadaRequisito.ContratoObra, CondicaoRequisito.Art),
        Req("aaaaaaaa-0001-0001-0001-000000000010", CatalogoRequisitoCodigos.SocioIdentidadeCpf, "Identidade e CPF do sócio", TitularRequisito.Socio, AplicacaoRequisito.Sempre, TipoEntregaRequisito.Upload, CamadaRequisito.Corporativa),
        Req("aaaaaaaa-0001-0001-0001-000000000011", CatalogoRequisitoCodigos.SocioEndereco, "Endereço residencial do sócio", TitularRequisito.Socio, AplicacaoRequisito.Sempre, TipoEntregaRequisito.Upload, CamadaRequisito.Corporativa),
        Req("aaaaaaaa-0001-0001-0001-000000000012", CatalogoRequisitoCodigos.MobCadastro, "Cadastro de mobilização", TitularRequisito.Trabalhador, AplicacaoRequisito.Condicional, TipoEntregaRequisito.Formulario, CamadaRequisito.Admissional, CondicaoRequisito.Mobilizacao, exigeValidade: false, vence: false),
        Req("aaaaaaaa-0001-0001-0001-000000000013", CatalogoRequisitoCodigos.DocOficialFoto, "Documento oficial com foto", TitularRequisito.Trabalhador, AplicacaoRequisito.Condicional, TipoEntregaRequisito.Upload, CamadaRequisito.Admissional, CondicaoRequisito.Mobilizacao),
        Req("aaaaaaaa-0001-0001-0001-000000000014", CatalogoRequisitoCodigos.EsocialVinculo, "Comprovante de vínculo (eSocial)", TitularRequisito.Trabalhador, AplicacaoRequisito.Condicional, TipoEntregaRequisito.Upload, CamadaRequisito.Admissional, CondicaoRequisito.Mobilizacao),
        Req("aaaaaaaa-0001-0001-0001-000000000015", CatalogoRequisitoCodigos.AsoAdmissional, "ASO admissional", TitularRequisito.Trabalhador, AplicacaoRequisito.Condicional, TipoEntregaRequisito.Upload, CamadaRequisito.Admissional, CondicaoRequisito.Mobilizacao),
        Req("aaaaaaaa-0001-0001-0001-000000000016", CatalogoRequisitoCodigos.Nr18Basica, "Capacitação básica NR-18", TitularRequisito.Trabalhador, AplicacaoRequisito.Condicional, TipoEntregaRequisito.Upload, CamadaRequisito.Admissional, CondicaoRequisito.Mobilizacao),
        Req("aaaaaaaa-0001-0001-0001-000000000017", CatalogoRequisitoCodigos.OrdemServico, "Ordem de serviço de segurança", TitularRequisito.Trabalhador, AplicacaoRequisito.Condicional, TipoEntregaRequisito.Upload, CamadaRequisito.Admissional, CondicaoRequisito.Mobilizacao, exigeValidade: false),
        Req("aaaaaaaa-0001-0001-0001-000000000018", CatalogoRequisitoCodigos.EpiEntrega, "Registro de entrega de EPI", TitularRequisito.Trabalhador, AplicacaoRequisito.Condicional, TipoEntregaRequisito.Movimento, CamadaRequisito.Admissional, CondicaoRequisito.Mobilizacao, exigeValidade: false, vence: false),
        Req("aaaaaaaa-0001-0001-0001-000000000019", CatalogoRequisitoCodigos.IntegracaoObra, "Integração da obra", TitularRequisito.Trabalhador, AplicacaoRequisito.Condicional, TipoEntregaRequisito.Formulario, CamadaRequisito.Admissional, CondicaoRequisito.Mobilizacao),
    ];

    public static IReadOnlyList<ParametroNormativo> CriarParametros() =>
    [
        new(Guid.Parse("bbbbbbbb-0001-0001-0001-000000000001"), ParametroNormativoChaves.Nr18CargaHorariaInicial, "4", "horas"),
        new(Guid.Parse("bbbbbbbb-0001-0001-0001-000000000002"), ParametroNormativoChaves.Nr18PeriodicidadeMeses, "24", "meses"),
        new(Guid.Parse("bbbbbbbb-0001-0001-0001-000000000003"), ParametroNormativoChaves.S2190PrazoSubstituicaoDias, "30", "dias"),
        new(Guid.Parse("bbbbbbbb-0001-0001-0001-000000000004"), ParametroNormativoChaves.AsoPeriodicidadeDias, "365", "dias"),
    ];

    private static CatalogoRequisito Req(
        string id,
        string codigo,
        string nome,
        TitularRequisito titular,
        AplicacaoRequisito aplicacao,
        TipoEntregaRequisito entrega,
        CamadaRequisito camada,
        CondicaoRequisito? condicao = null,
        bool exigeValidade = true,
        bool vence = true) =>
        new(
            Guid.Parse(id),
            codigo,
            nome,
            titular,
            aplicacao,
            entrega,
            camada,
            condicao,
            exigeValidade,
            vence);
}
