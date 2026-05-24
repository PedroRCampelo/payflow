using PayFlow.API.DTO;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;
using PayFlow.Domain.Repositories;
using PayFlow.Domain.ValueObjects;

namespace PayFlow.API.Endpoints;

public static class CobrancaEndpoints
{
    public static void MapCobrancaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/cobrancas");

        group.MapPost("", async (CriarCobrancaRequest request, ICobrancaRepository repository) =>
        {
            var valor = new Dinheiro(request.Valor, request.Moeda);
            var cobranca = new Cobranca(request.Descricao, valor, request.DataVencimento);

            await repository.AdicionarAsync(cobranca);

            return Results.Created($"/api/cobrancas/{cobranca.Id}", MapToResponse(cobranca));
        });

        group.MapGet("{id:guid}", async (Guid id, ICobrancaRepository repository) =>
        {
            var cobranca = await repository.ObterPorIdAsync(id);

            return cobranca is null
                ? Results.NotFound()
                : Results.Ok(MapToResponse(cobranca));
        });

        group.MapGet("", async (StatusCobranca? status, ICobrancaRepository repository) =>
        {
            var cobrancas = await repository.ListarAsync(status);

            return Results.Ok(cobrancas.Select(MapToResponse));
        });

        group.MapPatch("{id:guid}/confirmar", async (Guid id, ICobrancaRepository repository) =>
        {
            var cobranca = await repository.ObterPorIdAsync(id);

            if (cobranca is null)
                return Results.NotFound();

            cobranca.Confirmar();
            await repository.AtualizarAsync(cobranca);

            return Results.NoContent();
        });

        group.MapPatch("{id:guid}/cancelar", async (Guid id, ICobrancaRepository repository) =>
        {
            var cobranca = await repository.ObterPorIdAsync(id);

            if (cobranca is null)
                return Results.NotFound();

            cobranca.Cancelar();
            await repository.AtualizarAsync(cobranca);

            return Results.NoContent();
        });
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