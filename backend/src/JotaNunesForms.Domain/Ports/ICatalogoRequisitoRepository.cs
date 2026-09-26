using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface ICatalogoRequisitoRepository
{
    Task<IReadOnlyList<CatalogoRequisito>> ListAsync(CancellationToken cancellationToken = default);

    Task<CatalogoRequisito?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CatalogoRequisito?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    Task AddAsync(CatalogoRequisito requisito, CancellationToken cancellationToken = default);

    Task UpdateAsync(CatalogoRequisito requisito, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ParametroNormativo>> ListParametrosAsync(CancellationToken cancellationToken = default);

    Task<ParametroNormativo?> GetParametroByChaveAsync(string chave, CancellationToken cancellationToken = default);

    Task AddParametroAsync(ParametroNormativo parametro, CancellationToken cancellationToken = default);

    Task UpdateParametroAsync(ParametroNormativo parametro, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EscopoArt>> ListEscoposArtAsync(CancellationToken cancellationToken = default);
}
