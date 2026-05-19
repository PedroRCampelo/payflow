using PayFlow.Domain.Enums;
using PayFlow.Domain.Exceptions;

namespace PayFlow.Domain.Entities;

public class Cobranca
{
    public Guid Id { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public StatusCobranca Status { get; private set; }
    public DateTime CriadaEm { get; private set; }
    public DateTime? ConfirmadaEm { get; private set; }
    public DateTime? CanceladaEm { get; private set; }
    public DateTime? PagaEm { get; private set; }
    public DateTime DataVencimento { get; private set; }

    // EF Core precisa de um construtor sem parâmetros (private pra ninguém usar fora)
    private Cobranca() { }

    // Construtor público com validações — quem cria uma Cobrança PRECISA passar dados válidos
    public Cobranca(string descricao, decimal valor, DateTime dataVencimento)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Descrição é obrigatória.");

        if (valor <= 0)
            throw new DomainException("Valor deve ser maior que zero.");

        if (dataVencimento.Date < DateTime.UtcNow.Date)
            throw new DomainException("Data de vencimento não pode ser no passado.");

        Id = Guid.NewGuid();
        Descricao = descricao.Trim();
        Valor = valor;
        Status = StatusCobranca.Pendente;
        DataVencimento = dataVencimento;
        CriadaEm = DateTime.UtcNow;
    }

    // ── Métodos de comportamento (isso é o domínio RICO) ──

    public void Confirmar()
    {
        if (Status != StatusCobranca.Pendente)
            throw new DomainException(
                $"Só é possível confirmar cobranças pendentes. Status atual: {Status}.");

        Status = StatusCobranca.Confirmada;
        ConfirmadaEm = DateTime.UtcNow;
    }

    public void RegistrarPagamento()
    {
        if (Status != StatusCobranca.Confirmada)
            throw new DomainException(
                $"Só é possível registrar pagamento de cobranças confirmadas. Status atual: {Status}.");

        Status = StatusCobranca.Paga;
        PagaEm = DateTime.UtcNow;
    }

    public void Cancelar()
    {
        if (Status == StatusCobranca.Paga)
            throw new DomainException("Não é possível cancelar uma cobrança já paga.");

        if (Status == StatusCobranca.Cancelada)
            throw new DomainException("Cobrança já está cancelada.");

        Status = StatusCobranca.Cancelada;
        CanceladaEm = DateTime.UtcNow;
    }

    public void MarcarComoVencida()
    {
        if (Status != StatusCobranca.Pendente && Status != StatusCobranca.Confirmada)
            throw new DomainException(
                $"Só cobranças pendentes ou confirmadas podem vencer. Status atual: {Status}.");

        Status = StatusCobranca.Vencida;
    }

    // ── Queries do domínio ──

    public bool EstaPendente() => Status == StatusCobranca.Pendente;
    public bool PodeSerCancelada() => Status != StatusCobranca.Paga && Status != StatusCobranca.Cancelada;
    public bool EstaVencida() => DataVencimento.Date < DateTime.UtcNow.Date && Status == StatusCobranca.Pendente;
}
