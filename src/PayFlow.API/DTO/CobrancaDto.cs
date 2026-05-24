namespace PayFlow.API.DTO;

public record CriarCobrancaRequest(
    string Descricao,
    decimal Valor,
    string Moeda,
    DateTime DataVencimento
);
 
public record CobrancaResponse(
    Guid Id,
    string Descricao,
    decimal Valor,
    string Moeda,
    string Status,
    DateTime CriadaEm,
    DateTime DataVencimento,
    DateTime? ConfirmadaEm,
    DateTime? CanceladaEm,
    DateTime? PagaEm
);
