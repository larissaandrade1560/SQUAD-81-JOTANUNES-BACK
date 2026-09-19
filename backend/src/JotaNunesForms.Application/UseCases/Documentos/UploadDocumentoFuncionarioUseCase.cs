using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Documentos;

public sealed class UploadDocumentoFuncionarioUseCase
{
    private const long MaxBytes = 10 * 1024 * 1024;

    private readonly IDocumentoFuncionarioRepository _documentos;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;
    private readonly IObjectStorage _storage;

    public UploadDocumentoFuncionarioUseCase(
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
        Guid funcionarioId,
        Guid scopeEmpresaId,
        TipoDocumentoFuncionario tipo,
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

        var funcionario = await _funcionarios.GetByIdAsync(funcionarioId, cancellationToken);
        if (funcionario is null)
        {
            throw new DocumentoFuncionarioException("Funcionário não encontrado.");
        }

        if (funcionario.EmpresaId != scopeEmpresaId)
        {
            throw new DocumentoFuncionarioException("Sem permissão para enviar documentos deste funcionário.");
        }

        var empresa = await _empresas.GetByIdAsync(funcionario.EmpresaId, cancellationToken);
        if (empresa is null)
        {
            throw new DocumentoFuncionarioException("Empresa não encontrada.");
        }

        if (empresa.Tipo != TipoEmpresa.MaoDeObra)
        {
            throw new DocumentoFuncionarioException("Upload disponível apenas para empresas de Mão de Obra.");
        }

        if (!Enum.IsDefined(typeof(TipoDocumentoFuncionario), tipo))
        {
            throw new DocumentoFuncionarioException("Tipo de documento inválido.");
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

        var documentoId = Guid.NewGuid();
        var storageKey =
            $"empresas/{funcionario.EmpresaId:D}/funcionarios/{funcionarioId:D}/documentos/{documentoId:D}.pdf";

        await _storage.UploadAsync(
            storageKey,
            conteudo,
            "application/pdf",
            cancellationToken);

        var documento = new DocumentoFuncionario(
            documentoId,
            funcionarioId,
            tipo,
            nomeArquivo,
            storageKey,
            "application/pdf",
            tamanhoBytes);

        await _documentos.AddAsync(documento, cancellationToken);

        return DocumentoFuncionarioResponse.FromEntity(documento, funcionario, empresa.RazaoSocial);
    }
}
