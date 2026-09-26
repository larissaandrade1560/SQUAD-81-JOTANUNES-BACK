using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record CreateContratoRequest(
    Guid EmpresaId,
    Guid ObraId,
    string EscopoServico,
    string? Numero);

public sealed record ContratoResponse(
    Guid Id,
    Guid EmpresaId,
    Guid ObraId,
    string EscopoServico,
    string? Numero,
    SituacaoContrato Situacao,
    DateTime CriadoEm)
{
    public static ContratoResponse FromEntity(Contrato contrato) =>
        new(
            contrato.Id,
            contrato.EmpresaId,
            contrato.ObraId,
            contrato.EscopoServico,
            contrato.Numero,
            contrato.Situacao,
            contrato.CriadoEm);
}

public sealed record CreateProcessoContratacaoRequest(
    Guid EmpresaId,
    Guid ObraId,
    Guid? ContratoId,
    string ServicoContratado,
    bool TransportaResiduos,
    bool ControleTecnologico,
    bool OptanteSimples,
    bool ExigeArt,
    int QuantidadeSocios,
    bool MobilizaTrabalhadores,
    string? NumeroContrato);

public sealed record UpdateProcessoContratacaoRequest(
    string ServicoContratado,
    bool TransportaResiduos,
    bool ControleTecnologico,
    bool OptanteSimples,
    bool ExigeArt,
    int QuantidadeSocios,
    bool MobilizaTrabalhadores);

public sealed record ItemChecklistResponse(
    Guid Id,
    Guid CatalogoRequisitoId,
    string Codigo,
    string Nome,
    TitularRequisito TitularTipo,
    Guid? TitularId,
    int? TitularOrdem,
    bool Obrigatorio,
    bool Ativo,
    SituacaoItemChecklist Situacao);

public sealed record ProcessoContratacaoResponse(
    Guid Id,
    Guid ContratoId,
    Guid EmpresaId,
    Guid ObraId,
    string ServicoContratado,
    bool TransportaResiduos,
    bool ControleTecnologico,
    bool OptanteSimples,
    bool ExigeArt,
    int QuantidadeSociosInformada,
    bool MobilizaTrabalhadores,
    DateTime AbertoEm,
    DateTime? EncaminhadoSetorContratosEm,
    SituacaoProcesso Situacao,
    IReadOnlyList<ItemChecklistResponse> Checklist);
