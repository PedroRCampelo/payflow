using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;
using PayFlow.Domain.Exceptions;
using PayFlow.Domain.ValueObjects;

namespace PayFlow.Domain.Tests;

public class CobrancaTests
{
    // Creation

    [Fact]
    public void Constructor_WithValidData_ShouldCreateWithPendingStatus()
    {
        var descricao = "Monthly payment";
        var valor = new Dinheiro(150.00m);
        var vencimento = DateTime.UtcNow.AddDays(30);

        var cobranca = new Cobranca(descricao, valor, vencimento);

        Assert.Equal(descricao, cobranca.Descricao);
        Assert.Equal(150.00m, cobranca.Valor.Valor);
        Assert.Equal("BRL", cobranca.Valor.Moeda);
        Assert.Equal(StatusCobranca.Pendente, cobranca.Status);
        Assert.NotEqual(Guid.Empty, cobranca.Id);
    }

    [Fact]
    public void Constructor_WithZeroAmount_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => new Cobranca("Test", new Dinheiro(0), DateTime.UtcNow.AddDays(30)));

        Assert.Contains("greater than zero", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidDescription_ShouldThrowDomainException(string? descricao)
    {
        Assert.Throws<DomainException>(
            () => new Cobranca(descricao!, new Dinheiro(100), DateTime.UtcNow.AddDays(30)));
    }

    [Fact]
    public void Constructor_WithPastDueDate_ShouldThrowDomainException()
    {
        Assert.Throws<DomainException>(
            () => new Cobranca("Test", new Dinheiro(100), DateTime.UtcNow.AddDays(-1)));
    }

    // Confirmar

    [Fact]
    public void Confirmar_WhenPending_ShouldChangeStatusToConfirmada()
    {
        var cobranca = CreateValidCobranca();

        cobranca.Confirmar();

        Assert.Equal(StatusCobranca.Confirmada, cobranca.Status);
        Assert.NotNull(cobranca.ConfirmadaEm);
    }

    [Fact]
    public void Confirmar_WhenAlreadyConfirmed_ShouldThrowDomainException()
    {
        var cobranca = CreateValidCobranca();
        cobranca.Confirmar();

        Assert.Throws<DomainException>(() => cobranca.Confirmar());
    }

    // AdicionarPagamento (aggregate root behavior)

    [Fact]
    public void AdicionarPagamento_WhenConfirmed_ShouldAddPaymentToCollection()
    {
        var cobranca = CreateValidCobranca();
        cobranca.Confirmar();

        var pagamento = cobranca.AdicionarPagamento(new Dinheiro(100.00m), "TXN-001");

        Assert.Single(cobranca.Pagamentos);
        Assert.Equal(cobranca.Id, pagamento.CobrancaId);
    }

    [Fact]
    public void AdicionarPagamento_WhenPending_ShouldThrowDomainException()
    {
        var cobranca = CreateValidCobranca();

        Assert.Throws<DomainException>(
            () => cobranca.AdicionarPagamento(new Dinheiro(100.00m)));
    }

    [Fact]
    public void AdicionarPagamento_WithDifferentCurrency_ShouldThrowDomainException()
    {
        var cobranca = CreateValidCobranca();
        cobranca.Confirmar();

        Assert.Throws<DomainException>(
            () => cobranca.AdicionarPagamento(new Dinheiro(100.00m, "USD")));
    }

    // RegistrarPagamento

    [Fact]
    public void RegistrarPagamento_WhenConfirmed_ShouldChangeStatusToPaga()
    {
        var cobranca = CreateValidCobranca();
        cobranca.Confirmar();

        cobranca.RegistrarPagamento();

        Assert.Equal(StatusCobranca.Paga, cobranca.Status);
        Assert.NotNull(cobranca.PagaEm);
    }

    [Fact]
    public void RegistrarPagamento_WhenPending_ShouldThrowDomainException()
    {
        var cobranca = CreateValidCobranca();

        Assert.Throws<DomainException>(() => cobranca.RegistrarPagamento());
    }

    // Cancelar

    [Fact]
    public void Cancelar_WhenPending_ShouldChangeStatusToCancelada()
    {
        var cobranca = CreateValidCobranca();

        cobranca.Cancelar();

        Assert.Equal(StatusCobranca.Cancelada, cobranca.Status);
        Assert.NotNull(cobranca.CanceladaEm);
    }

    [Fact]
    public void Cancelar_WhenConfirmed_ShouldChangeStatusToCancelada()
    {
        var cobranca = CreateValidCobranca();
        cobranca.Confirmar();

        cobranca.Cancelar();

        Assert.Equal(StatusCobranca.Cancelada, cobranca.Status);
    }

    [Fact]
    public void Cancelar_WhenAlreadyPaid_ShouldThrowDomainException()
    {
        var cobranca = CreateValidCobranca();
        cobranca.Confirmar();
        cobranca.RegistrarPagamento();

        var exception = Assert.Throws<DomainException>(() => cobranca.Cancelar());
        Assert.Contains("already been paid", exception.Message);
    }

    [Fact]
    public void Cancelar_WhenAlreadyCancelled_ShouldThrowDomainException()
    {
        var cobranca = CreateValidCobranca();
        cobranca.Cancelar();

        Assert.Throws<DomainException>(() => cobranca.Cancelar());
    }

    // Domain queries

    [Fact]
    public void PodeSerCancelada_WhenPending_ShouldReturnTrue()
    {
        var cobranca = CreateValidCobranca();

        Assert.True(cobranca.PodeSerCancelada());
    }

    [Fact]
    public void PodeSerCancelada_WhenPaid_ShouldReturnFalse()
    {
        var cobranca = CreateValidCobranca();
        cobranca.Confirmar();
        cobranca.RegistrarPagamento();

        Assert.False(cobranca.PodeSerCancelada());
    }

    // Helper

    private static Cobranca CreateValidCobranca()
    {
        return new Cobranca("Monthly payment", new Dinheiro(100.00m), DateTime.UtcNow.AddDays(30));
    }
}