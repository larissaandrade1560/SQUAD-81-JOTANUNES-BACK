using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Auditoria;

public sealed class AuditoriaConsultaException : Exception
{
    public AuditoriaConsultaException(string message) : base(message)
    {
    }
}

public sealed class ListEventosAuditoriaDocumentoUseCase
{
    private readonly IEventoAuditoriaDocumentoRepository _eventos;

    public ListEventosAuditoriaDocumentoUseCase(IEventoAuditoriaDocumentoRepository eventos) => _eventos = eventos;

    public async Task<AuditoriaEventosResponse> ExecuteAsync(
        AuditoriaEventosRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.De is not null && request.Ate is not null && request.De >= request.Ate)
        {
            throw new AuditoriaConsultaException("O instante inicial deve ser anterior ao instante final.");
        }

        if (request.Page < 1)
        {
            throw new AuditoriaConsultaException("A página deve ser maior ou igual a 1.");
        }

        if (request.PageSize is < 1 or > 100)
        {
            throw new AuditoriaConsultaException("O tamanho da página deve estar entre 1 e 100.");
        }

        if ((long)(request.Page - 1) * request.PageSize > int.MaxValue)
        {
            throw new AuditoriaConsultaException("A página solicitada excede o limite suportado.");
        }

        var query = new EventoAuditoriaDocumentoQuery(
            request.De?.UtcDateTime,
            request.Ate?.UtcDateTime,
            ParseCodigo(request.Codigo),
            request.EmpresaId,
            ParseEscopo(request.Escopo),
            request.Page,
            request.PageSize);
        var page = await _eventos.SearchAsync(query, cancellationToken);
        var totalPages = page.Total == 0 ? 0 : checked((int)Math.Ceiling(page.Total / (double)request.PageSize));

        return new AuditoriaEventosResponse(
            page.Items.Select(result => Map(result.Evento, result.OrigemDisponivel)).ToArray(),
            request.Page,
            request.PageSize,
            page.Total,
            totalPages);
    }

    private static CodigoAuditoriaDocumento? ParseCodigo(string? code) => code switch
    {
        null or "" => null,
        "documento_enviado" => CodigoAuditoriaDocumento.DocumentoEnviado,
        "documento_reenviado" => CodigoAuditoriaDocumento.DocumentoReenviado,
        "documento_aprovado" => CodigoAuditoriaDocumento.DocumentoAprovado,
        "documento_rejeitado" => CodigoAuditoriaDocumento.DocumentoRejeitado,
        "documento_vencido" => CodigoAuditoriaDocumento.DocumentoVencido,
        _ => throw new AuditoriaConsultaException("O código de ação informado é inválido."),
    };

    private static EscopoAuditoriaDocumento? ParseEscopo(string? scope) => scope switch
    {
        null or "" => null,
        "empresa" => EscopoAuditoriaDocumento.Empresa,
        "funcionario" => EscopoAuditoriaDocumento.Funcionario,
        "requisito" => EscopoAuditoriaDocumento.Requisito,
        _ => throw new AuditoriaConsultaException("O escopo informado é inválido."),
    };

    private static AuditoriaEventoResponse Map(EventoAuditoriaDocumento evento, bool sourceAvailable) =>
        new(
            evento.Id,
            PublicCode(evento.Codigo),
            ActionLabel(evento.Codigo),
            new DateTimeOffset(DateTime.SpecifyKind(evento.OcorreuEm, DateTimeKind.Utc)),
            PublicScope(evento.Escopo),
            ScopeLabel(evento.Escopo),
            evento.Automatico,
            new AuditoriaAtorSnapshot(evento.AtorUsuarioId, evento.AtorNome, evento.AtorPerfil),
            new AuditoriaEmpresaSnapshot(evento.EmpresaId, evento.EmpresaRazaoSocial),
            evento.FuncionarioId is Guid employeeId
                ? new AuditoriaFuncionarioSnapshot(employeeId, evento.FuncionarioNome ?? string.Empty)
                : null,
            new AuditoriaDocumentoReferencia(
                evento.DocumentoId,
                evento.TipoDocumentoRotulo,
                evento.VersaoId,
                evento.VersaoNumero,
                evento.VersaoAnteriorId,
                evento.VersaoAnteriorNumero),
            new AuditoriaOrigemReferencia(sourceAvailable, evento.ProcessoId, evento.ItemChecklistId),
            new AuditoriaDetalhes(
                evento.Motivo,
                evento.Comentario,
                evento.ValidoAte is DateTime validUntil
                    ? new DateTimeOffset(DateTime.SpecifyKind(validUntil, DateTimeKind.Utc))
                    : null));

    private static string PublicCode(CodigoAuditoriaDocumento code) => code switch
    {
        CodigoAuditoriaDocumento.DocumentoEnviado => "documento_enviado",
        CodigoAuditoriaDocumento.DocumentoReenviado => "documento_reenviado",
        CodigoAuditoriaDocumento.DocumentoAprovado => "documento_aprovado",
        CodigoAuditoriaDocumento.DocumentoRejeitado => "documento_rejeitado",
        CodigoAuditoriaDocumento.DocumentoVencido => "documento_vencido",
        _ => throw new ArgumentOutOfRangeException(nameof(code)),
    };

    private static string ActionLabel(CodigoAuditoriaDocumento code) => code switch
    {
        CodigoAuditoriaDocumento.DocumentoEnviado => "Documento enviado",
        CodigoAuditoriaDocumento.DocumentoReenviado => "Documento reenviado",
        CodigoAuditoriaDocumento.DocumentoAprovado => "Documento aprovado",
        CodigoAuditoriaDocumento.DocumentoRejeitado => "Documento rejeitado",
        CodigoAuditoriaDocumento.DocumentoVencido => "Documento vencido",
        _ => throw new ArgumentOutOfRangeException(nameof(code)),
    };

    private static string PublicScope(EscopoAuditoriaDocumento scope) => scope switch
    {
        EscopoAuditoriaDocumento.Empresa => "empresa",
        EscopoAuditoriaDocumento.Funcionario => "funcionario",
        EscopoAuditoriaDocumento.Requisito => "requisito",
        _ => throw new ArgumentOutOfRangeException(nameof(scope)),
    };

    private static string ScopeLabel(EscopoAuditoriaDocumento scope) => scope switch
    {
        EscopoAuditoriaDocumento.Empresa => "Empresa",
        EscopoAuditoriaDocumento.Funcionario => "Funcionário",
        EscopoAuditoriaDocumento.Requisito => "Requisito",
        _ => throw new ArgumentOutOfRangeException(nameof(scope)),
    };
}
