using PayFlow.Domain.Enums;
using PayFlow.Domain.Exceptions;
using PayFlow.Domain.ValueObjects;

namespace PayFlow.Domain.Entities;

public class Cobranca
{
    public Guid Id { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public Dinheiro Valor { get; private set; } = null!;
    public StatusCobranca Status { get; private set; }
    public DateTime CriadaEm { get; private set; }
    public DateTime? ConfirmadaEm { get; private set; }
    public DateTime? CanceladaEm { get; private set; }
    public DateTime? PagaEm { get; private set; }
    public DateTime DataVencimento { get; private set; }

    private readonly List<Pagamento> _pagamentos = [];
    public IReadOnlyCollection<Pagamento> Pagamentos => _pagamentos.AsReadOnly();

    // Required by EF Core for object materialization
    private Cobranca() { }

    public Cobranca(string descricao, Dinheiro valor, DateTime dataVencimento)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Description is required.");

        if (valor.Valor <= 0)
            throw new DomainException("Amount must be greater than zero.");

        if (dataVencimento.Date < DateTime.UtcNow.Date)
            throw new DomainException("Due date cannot be in the past.");

        Id = Guid.NewGuid();
        Descricao = descricao.Trim();
        Valor = valor;
        Status = StatusCobranca.Pendente;
        DataVencimento = dataVencimento;
        CriadaEm = DateTime.UtcNow;
    }

    // Behavior methods

    public void Confirmar()
    {
        if (Status != StatusCobranca.Pendente)
            throw new DomainException(
                $"Only pending charges can be confirmed. Current status: {Status}.");

        Status = StatusCobranca.Confirmada;
        ConfirmadaEm = DateTime.UtcNow;
    }

    public Pagamento AdicionarPagamento(Dinheiro valorPago, string? codigoTransacao = null)
    {
        if (Status != StatusCobranca.Confirmada)
            throw new DomainException(
                $"Payments can only be added to confirmed charges. Current status: {Status}.");

        if (valorPago.Moeda != Valor.Moeda)
            throw new DomainException(
                $"Payment currency ({valorPago.Moeda}) must match charge currency ({Valor.Moeda}).");

        var pagamento = new Pagamento(Id, valorPago.Valor, codigoTransacao);
        _pagamentos.Add(pagamento);
        return pagamento;
    }

    public void RegistrarPagamento()
    {
        if (Status != StatusCobranca.Confirmada)
            throw new DomainException(
                $"Only confirmed charges can be marked as paid. Current status: {Status}.");

        Status = StatusCobranca.Paga;
        PagaEm = DateTime.UtcNow;
    }

    public void Cancelar()
    {
        if (Status == StatusCobranca.Paga)
            throw new DomainException("Cannot cancel a charge that has already been paid.");

        if (Status == StatusCobranca.Cancelada)
            throw new DomainException("Charge is already cancelled.");

        Status = StatusCobranca.Cancelada;
        CanceladaEm = DateTime.UtcNow;
    }

    public void MarcarComoVencida()
    {
        if (Status != StatusCobranca.Pendente && Status != StatusCobranca.Confirmada)
            throw new DomainException(
                $"Only pending or confirmed charges can be marked as overdue. Current status: {Status}.");

        Status = StatusCobranca.Vencida;
    }

    // Domain queries

    public bool EstaPendente() => Status == StatusCobranca.Pendente;
    public bool PodeSerCancelada() => Status != StatusCobranca.Paga && Status != StatusCobranca.Cancelada;
    public bool EstaVencida() => DataVencimento.Date < DateTime.UtcNow.Date && Status == StatusCobranca.Pendente;
}