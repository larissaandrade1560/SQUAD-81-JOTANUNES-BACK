using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Auditoria;

public sealed record RegistrarEventoAuditoriaDocumento(
    CodigoAuditoriaDocumento Codigo,
    Guid? AtorUsuarioId,
    OrigemDocumentoAuditoria OrigemTipo,
    Guid DocumentoId,
    Guid VersaoId,
    int VersaoNumero,
    Guid? VersaoAnteriorId = null,
    int? VersaoAnteriorNumero = null,
    string? Motivo = null,
    string? Comentario = null,
    DateTime? ValidoAte = null,
    DateTime? OcorreuEm = null);

public sealed record AuditoriaRegistroResult(EventoAuditoriaDocumento Evento, bool Inserido);

public sealed class AuditoriaConflictException : Exception
{
    public const string StableCode = "documento_estado_conflitante";

    public AuditoriaConflictException()
        : base(StableCode)
    {
    }
}

public sealed class AuditoriaDocumentoService
{
    private readonly IEventoAuditoriaDocumentoRepository _eventos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IEmpresaRepository _empresas;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IDocumentoEmpresaRepository _documentosEmpresa;
    private readonly IDocumentoFuncionarioRepository _documentosFuncionario;
    private readonly IItemChecklistRepository _itens;
    private readonly IProcessoContratacaoRepository _processos;
    private readonly IContratoRepository _contratos;
    private readonly ICatalogoRequisitoRepository _catalogo;

    public AuditoriaDocumentoService(
        IEventoAuditoriaDocumentoRepository eventos,
        IUsuarioRepository usuarios,
        IEmpresaRepository empresas,
        IFuncionarioRepository funcionarios,
        IDocumentoEmpresaRepository documentosEmpresa,
        IDocumentoFuncionarioRepository documentosFuncionario,
        IItemChecklistRepository itens,
        IProcessoContratacaoRepository processos,
        IContratoRepository contratos,
        ICatalogoRequisitoRepository catalogo)
    {
        _eventos = eventos;
        _usuarios = usuarios;
        _empresas = empresas;
        _funcionarios = funcionarios;
        _documentosEmpresa = documentosEmpresa;
        _documentosFuncionario = documentosFuncionario;
        _itens = itens;
        _processos = processos;
        _contratos = contratos;
        _catalogo = catalogo;
    }

    public async Task<AuditoriaRegistroResult> RegisterAsync(
        RegistrarEventoAuditoriaDocumento request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var actor = request.AtorUsuarioId is Guid actorId
            ? await _usuarios.GetByIdAsync(actorId, cancellationToken)
            : null;
        if (request.AtorUsuarioId is not null && (actor is null || !actor.Ativo))
        {
            throw new InvalidOperationException("A identidade do ator não está ativa.");
        }

        var context = await ResolveContextAsync(request, cancellationToken);
        var now = request.OcorreuEm ?? DateTime.UtcNow;
        var auditEvent = EventoAuditoriaDocumento.Create(
            chaveNegocio: BuildBusinessKey(request.Codigo, request.OrigemTipo, request.VersaoId),
            codigo: request.Codigo,
            ocorreuEm: now,
            atorTipo: actor is null ? TipoAtorAuditoria.Sistema : TipoAtorAuditoria.Usuario,
            atorUsuarioId: actor?.Id,
            atorNome: actor?.NomeExibicao ?? "Sistema",
            atorPerfil: actor?.PerfilRotulo,
            escopo: context.Escopo,
            origemTipo: request.OrigemTipo,
            documentoId: request.DocumentoId,
            versaoId: request.VersaoId,
            versaoNumero: request.VersaoNumero,
            versaoAnteriorId: request.VersaoAnteriorId,
            versaoAnteriorNumero: request.VersaoAnteriorNumero,
            empresaId: context.Empresa.Id,
            empresaRazaoSocial: context.Empresa.RazaoSocial,
            funcionarioId: context.Funcionario?.Id,
            funcionarioNome: context.Funcionario?.Nome,
            processoId: context.ProcessoId,
            itemChecklistId: context.ItemChecklistId,
            tipoDocumentoRotulo: context.TipoDocumentoRotulo,
            motivo: request.Motivo,
            comentario: request.Comentario,
            validoAte: request.ValidoAte);

        var (saved, inserted) = await _eventos.AddOrGetByBusinessKeyAsync(auditEvent, cancellationToken);
        if (!inserted && saved.Codigo != request.Codigo)
        {
            throw new AuditoriaConflictException();
        }

        return new AuditoriaRegistroResult(saved, inserted);
    }

