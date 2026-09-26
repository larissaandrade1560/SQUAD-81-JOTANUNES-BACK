using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Application.Mobilizacoes;
using JotaNunesForms.Application.Obras;
using JotaNunesForms.Application.Processos;
using JotaNunesForms.Application.UseCases.Mobilizacoes;
using JotaNunesForms.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/mobilizacoes")]
[Authorize]
public sealed class MobilizacoesController : ControllerBase
{
    private readonly CreateMobilizacaoUseCase _create;
    private readonly ListMobilizacoesUseCase _list;
    private readonly GetMobilizacaoUseCase _get;
    private readonly UpdateMobilizacaoUseCase _update;

    public MobilizacoesController(
        CreateMobilizacaoUseCase create,
        ListMobilizacoesUseCase list,
        GetMobilizacaoUseCase get,
        UpdateMobilizacaoUseCase update)
    {
        _create = create;
        _list = list;
        _get = get;
        _update = update;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MobilizacaoResponse>>> List(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? obraId,
        [FromQuery] Guid? contratoId,
        [FromQuery] SituacaoMobilizacao? situacao,
        CancellationToken cancellationToken)
    {
        Guid? scope = UserClaims.IsTerceirizado(User) ? UserClaims.GetEmpresaId(User) : empresaId;
        if (UserClaims.IsTerceirizado(User) && scope is null)
        {
            return Forbid();
        }

        return Ok(await _list.ExecuteAsync(scope, obraId, contratoId, situacao, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MobilizacaoResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _get.ExecuteAsync(id, ScopeEmpresa(), cancellationToken));
        }
        catch (MobilizacaoException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (EmpresaException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<MobilizacaoResponse>> Create(
        [FromBody] CreateMobilizacaoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _create.ExecuteAsync(request, ScopeEmpresa(), cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (MobilizacaoException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (EmpresaException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ObraException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ProcessoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<MobilizacaoResponse>> Update(
        Guid id,
        [FromBody] UpdateMobilizacaoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _update.ExecuteAsync(id, request, ScopeEmpresa(), cancellationToken));
        }
        catch (MobilizacaoException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    private Guid? ScopeEmpresa() => UserClaims.IsTerceirizado(User) ? UserClaims.GetEmpresaId(User) : null;
}
