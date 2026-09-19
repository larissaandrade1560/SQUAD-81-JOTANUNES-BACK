using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class GetDocumentoFuncionarioDownloadUseCase
{
    private readonly IDocumentoFuncionarioRepository _documentos;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IObjectStorage _storage;

    public GetDocumentoFuncionarioDownloadUseCase(
        IDocumentoFuncionarioRepository documentos,
        IFuncionarioRepository funcionarios,
        IObjectStorage storage)
    {
        _documentos = documentos;
        _funcionarios = funcionarios;
        _storage = storage;
    }

    public async Task<DocumentoDownloadResponse> ExecuteAsync(
        Guid id,
        Guid? scopeEmpresaId,
        CancellationToken cancellationToken = default)
    {
        if (!_storage.IsConfigured)
        {
            throw new DocumentoFuncionarioException("Armazenamento de documentos não configurado (R2).");
        }

        var documento = await _documentos.GetByIdAsync(id, cancellationToken);
        if (documento is null)
        {
            throw new DocumentoFuncionarioException("Documento não encontrado.");
        }

        var funcionario = await _funcionarios.GetByIdAsync(documento.FuncionarioId, cancellationToken);
        if (funcionario is null)
        {
            throw new DocumentoFuncionarioException("Funcionário não encontrado.");
        }

        if (scopeEmpresaId is not null && funcionario.EmpresaId != scopeEmpresaId.Value)
        {
            throw new DocumentoFuncionarioException("Sem permissão para acessar este documento.");
        }

        var validFor = TimeSpan.FromMinutes(15);
        var url = await _storage.GetDownloadUrlAsync(documento.StorageKey, validFor, cancellationToken);
        return new DocumentoDownloadResponse(url, DateTime.UtcNow.Add(validFor));
    }
}
