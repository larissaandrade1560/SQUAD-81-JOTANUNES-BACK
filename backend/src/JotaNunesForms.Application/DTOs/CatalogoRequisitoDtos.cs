using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record CatalogoRequisitoResponse(
    Guid Id,
    string Codigo,
    string Nome,
    TitularRequisito Titular,
    AplicacaoRequisito Aplicacao,
    CondicaoRequisito? Condicao,
    TipoEntregaRequisito TipoEntrega,
    CamadaRequisito Camada,
    bool ExigeValidade,
    bool PermiteVencerComoDocumento,
    bool Ativo)
{
    public static CatalogoRequisitoResponse FromEntity(CatalogoRequisito requisito) =>
        new(
            requisito.Id,
            requisito.Codigo,
            requisito.Nome,
            requisito.Titular,
            requisito.Aplicacao,
            requisito.Condicao,
            requisito.TipoEntrega,
            requisito.Camada,
            requisito.ExigeValidade,
            requisito.PermiteVencerComoDocumento,
            requisito.Ativo);
}

public sealed record CreateCatalogoRequisitoRequest(
    string Codigo,
    string Nome,
    TitularRequisito Titular,
    AplicacaoRequisito Aplicacao,
    TipoEntregaRequisito TipoEntrega,
    CamadaRequisito Camada,
    CondicaoRequisito? Condicao,
    bool ExigeValidade,
    bool PermiteVencerComoDocumento);

public sealed record UpdateCatalogoRequisitoRequest(
    string Nome,
    bool Ativo,
    CondicaoRequisito? Condicao,
    bool ExigeValidade);

public sealed record ParametroNormativoResponse(string Chave, string Valor, string Unidade, DateTime AtualizadoEm)
{
    public static ParametroNormativoResponse FromEntity(ParametroNormativo parametro) =>
        new(parametro.Chave, parametro.Valor, parametro.Unidade, parametro.AtualizadoEm);
}

public sealed record UpdateParametroNormativoRequest(string Valor);

public sealed record EscopoArtResponse(Guid Id, string Nome, bool Ativo)
{
    public static EscopoArtResponse FromEntity(EscopoArt escopo) =>
        new(escopo.Id, escopo.Nome, escopo.Ativo);
}

public sealed record CatalogoCompletoResponse(
    IReadOnlyList<CatalogoRequisitoResponse> Requisitos,
    IReadOnlyList<ParametroNormativoResponse> Parametros);
