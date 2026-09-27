using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class EventoAuditoriaDocumentoRepository : IEventoAuditoriaDocumentoRepository
{
    private readonly JotaNunesFormsDbContext _db;

    public EventoAuditoriaDocumentoRepository(JotaNunesFormsDbContext db) => _db = db;

    public async Task AddAsync(EventoAuditoriaDocumento evento, CancellationToken cancellationToken = default)
    {
        await _db.EventosAuditoriaDocumental.AddAsync(evento, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(EventoAuditoriaDocumento Evento, bool Inserido)> AddOrGetByBusinessKeyAsync(
        EventoAuditoriaDocumento evento,
        CancellationToken cancellationToken = default)
    {
        var inserted = await _db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO auditoria_eventos_documentais (
                id, chave_negocio, codigo, ocorreu_em, ator_tipo, ator_usuario_id, ator_nome, ator_perfil,
                escopo, origem_tipo, documento_id, versao_id, versao_numero, versao_anterior_id,
                versao_anterior_numero, empresa_id, empresa_razao_social, funcionario_id, funcionario_nome,
                processo_id, item_checklist_id, tipo_documento_rotulo, motivo, comentario, valido_ate)
            VALUES (
                {evento.Id}, {evento.ChaveNegocio}, {evento.Codigo.ToString()}, {evento.OcorreuEm},
                {evento.AtorTipo.ToString()}, {evento.AtorUsuarioId}, {evento.AtorNome}, {evento.AtorPerfil},
                {evento.Escopo.ToString()}, {evento.OrigemTipo.ToString()}, {evento.DocumentoId},
                {evento.VersaoId}, {evento.VersaoNumero}, {evento.VersaoAnteriorId}, {evento.VersaoAnteriorNumero},
                {evento.EmpresaId}, {evento.EmpresaRazaoSocial}, {evento.FuncionarioId}, {evento.FuncionarioNome},
                {evento.ProcessoId}, {evento.ItemChecklistId}, {evento.TipoDocumentoRotulo}, {evento.Motivo},
                {evento.Comentario}, {evento.ValidoAte})
            ON CONFLICT (chave_negocio) DO NOTHING
            """, cancellationToken);

        if (inserted == 1)
        {
            return (evento, true);
        }

        var existing = await _db.EventosAuditoriaDocumental
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.ChaveNegocio == evento.ChaveNegocio, cancellationToken);
        if (existing is null)
        {
            throw new InvalidOperationException("O evento concorrente não pôde ser recuperado.");
        }

        return (existing, false);
    }

    public Task<EventoAuditoriaDocumento?> GetByBusinessKeyAsync(
        string chaveNegocio,
        CancellationToken cancellationToken = default) =>
        _db.EventosAuditoriaDocumental
            .AsNoTracking()
            .FirstOrDefaultAsync(evento => evento.ChaveNegocio == chaveNegocio, cancellationToken);

    public async Task<EventoAuditoriaDocumentoPage> SearchAsync(
        EventoAuditoriaDocumentoQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<EventoAuditoriaDocumento> filtered = _db.EventosAuditoriaDocumental.AsNoTracking();
        if (query.De is not null)
        {
            filtered = filtered.Where(evento => evento.OcorreuEm >= query.De.Value);
        }

        if (query.Ate is not null)
        {
            filtered = filtered.Where(evento => evento.OcorreuEm < query.Ate.Value);
        }

        if (query.Codigo is not null)
        {
            filtered = filtered.Where(evento => evento.Codigo == query.Codigo.Value);
        }

        if (query.EmpresaId is not null)
        {
            filtered = filtered.Where(evento => evento.EmpresaId == query.EmpresaId.Value);
        }

        if (query.Escopo is not null)
        {
            filtered = filtered.Where(evento => evento.Escopo == query.Escopo.Value);
        }

        var total = await filtered.LongCountAsync(cancellationToken);
        var offset = checked((query.Page - 1) * query.PageSize);
        var items = await filtered
            .OrderByDescending(evento => evento.OcorreuEm)
            .ThenByDescending(evento => evento.Id)
            .Skip(offset)
            .Take(query.PageSize)
            .Select(evento => new EventoAuditoriaDocumentoSearchResult(
                evento,
                evento.OrigemTipo == OrigemDocumentoAuditoria.DocumentoEmpresa
                    ? _db.DocumentosEmpresa.Any(documento => documento.Id == evento.DocumentoId)
                    : evento.OrigemTipo == OrigemDocumentoAuditoria.DocumentoFuncionario
                        ? _db.DocumentosFuncionario.Any(documento => documento.Id == evento.DocumentoId)
                        : _db.ItensChecklist.Any(item => item.Id == evento.ItemChecklistId)))
            .ToListAsync(cancellationToken);

        return new EventoAuditoriaDocumentoPage(items, total);
    }
}
