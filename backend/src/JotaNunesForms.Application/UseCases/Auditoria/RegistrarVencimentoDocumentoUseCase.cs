using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Auditoria;

public sealed class RegistrarVencimentoDocumentoUseCase
{
    private readonly IDocumentoEmpresaRepository _documentosEmpresa;
    private readonly IDocumentoFuncionarioRepository _documentosFuncionario;
    private readonly IDocumentoArquivoVersaoRepository _versoes;
    private readonly ITransactionalExecutor _transactions;
    private readonly AuditoriaDocumentoService _auditoria;

    public RegistrarVencimentoDocumentoUseCase(
        IDocumentoEmpresaRepository documentosEmpresa,
        IDocumentoFuncionarioRepository documentosFuncionario,
        IDocumentoArquivoVersaoRepository versoes,
        ITransactionalExecutor transactions,
        AuditoriaDocumentoService auditoria)
    {
        _documentosEmpresa = documentosEmpresa;
        _documentosFuncionario = documentosFuncionario;
        _versoes = versoes;
        _transactions = transactions;
        _auditoria = auditoria;
    }

    public Task<DocumentoEmpresa> ExpireCompanyIfDueAsync(
        Guid documentoId,
        DateTime utcNow,
        CancellationToken cancellationToken = default) =>
        _transactions.ExecuteAsync(async transactionToken =>
        {
            var documento = await _documentosEmpresa.GetByIdForUpdateAsync(documentoId, transactionToken)
                ?? throw new DocumentoEmpresaException("Recurso não encontrado.", 404);
            if (documento.Status != StatusDocumento.Aprovado || documento.ValidoAte is null || documento.ValidoAte > utcNow)
            {
                return documento;
            }

            var version = await _versoes.GetCurrentForEmpresaAsync(documentoId, transactionToken)
                ?? throw new DocumentoEmpresaException("Versão vigente não encontrada.", 409);
            documento.AtualizarVencimentoSeExpirado(utcNow);
            if (documento.Status == StatusDocumento.Vencido)
            {
                await _documentosEmpresa.UpdateAsync(documento, transactionToken);
                await _auditoria.RegisterAsync(new RegistrarEventoAuditoriaDocumento(
                    CodigoAuditoriaDocumento.DocumentoVencido,
                    null,
                    OrigemDocumentoAuditoria.DocumentoEmpresa,
                    documento.Id,
                    version.Id,
                    version.Numero,
                    ValidoAte: documento.ValidoAte,
                    OcorreuEm: utcNow), transactionToken);
            }

            return documento;
        }, cancellationToken);

    public Task<DocumentoFuncionario> ExpireEmployeeIfDueAsync(
        Guid documentoId,
        DateTime utcNow,
        CancellationToken cancellationToken = default) =>
        _transactions.ExecuteAsync(async transactionToken =>
        {
            var documento = await _documentosFuncionario.GetByIdForUpdateAsync(documentoId, transactionToken)
                ?? throw new DocumentoFuncionarioException("Recurso não encontrado.", 404);
            if (documento.Status != StatusDocumento.Aprovado || documento.ValidoAte is null || documento.ValidoAte > utcNow)
            {
                return documento;
            }

            var version = await _versoes.GetCurrentForFuncionarioAsync(documentoId, transactionToken)
                ?? throw new DocumentoFuncionarioException("Versão vigente não encontrada.", 409);
            documento.AtualizarVencimentoSeExpirado(utcNow);
            if (documento.Status == StatusDocumento.Vencido)
            {
                await _documentosFuncionario.UpdateAsync(documento, transactionToken);
                await _auditoria.RegisterAsync(new RegistrarEventoAuditoriaDocumento(
                    CodigoAuditoriaDocumento.DocumentoVencido,
                    null,
                    OrigemDocumentoAuditoria.DocumentoFuncionario,
                    documento.Id,
                    version.Id,
                    version.Numero,
                    ValidoAte: documento.ValidoAte,
                    OcorreuEm: utcNow), transactionToken);
            }

            return documento;
        }, cancellationToken);
}
