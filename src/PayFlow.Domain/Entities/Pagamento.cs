using PayFlow.Domain.Enums;
using PayFlow.Domain.Exceptions;

namespace PayFlow.Domain.Entities;

public class Pagamento
{
    public Guid Id { get; private set; }
    public Guid CobrancaId { get; private set; }
    public decimal ValorPago { get; private set; }
    public StatusPagamento Status { get; private set; }
    public string? CodigoTransacao { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? AprovadoEm { get; private set; }
    public DateTime? RecusadoEm { get; private set; }
    public DateTime? EstornadoEm { get; private set; }
    public string? MotivoRecusa { get; private set; }

    private Pagamento() { }

    public Pagamento(Guid cobrancaId, decimal valorPago, string? codigoTransacao = null)
    {
        if (cobrancaId == Guid.Empty)
            throw new DomainException("Charge ID is required.");

        if (valorPago <= 0)
            throw new DomainException("Payment amount must be greater than zero.");

        Id = Guid.NewGuid();
        CobrancaId = cobrancaId;
        ValorPago = valorPago;
        CodigoTransacao = codigoTransacao;
        Status = StatusPagamento.Processando;
        CriadoEm = DateTime.UtcNow;
    }

    public void Aprovar()
    {
        if (Status != StatusPagamento.Processando)
            throw new DomainException(
                $"Only processing payments can be approved. Current status: {Status}.");

        Status = StatusPagamento.Aprovado;
        AprovadoEm = DateTime.UtcNow;
    }

    public void Recusar(string motivo)
    {
        if (Status != StatusPagamento.Processando)
            throw new DomainException(
                $"Only processing payments can be declined. Current status: {Status}.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new DomainException("Decline reason is required.");

        Status = StatusPagamento.Recusado;
        MotivoRecusa = motivo.Trim();
        RecusadoEm = DateTime.UtcNow;
    }

    public void Estornar()
    {
        if (Status != StatusPagamento.Aprovado)
            throw new DomainException(
                $"Only approved payments can be refunded. Current status: {Status}.");

        Status = StatusPagamento.Estornado;
        EstornadoEm = DateTime.UtcNow;
    }

    public bool PodeSerEstornado() => Status == StatusPagamento.Aprovado;
}