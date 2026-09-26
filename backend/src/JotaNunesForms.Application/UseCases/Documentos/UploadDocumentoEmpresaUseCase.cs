using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class UploadDocumentoEmpresaUseCase
{
    private const long MaxBytes = 10 * 1024 * 1024;

    private readonly IDocumentoEmpresaRepository _documentos;
    private readonly IEmpresaRepository _empresas;
    private readonly IObjectStorage _storage;

    public UploadDocumentoEmpresaUseCase(
        IDocumentoEmpresaRepository documentos,
        IEmpresaRepository empresas,
        IObjectStorage storage)
    {
        _documentos = documentos;
        _empresas = empresas;
        _storage = storage;
    }

    public async Task<DocumentoEmpresaResponse> ExecuteAsync(
        AccessScope scope,
        TipoDocumentoEmpresarial tipo,
        string nomeArquivo,
        string contentType,
        long tamanhoBytes,
        Stream conteudo,
        CancellationToken cancellationToken = default)
    {
        if (scope is not AccessScope.Company companyScope)
        {
            throw new DocumentoEmpresaException("Acesso não permitido.", 403);
        }

        var empresaId = companyScope.CompanyId;
        var empresa = await _empresas.GetByIdAsync(empresaId, cancellationToken);
        if (empresa is null)
        {
            throw new DocumentoEmpresaException("Recurso não encontrado.", 404);
        }

        if (!_storage.IsConfigured)
        {
            throw new DocumentoEmpresaException("Armazenamento de documentos não configurado (R2).", 503);
        }

        if (!Enum.IsDefined(typeof(TipoDocumentoEmpresarial), tipo))
        {
            throw new DocumentoEmpresaException("Tipo de documento inválido.");
        }

        if (tamanhoBytes <= 0 || tamanhoBytes > MaxBytes)
        {
            throw new DocumentoEmpresaException("Arquivo deve ter no máximo 10 MB.");
        }

        if (!string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
            && !nomeArquivo.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new DocumentoEmpresaException("Envie apenas arquivos PDF.");
        }

        var documentoId = Guid.NewGuid();
        var storageKey = $"empresas/{empresaId:D}/documentos/{documentoId:D}.pdf";

        await _storage.UploadAsync(
            storageKey,
            conteudo,
            "application/pdf",
            cancellationToken);

        var documento = new DocumentoEmpresa(
            documentoId,
            empresaId,
            tipo,
            nomeArquivo,
            storageKey,
            "application/pdf",
            tamanhoBytes);

        await _documentos.AddAsync(documento, cancellationToken);

        return DocumentoEmpresaResponse.FromEntity(documento, empresa.RazaoSocial);
    }
}
