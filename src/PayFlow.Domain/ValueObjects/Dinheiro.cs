using PayFlow.Domain.Exceptions;

namespace PayFlow.Domain.ValueObjects;

public record Dinheiro
{
    public decimal Valor { get; }
    public string Moeda { get; }

    public Dinheiro(decimal valor, string moeda = "BRL") // "BRL" Default parameter
    {
        if (valor < 0)
            throw new DomainException("Amount cannot be negative.");

        if (string.IsNullOrWhiteSpace(moeda))
            throw new DomainException("Currency is required.");

        Valor = valor;
        Moeda = moeda.ToUpperInvariant();
    }

    public static Dinheiro Zero(string moeda = "BRL") => new(0, moeda);

    public Dinheiro Somar(Dinheiro outro)
    {
        ValidarMesmaMoeda(outro);
        return new Dinheiro(Valor + outro.Valor, Moeda);
    }

    public Dinheiro Subtrair(Dinheiro outro)
    {
        ValidarMesmaMoeda(outro);

        if (outro.Valor > Valor)
            throw new DomainException("Resulting amount cannot be negative.");

        return new Dinheiro(Valor - outro.Valor, Moeda);
    }

    public bool MaiorQue(Dinheiro outro)
    {
        ValidarMesmaMoeda(outro);
        return Valor > outro.Valor;
    }

    private void ValidarMesmaMoeda(Dinheiro outro)
    {
        if (Moeda != outro.Moeda)
            throw new DomainException($"Cannot operate on different currencies: {Moeda} and {outro.Moeda}.");
    }

    public override string ToString() => $"{Moeda} {Valor:N2}";
}