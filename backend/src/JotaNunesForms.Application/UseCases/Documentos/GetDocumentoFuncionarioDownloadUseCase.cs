using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class GetDocumentoFuncionarioDownloadUseCase
{
    private readonly IDocumentoFuncionarioRepository _documentos;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IObjectStorage _storage;
    private readonly AccessScopeGuard _scopeGuard;

    public GetDocumentoFuncionarioDownloadUseCase(
        IDocumentoFuncionarioRepository documentos,
        IFuncionarioRepository funcionarios,
        IObjectStorage storage,
        AccessScopeGuard scopeGuard)
    {
        _documentos = documentos;
        _funcionarios = funcionarios;
        _storage = storage;
        _scopeGuard = scopeGuard;
    }

    public async Task<DocumentoDownloadResponse> ExecuteAsync(
        Guid id,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        CancellationToken cancellationToken = default)
    {
        var documento = await _documentos.GetByIdAsync(id, cancellationToken);
        if (documento is null)
        {
            throw new DocumentoFuncionarioException("Recurso não encontrado.", 404);
        }

        var funcionario = await _funcionarios.GetByIdAsync(documento.FuncionarioId, cancellationToken);
        if (funcionario is null)
        {
            throw new DocumentoFuncionarioException("Recurso não encontrado.", 404);
        }

        if (scope is AccessScope.Company company && company.Type != TipoEmpresa.MaoDeObra)
        {
            throw new DocumentoFuncionarioException("Acesso não permitido.", 403);
        }

        if (!await _scopeGuard.AllowsCompanyAsync(scope, funcionario.EmpresaId, securityRequest, cancellationToken))
        {
            throw new DocumentoFuncionarioException("Recurso não encontrado.", 404);
        }

        if (!_storage.IsConfigured)
        {
            throw new DocumentoFuncionarioException("Armazenamento de documentos não configurado (R2).", 503);
        }

        var validFor = TimeSpan.FromMinutes(15);
        var url = await _storage.GetDownloadUrlAsync(documento.StorageKey, validFor, cancellationToken);
        return new DocumentoDownloadResponse(url, DateTime.UtcNow.Add(validFor));
    }
}
