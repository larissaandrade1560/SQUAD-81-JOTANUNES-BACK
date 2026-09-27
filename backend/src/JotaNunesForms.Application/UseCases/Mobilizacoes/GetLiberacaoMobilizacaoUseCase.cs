using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Mobilizacoes;
namespace JotaNunesForms.Application.UseCases.Mobilizacoes;

public sealed class GetLiberacaoMobilizacaoUseCase
{
    private readonly MobilizacaoAccessService _access;
    private readonly RecalcularLiberacaoMobilizacaoUseCase _recalcular;

    public GetLiberacaoMobilizacaoUseCase(
        MobilizacaoAccessService access,
        RecalcularLiberacaoMobilizacaoUseCase recalcular)
    {
        _access = access;
        _recalcular = recalcular;
    }

    public async Task<ResultadoLiberacaoResponse> ExecuteAsync(
        Guid mobilizacaoId,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        CancellationToken cancellationToken = default)
    {
        var ownership = await _access.RequireReadableAsync(mobilizacaoId, scope, securityRequest, cancellationToken);
        var resultado = await _recalcular.ExecuteAsync(ownership.Mobilizacao.Id, ownership.Funcionario, cancellationToken);
        return LiberacaoMapper.ToResponse(resultado);
    }
}