    public static string BuildBusinessKey(
        CodigoAuditoriaDocumento codigo,
        OrigemDocumentoAuditoria origemTipo,
        Guid versaoId)
    {
        var prefix = codigo switch
        {
            CodigoAuditoriaDocumento.DocumentoEnviado => "documento_enviado",
            CodigoAuditoriaDocumento.DocumentoReenviado => "documento_reenviado",
            CodigoAuditoriaDocumento.DocumentoAprovado or CodigoAuditoriaDocumento.DocumentoRejeitado => "documento_analisado",
            CodigoAuditoriaDocumento.DocumentoVencido => "documento_vencido",
            _ => throw new ArgumentOutOfRangeException(nameof(codigo)),
        };

        return $"{prefix}:{origemTipo}:{versaoId:N}";
    }

    public Task<EventoAuditoriaDocumento?> FindExistingTransitionAsync(
        CodigoAuditoriaDocumento codigo,
        OrigemDocumentoAuditoria origemTipo,
        Guid versaoId,
        CancellationToken cancellationToken = default) =>
        _eventos.GetByBusinessKeyAsync(BuildBusinessKey(codigo, origemTipo, versaoId), cancellationToken);

    private async Task<AuditContext> ResolveContextAsync(
        RegistrarEventoAuditoriaDocumento request,
        CancellationToken cancellationToken)
    {
        switch (request.OrigemTipo)
        {
            case OrigemDocumentoAuditoria.DocumentoEmpresa:
            {
                var document = await _documentosEmpresa.GetByIdAsync(request.DocumentoId, cancellationToken)
                    ?? throw new InvalidOperationException("Documento empresarial não encontrado para auditoria.");
                var company = await _empresas.GetByIdAsync(document.EmpresaId, cancellationToken)
                    ?? throw new InvalidOperationException("Empresa não encontrada para auditoria.");
                return new AuditContext(
                    EscopoAuditoriaDocumento.Empresa,
                    company,
                    null,
                    null,
                    null,
                    DocumentoEmpresaResponse.TipoLabel(document.Tipo));
            }
            case OrigemDocumentoAuditoria.DocumentoFuncionario:
            {
                var document = await _documentosFuncionario.GetByIdAsync(request.DocumentoId, cancellationToken)
                    ?? throw new InvalidOperationException("Documento do funcionário não encontrado para auditoria.");
                var employee = await _funcionarios.GetByIdAsync(document.FuncionarioId, cancellationToken)
                    ?? throw new InvalidOperationException("Funcionário não encontrado para auditoria.");
                var company = await _empresas.GetByIdAsync(employee.EmpresaId, cancellationToken)
                    ?? throw new InvalidOperationException("Empresa não encontrada para auditoria.");
                return new AuditContext(
                    EscopoAuditoriaDocumento.Funcionario,
                    company,
                    employee,
                    null,
                    null,
                    DocumentoFuncionarioResponse.TipoLabel(document.Tipo));
            }
            case OrigemDocumentoAuditoria.DocumentoVersao:
            {
                var item = await _itens.GetByIdAsync(request.DocumentoId, cancellationToken)
                    ?? throw new InvalidOperationException("Item de checklist não encontrado para auditoria.");
                var processo = await _processos.GetByIdAsync(item.ProcessoId, cancellationToken)
                    ?? throw new InvalidOperationException("Processo não encontrado para auditoria.");
                var contrato = await _contratos.GetByIdAsync(processo.ContratoId, cancellationToken)
                    ?? throw new InvalidOperationException("Contrato não encontrado para auditoria.");
                var company = await _empresas.GetByIdAsync(contrato.EmpresaId, cancellationToken)
                    ?? throw new InvalidOperationException("Empresa não encontrada para auditoria.");
                var requirement = await _catalogo.GetByIdAsync(item.CatalogoRequisitoId, cancellationToken);
                var employee = item.TitularTipo == TitularRequisito.Trabalhador && item.TitularId is Guid titularId
                    ? await _funcionarios.GetByIdAsync(titularId, cancellationToken)
                    : null;
                if (item.TitularTipo == TitularRequisito.Trabalhador && employee is null)
                {
                    throw new InvalidOperationException("Funcionário titular do requisito não encontrado para auditoria.");
                }

                return new AuditContext(
                    employee is null ? EscopoAuditoriaDocumento.Requisito : EscopoAuditoriaDocumento.Funcionario,
                    company,
                    employee,
                    processo.Id,
                    item.Id,
                    requirement?.Nome ?? "Requisito documental");
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(request.OrigemTipo));
        }
    }

    private sealed record AuditContext(
        EscopoAuditoriaDocumento Escopo,
        Empresa Empresa,
        Funcionario? Funcionario,
        Guid? ProcessoId,
        Guid? ItemChecklistId,
        string TipoDocumentoRotulo);
}
