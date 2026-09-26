using JotaNunesForms.Application.Catalogo;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Catalogo;

public sealed class ListCatalogoRequisitosUseCase
{
    private readonly ICatalogoRequisitoRepository _catalogo;

    public ListCatalogoRequisitosUseCase(ICatalogoRequisitoRepository catalogo) => _catalogo = catalogo;

    public async Task<CatalogoCompletoResponse> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var requisitos = await _catalogo.ListAsync(cancellationToken);
        var parametros = await _catalogo.ListParametrosAsync(cancellationToken);
        return new CatalogoCompletoResponse(
            requisitos.Select(CatalogoRequisitoResponse.FromEntity).ToList(),
            parametros.Select(ParametroNormativoResponse.FromEntity).ToList());
    }
}

public sealed class CreateCatalogoRequisitoUseCase
{
    private readonly ICatalogoRequisitoRepository _catalogo;

    public CreateCatalogoRequisitoUseCase(ICatalogoRequisitoRepository catalogo) => _catalogo = catalogo;

    public async Task<CatalogoRequisitoResponse> ExecuteAsync(
        CreateCatalogoRequisitoRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existente = await _catalogo.GetByCodigoAsync(request.Codigo.Trim().ToUpperInvariant(), cancellationToken);
            if (existente is not null)
            {
                throw new CatalogoException("Já existe um requisito com este código.");
            }

            var requisito = new CatalogoRequisito(
                request.Codigo,
                request.Nome,
                request.Titular,
                request.Aplicacao,
                request.TipoEntrega,
                request.Camada,
                request.Condicao,
                request.ExigeValidade,
                request.PermiteVencerComoDocumento);
            await _catalogo.AddAsync(requisito, cancellationToken);
            return CatalogoRequisitoResponse.FromEntity(requisito);
        }
        catch (ArgumentException ex)
        {
            throw new CatalogoException(ex.Message);
        }
    }
}

public sealed class UpdateCatalogoRequisitoUseCase
{
    private readonly ICatalogoRequisitoRepository _catalogo;

    public UpdateCatalogoRequisitoUseCase(ICatalogoRequisitoRepository catalogo) => _catalogo = catalogo;

    public async Task<CatalogoRequisitoResponse> ExecuteAsync(
        Guid id,
        UpdateCatalogoRequisitoRequest request,
        CancellationToken cancellationToken = default)
    {
        var requisito = await _catalogo.GetByIdAsync(id, cancellationToken)
            ?? throw new CatalogoException("Requisito não encontrado.");

        try
        {
            requisito.Atualizar(request.Nome, request.Ativo, request.Condicao, request.ExigeValidade);
            await _catalogo.UpdateAsync(requisito, cancellationToken);
            return CatalogoRequisitoResponse.FromEntity(requisito);
        }
        catch (ArgumentException ex)
        {
            throw new CatalogoException(ex.Message);
        }
    }
}

public sealed class UpdateParametroNormativoUseCase
{
    private readonly ICatalogoRequisitoRepository _catalogo;

    public UpdateParametroNormativoUseCase(ICatalogoRequisitoRepository catalogo) => _catalogo = catalogo;

    public async Task<ParametroNormativoResponse> ExecuteAsync(
        string chave,
        UpdateParametroNormativoRequest request,
        CancellationToken cancellationToken = default)
    {
        var parametro = await _catalogo.GetParametroByChaveAsync(chave.Trim().ToUpperInvariant(), cancellationToken)
            ?? throw new CatalogoException("Parâmetro normativo não encontrado.");

        try
        {
            parametro.DefinirValor(request.Valor);
            await _catalogo.UpdateParametroAsync(parametro, cancellationToken);
            return ParametroNormativoResponse.FromEntity(parametro);
        }
        catch (ArgumentException ex)
        {
            throw new CatalogoException(ex.Message);
        }
    }
}

public sealed class ListEscoposArtUseCase
{
    private readonly ICatalogoRequisitoRepository _catalogo;

    public ListEscoposArtUseCase(ICatalogoRequisitoRepository catalogo) => _catalogo = catalogo;

    public async Task<IReadOnlyList<EscopoArtResponse>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var escopos = await _catalogo.ListEscoposArtAsync(cancellationToken);
        return escopos.Select(EscopoArtResponse.FromEntity).ToList();
    }
}
