using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class ReenviarDocumentoFuncionarioUseCase
{
    private const long MaxBytes = 10 * 1024 * 1024;

    private readonly IDocumentoFuncionarioRepository _documentos;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;
    private readonly IObjectStorage _storage;

    public ReenviarDocumentoFuncionarioUseCase(
        IDocumentoFuncionarioRepository documentos,
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas,
        IObjectStorage storage)
    {
        _documentos = documentos;
        _funcionarios = funcionarios;
        _empresas = empresas;
        _storage = storage;
    }

    public async Task<DocumentoFuncionarioResponse> ExecuteAsync(
        Guid id,
        Guid scopeEmpresaId,
        string nomeArquivo,
        string contentType,
        long tamanhoBytes,
        Stream conteudo,
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

        if (funcionario.EmpresaId != scopeEmpresaId)
        {
            throw new DocumentoFuncionarioException("Sem permissão para reenviar este documento.");
        }

        if (tamanhoBytes <= 0 || tamanhoBytes > MaxBytes)
        {
            throw new DocumentoFuncionarioException("Arquivo deve ter no máximo 10 MB.");
        }

        if (!string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
            && !nomeArquivo.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new DocumentoFuncionarioException("Envie apenas arquivos PDF.");
        }

        var empresa = await _empresas.GetByIdAsync(funcionario.EmpresaId, cancellationToken);
        if (empresa is null)
        {
            throw new DocumentoFuncionarioException("Empresa não encontrada.");
        }

        var novoArquivoId = Guid.NewGuid();
        var storageKey =
            $"empresas/{funcionario.EmpresaId:D}/funcionarios/{funcionario.Id:D}/documentos/{novoArquivoId:D}.pdf";

        await _storage.UploadAsync(
            storageKey,
            conteudo,
            "application/pdf",
            cancellationToken);

        try
        {
            documento.Reenviar(nomeArquivo, storageKey, "application/pdf", tamanhoBytes);
        }
        catch (InvalidOperationException ex)
        {
            throw new DocumentoFuncionarioException(ex.Message);
        }

        await _documentos.UpdateAsync(documento, cancellationToken);
        return DocumentoFuncionarioResponse.FromEntity(documento, funcionario, empresa.RazaoSocial);
    }
}
