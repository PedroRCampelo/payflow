using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;
using PayFlow.Domain.Exceptions;

namespace PayFlow.Domain.Tests;

public class CobrancaTests
{
    // ═══════════════════════════════════════
    // CRIAÇÃO
    // ═══════════════════════════════════════

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarComStatusPendente()
    {
        // Arrange
        var descricao = "Mensalidade Janeiro";
        var valor = 150.00m;
        var vencimento = DateTime.UtcNow.AddDays(30);

        // Act
        var cobranca = new Cobranca(descricao, valor, vencimento);

        // Assert
        Assert.Equal(descricao, cobranca.Descricao);
        Assert.Equal(valor, cobranca.Valor);
        Assert.Equal(StatusCobranca.Pendente, cobranca.Status);
        Assert.NotEqual(Guid.Empty, cobranca.Id);
    }

    [Fact]
    public void Criar_ComValorZero_DeveLancarDomainException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<DomainException>(
            () => new Cobranca("Teste", 0, DateTime.UtcNow.AddDays(30)));

        Assert.Contains("maior que zero", exception.Message);
    }

    [Fact]
    public void Criar_ComValorNegativo_DeveLancarDomainException()
    {
        Assert.Throws<DomainException>(
            () => new Cobranca("Teste", -100, DateTime.UtcNow.AddDays(30)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Criar_ComDescricaoInvalida_DeveLancarDomainException(string? descricao)
    {
        Assert.Throws<DomainException>(
            () => new Cobranca(descricao!, 100, DateTime.UtcNow.AddDays(30)));
    }

    [Fact]
    public void Criar_ComVencimentoNoPassado_DeveLancarDomainException()
    {
        Assert.Throws<DomainException>(
            () => new Cobranca("Teste", 100, DateTime.UtcNow.AddDays(-1)));
    }

    // ═══════════════════════════════════════
    // CONFIRMAR
    // ═══════════════════════════════════════

    [Fact]
    public void Confirmar_QuandoPendente_DeveAlterarStatusParaConfirmada()
    {
        // Arrange
        var cobranca = CriarCobrancaValida();

        // Act
        cobranca.Confirmar();

        // Assert
        Assert.Equal(StatusCobranca.Confirmada, cobranca.Status);
        Assert.NotNull(cobranca.ConfirmadaEm);
    }

    [Fact]
    public void Confirmar_QuandoJaConfirmada_DeveLancarDomainException()
    {
        // Arrange
        var cobranca = CriarCobrancaValida();
        cobranca.Confirmar();

        // Act & Assert
        Assert.Throws<DomainException>(() => cobranca.Confirmar());
    }

    // ═══════════════════════════════════════
    // REGISTRAR PAGAMENTO
    // ═══════════════════════════════════════

    [Fact]
    public void RegistrarPagamento_QuandoConfirmada_DeveAlterarStatusParaPaga()
    {
        // Arrange
        var cobranca = CriarCobrancaValida();
        cobranca.Confirmar();

        // Act
        cobranca.RegistrarPagamento();

        // Assert
        Assert.Equal(StatusCobranca.Paga, cobranca.Status);
        Assert.NotNull(cobranca.PagaEm);
    }

    [Fact]
    public void RegistrarPagamento_QuandoPendente_DeveLancarDomainException()
    {
        var cobranca = CriarCobrancaValida();

        Assert.Throws<DomainException>(() => cobranca.RegistrarPagamento());
    }

    // ═══════════════════════════════════════
    // CANCELAR
    // ═══════════════════════════════════════

    [Fact]
    public void Cancelar_QuandoPendente_DeveAlterarStatusParaCancelada()
    {
        var cobranca = CriarCobrancaValida();

        cobranca.Cancelar();

        Assert.Equal(StatusCobranca.Cancelada, cobranca.Status);
        Assert.NotNull(cobranca.CanceladaEm);
    }

    [Fact]
    public void Cancelar_QuandoConfirmada_DeveAlterarStatusParaCancelada()
    {
        var cobranca = CriarCobrancaValida();
        cobranca.Confirmar();

        cobranca.Cancelar();

        Assert.Equal(StatusCobranca.Cancelada, cobranca.Status);
    }

    [Fact]
    public void Cancelar_QuandoPaga_DeveLancarDomainException()
    {
        var cobranca = CriarCobrancaValida();
        cobranca.Confirmar();
        cobranca.RegistrarPagamento();

        var exception = Assert.Throws<DomainException>(() => cobranca.Cancelar());
        Assert.Contains("já paga", exception.Message);
    }

    [Fact]
    public void Cancelar_QuandoJaCancelada_DeveLancarDomainException()
    {
        var cobranca = CriarCobrancaValida();
        cobranca.Cancelar();

        Assert.Throws<DomainException>(() => cobranca.Cancelar());
    }

    // ═══════════════════════════════════════
    // QUERIES DO DOMÍNIO
    // ═══════════════════════════════════════

    [Fact]
    public void PodeSerCancelada_QuandoPendente_DeveRetornarTrue()
    {
        var cobranca = CriarCobrancaValida();

        Assert.True(cobranca.PodeSerCancelada());
    }

    [Fact]
    public void PodeSerCancelada_QuandoPaga_DeveRetornarFalse()
    {
        var cobranca = CriarCobrancaValida();
        cobranca.Confirmar();
        cobranca.RegistrarPagamento();

        Assert.False(cobranca.PodeSerCancelada());
    }

    // ═══════════════════════════════════════
    // HELPER
    // ═══════════════════════════════════════

    private static Cobranca CriarCobrancaValida()
    {
        return new Cobranca("Mensalidade", 100.00m, DateTime.UtcNow.AddDays(30));
    }
}
