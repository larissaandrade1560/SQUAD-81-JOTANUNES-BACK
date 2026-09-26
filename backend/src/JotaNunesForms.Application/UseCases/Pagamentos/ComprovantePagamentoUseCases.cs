using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Pagamentos;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Pagamentos;

public sealed class UploadComprovantePagamentoUseCase
{
    private const long MaxBytes = 10 * 1024 * 1024;

    private readonly IPagamentoFuncionarioRepository _pagamentos;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;
    private readonly IObjectStorage _storage;
    private readonly AccessScopeGuard _scopeGuard;

    public UploadComprovantePagamentoUseCase(
        IPagamentoFuncionarioRepository pagamentos,
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas,
        IObjectStorage storage,
        AccessScopeGuard scopeGuard)
    {
        _pagamentos = pagamentos;
        _funcionarios = funcionarios;
        _empresas = empresas;
        _storage = storage;
        _scopeGuard = scopeGuard;
    }

    public async Task<PagamentoFuncionarioResponse> ExecuteAsync(
        Guid pagamentoId,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        string nomeArquivo,
        string contentType,
        long tamanhoBytes,
        Stream conteudo,
        CancellationToken cancellationToken = default)
    {
        if (scope is not AccessScope.Company companyScope || companyScope.Type != TipoEmpresa.MaoDeObra)
        {
            throw new PagamentoException("Acesso não permitido.", 403);
        }

        var pagamento = await _pagamentos.GetByIdAsync(pagamentoId, cancellationToken);
        if (pagamento is null)
        {
            throw new PagamentoException("Recurso não encontrado.", 404);
        }

        if (!await _scopeGuard.AllowsCompanyAsync(scope, pagamento.EmpresaId, securityRequest, cancellationToken))
        {
            throw new PagamentoException("Recurso não encontrado.", 404);
        }

        if (!_storage.IsConfigured)
        {
            throw new PagamentoException("Armazenamento de comprovantes não configurado (R2).", 503);
        }

        if (tamanhoBytes <= 0 || tamanhoBytes > MaxBytes)
        {
            throw new PagamentoException("Arquivo deve ter no máximo 10 MB.");
        }

        if (!string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
            && !nomeArquivo.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new PagamentoException("Envie apenas arquivos PDF.");
        }

        var arquivoId = Guid.NewGuid();
        var storageKey =
            $"empresas/{pagamento.EmpresaId:D}/pagamentos/{pagamento.Id:D}/comprovante-{arquivoId:D}.pdf";

        await _storage.UploadAsync(
            storageKey,
            conteudo,
            "application/pdf",
            cancellationToken);

        pagamento.RegistrarComprovante(nomeArquivo, storageKey, "application/pdf", tamanhoBytes);
        await _pagamentos.UpdateAsync(pagamento, cancellationToken);

        var funcionario = await _funcionarios.GetByIdAsync(pagamento.FuncionarioId, cancellationToken);
        var empresas = await _empresas.ListAsync(cancellationToken);
        var razao = empresas.FirstOrDefault(e => e.Id == pagamento.EmpresaId)?.RazaoSocial ?? "—";
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        return ListPagamentosFuncionarioUseCase.ToResponse(
            pagamento,
            funcionario?.Nome ?? "—",
            razao,
            hoje);
    }
}

public sealed class GetComprovantePagamentoDownloadUseCase
{
    private readonly IPagamentoFuncionarioRepository _pagamentos;
    private readonly IObjectStorage _storage;
    private readonly AccessScopeGuard _scopeGuard;

    public GetComprovantePagamentoDownloadUseCase(
        IPagamentoFuncionarioRepository pagamentos,
        IObjectStorage storage,
        AccessScopeGuard scopeGuard)
    {
        _pagamentos = pagamentos;
        _storage = storage;
        _scopeGuard = scopeGuard;
    }

    public async Task<DocumentoDownloadResponse> ExecuteAsync(
        Guid pagamentoId,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        CancellationToken cancellationToken = default)
    {
        if (scope is AccessScope.Company companyScope && companyScope.Type != TipoEmpresa.MaoDeObra)
        {
            throw new PagamentoException("Acesso não permitido.", 403);
        }

        var pagamento = await _pagamentos.GetByIdAsync(pagamentoId, cancellationToken);
        if (pagamento is null)
        {
            throw new PagamentoException("Recurso não encontrado.", 404);
        }

        if (!await _scopeGuard.AllowsCompanyAsync(scope, pagamento.EmpresaId, securityRequest, cancellationToken))
        {
            throw new PagamentoException("Recurso não encontrado.", 404);
        }

        if (!pagamento.PossuiComprovante || pagamento.ComprovanteStorageKey is null)
        {
            throw new PagamentoException("Recurso não encontrado.", 404);
        }

        if (!_storage.IsConfigured)
        {
            throw new PagamentoException("Armazenamento de comprovantes não configurado (R2).", 503);
        }

        var validFor = TimeSpan.FromMinutes(15);
        var url = await _storage.GetDownloadUrlAsync(
            pagamento.ComprovanteStorageKey,
            validFor,
            cancellationToken);
        return new DocumentoDownloadResponse(url, DateTime.UtcNow.Add(validFor));
    }
}
