using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;
using PayFlow.Domain.Exceptions;

namespace PayFlow.Domain.Tests;

public class PagamentoTests
{
    // Creation

    [Fact]
    public void Constructor_WithValidData_ShouldCreateWithProcessingStatus()
    {
        var cobrancaId = Guid.NewGuid();

        var pagamento = new Pagamento(cobrancaId, 150.00m, "TXN-001");

        Assert.Equal(cobrancaId, pagamento.CobrancaId);
        Assert.Equal(150.00m, pagamento.ValorPago);
        Assert.Equal(StatusPagamento.Processando, pagamento.Status);
        Assert.Equal("TXN-001", pagamento.CodigoTransacao);
        Assert.NotEqual(Guid.Empty, pagamento.Id);
    }

    [Fact]
    public void Constructor_WithEmptyCobrancaId_ShouldThrowDomainException()
    {
        Assert.Throws<DomainException>(
            () => new Pagamento(Guid.Empty, 100.00m));
    }

    [Fact]
    public void Constructor_WithZeroAmount_ShouldThrowDomainException()
    {
        Assert.Throws<DomainException>(
            () => new Pagamento(Guid.NewGuid(), 0));
    }

    [Fact]
    public void Constructor_WithoutTransactionCode_ShouldCreateWithNullCode()
    {
        var pagamento = new Pagamento(Guid.NewGuid(), 100.00m);

        Assert.Null(pagamento.CodigoTransacao);
    }

    // Aprovar

    [Fact]
    public void Aprovar_WhenProcessing_ShouldChangeStatusToApproved()
    {
        var pagamento = CreateValidPagamento();

        pagamento.Aprovar();

        Assert.Equal(StatusPagamento.Aprovado, pagamento.Status);
        Assert.NotNull(pagamento.AprovadoEm);
    }

    [Fact]
    public void Aprovar_WhenAlreadyApproved_ShouldThrowDomainException()
    {
        var pagamento = CreateValidPagamento();
        pagamento.Aprovar();

        Assert.Throws<DomainException>(() => pagamento.Aprovar());
    }

    [Fact]
    public void Aprovar_WhenDeclined_ShouldThrowDomainException()
    {
        var pagamento = CreateValidPagamento();
        pagamento.Recusar("Insufficient funds");

        Assert.Throws<DomainException>(() => pagamento.Aprovar());
    }

    // Recusar

    [Fact]
    public void Recusar_WhenProcessing_ShouldChangeStatusToDeclined()
    {
        var pagamento = CreateValidPagamento();

        pagamento.Recusar("Card expired");

        Assert.Equal(StatusPagamento.Recusado, pagamento.Status);
        Assert.Equal("Card expired", pagamento.MotivoRecusa);
        Assert.NotNull(pagamento.RecusadoEm);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Recusar_WithInvalidReason_ShouldThrowDomainException(string? motivo)
    {
        var pagamento = CreateValidPagamento();

        Assert.Throws<DomainException>(() => pagamento.Recusar(motivo!));
    }

    [Fact]
    public void Recusar_WhenAlreadyApproved_ShouldThrowDomainException()
    {
        var pagamento = CreateValidPagamento();
        pagamento.Aprovar();

        Assert.Throws<DomainException>(() => pagamento.Recusar("Fraud"));
    }

    // Estornar

    [Fact]
    public void Estornar_WhenApproved_ShouldChangeStatusToRefunded()
    {
        var pagamento = CreateValidPagamento();
        pagamento.Aprovar();

        pagamento.Estornar();

        Assert.Equal(StatusPagamento.Estornado, pagamento.Status);
        Assert.NotNull(pagamento.EstornadoEm);
    }

    [Fact]
    public void Estornar_WhenProcessing_ShouldThrowDomainException()
    {
        var pagamento = CreateValidPagamento();

        Assert.Throws<DomainException>(() => pagamento.Estornar());
    }

    [Fact]
    public void Estornar_WhenDeclined_ShouldThrowDomainException()
    {
        var pagamento = CreateValidPagamento();
        pagamento.Recusar("Insufficient funds");

        Assert.Throws<DomainException>(() => pagamento.Estornar());
    }

    // Domain queries

    [Fact]
    public void PodeSerEstornado_WhenApproved_ShouldReturnTrue()
    {
        var pagamento = CreateValidPagamento();
        pagamento.Aprovar();

        Assert.True(pagamento.PodeSerEstornado());
    }

    [Fact]
    public void PodeSerEstornado_WhenProcessing_ShouldReturnFalse()
    {
        var pagamento = CreateValidPagamento();

        Assert.False(pagamento.PodeSerEstornado());
    }

    // Helper

    private static Pagamento CreateValidPagamento()
    {
        return new Pagamento(Guid.NewGuid(), 100.00m, "TXN-001");
    }
}