using Microsoft.EntityFrameworkCore;
using PayFlow.Domain.Entities;

namespace PayFlow.Infrastructure.Persistence;

public class PayFlowDbContext : DbContext
{
    public DbSet<Cobranca> Cobrancas { get; set; }
    public DbSet<Pagamento> Pagamentos { get; set; }

    public PayFlowDbContext(DbContextOptions<PayFlowDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PayFlowDbContext).Assembly);
    }
}