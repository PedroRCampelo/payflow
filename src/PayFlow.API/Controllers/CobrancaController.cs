using Microsoft.AspNetCore.Mvc;
using PayFlow.API.DTO;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;
using PayFlow.Domain.Exceptions;
using PayFlow.Domain.Repositories;
using PayFlow.Domain.ValueObjects;

namespace PayFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CobrancasController : ControllerBase
{
    private readonly ICobrancaRepository _repository;

    public CobrancasController(ICobrancaRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CobrancaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Criar([FromBody] CriarCobrancaRequest request)
    {
        var valor = new Dinheiro(request.Valor, request.Moeda);
        var cobranca = new Cobranca(request.Descricao, valor, request.DataVencimento);

        await _repository.AdicionarAsync(cobranca);

        var response = MapToResponse(cobranca);
        return CreatedAtAction(nameof(ObterPorId), new { id = cobranca.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CobrancaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var cobranca = await _repository.ObterPorIdAsync(id);

        if (cobranca is null)
            return NotFound();

        return Ok(MapToResponse(cobranca));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CobrancaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] StatusCobranca? status = null)
    {
        var cobrancas = await _repository.ListarAsync(status);

        var response = cobrancas.Select(MapToResponse);
        return Ok(response);
    }

    [HttpPatch("{id:guid}/confirmar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Confirmar(Guid id)
    {
        var cobranca = await _repository.ObterPorIdAsync(id);

        if (cobranca is null)
            return NotFound();

        cobranca.Confirmar();
        await _repository.AtualizarAsync(cobranca);

        return NoContent();
    }

    [HttpPatch("{id:guid}/cancelar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        var cobranca = await _repository.ObterPorIdAsync(id);

        if (cobranca is null)
            return NotFound();

        cobranca.Cancelar();
        await _repository.AtualizarAsync(cobranca);

        return NoContent();
    }

    private static CobrancaResponse MapToResponse(Cobranca cobranca)
    {
        return new CobrancaResponse(
            cobranca.Id,
            cobranca.Descricao,
            cobranca.Valor.Valor,
            cobranca.Valor.Moeda,
            cobranca.Status.ToString(),
            cobranca.CriadaEm,
            cobranca.DataVencimento,
            cobranca.ConfirmadaEm,
            cobranca.CanceladaEm,
            cobranca.PagaEm
        );
    }
}