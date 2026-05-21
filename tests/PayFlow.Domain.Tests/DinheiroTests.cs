using PayFlow.Domain.Exceptions;
using PayFlow.Domain.ValueObjects;

namespace PayFlow.Domain.Tests;

public class DinheiroTests
{
    [Fact]
    public void Constructor_WithValidAmount_ShouldCreate()
    {
        var dinheiro = new Dinheiro(100.00m, "BRL");

        Assert.Equal(100.00m, dinheiro.Valor);
        Assert.Equal("BRL", dinheiro.Moeda);
    }

    [Fact]
    public void Constructor_WithoutCurrency_ShouldDefaultToBRL()
    {
        var dinheiro = new Dinheiro(50.00m);

        Assert.Equal("BRL", dinheiro.Moeda);
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ShouldThrowDomainException()
    {
        Assert.Throws<DomainException>(() => new Dinheiro(-10.00m));
    }

    [Fact]
    public void Constructor_ShouldNormalizeCurrencyToUpperCase()
    {
        var dinheiro = new Dinheiro(100.00m, "brl");

        Assert.Equal("BRL", dinheiro.Moeda);
    }

    // Value equality (records compare by value)

    [Fact]
    public void Equals_WithSameAmountAndCurrency_ShouldBeEqual()
    {
        var a = new Dinheiro(100.00m, "BRL");
        var b = new Dinheiro(100.00m, "BRL");

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equals_WithDifferentAmount_ShouldNotBeEqual()
    {
        var a = new Dinheiro(100.00m, "BRL");
        var b = new Dinheiro(200.00m, "BRL");

        Assert.NotEqual(a, b);
    }

    // Operations

    [Fact]
    public void Somar_WithSameCurrency_ShouldReturnSum()
    {
        var a = new Dinheiro(100.00m);
        var b = new Dinheiro(50.00m);

        var result = a.Somar(b);

        Assert.Equal(150.00m, result.Valor);
    }

    [Fact]
    public void Somar_WithDifferentCurrency_ShouldThrowDomainException()
    {
        var brl = new Dinheiro(100.00m, "BRL");
        var usd = new Dinheiro(50.00m, "USD");

        Assert.Throws<DomainException>(() => brl.Somar(usd));
    }

    [Fact]
    public void Subtrair_WithValidAmount_ShouldReturnDifference()
    {
        var a = new Dinheiro(100.00m);
        var b = new Dinheiro(30.00m);

        var result = a.Subtrair(b);

        Assert.Equal(70.00m, result.Valor);
    }

    [Fact]
    public void Subtrair_WhenResultWouldBeNegative_ShouldThrowDomainException()
    {
        var a = new Dinheiro(50.00m);
        var b = new Dinheiro(100.00m);

        Assert.Throws<DomainException>(() => a.Subtrair(b));
    }

    [Fact]
    public void MaiorQue_WhenGreater_ShouldReturnTrue()
    {
        var a = new Dinheiro(200.00m);
        var b = new Dinheiro(100.00m);

        Assert.True(a.MaiorQue(b));
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var dinheiro = new Dinheiro(1500.50m, "BRL");

        Assert.Equal("BRL 1,500.50", dinheiro.ToString());
    }
}