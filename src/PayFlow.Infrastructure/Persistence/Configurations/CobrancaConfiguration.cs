using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayFlow.Domain.Entities;

namespace PayFlow.Infrastructure.Persistence.Configurations;

public class CobrancaConfiguration : IEntityTypeConfiguration<Cobranca>
{
    public void Configure(EntityTypeBuilder<Cobranca> builder)
    {
        builder.ToTable("cobrancas");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Descricao)
            .IsRequired()
            .HasMaxLength(200);

        // Maps the Dinheiro value object as two columns in the same table
        builder.OwnsOne(c => c.Valor, valor =>
        {
            valor.Property(v => v.Valor)
                .HasColumnName("valor")
                .HasPrecision(18, 2)
                .IsRequired();

            valor.Property(v => v.Moeda)
                .HasColumnName("moeda")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(c => c.Status)
            .IsRequired();

        builder.Property(c => c.CriadaEm)
            .IsRequired();

        builder.Property(c => c.DataVencimento)
            .IsRequired();

        // Relationship: Cobranca has many Pagamentos
        builder.HasMany(c => c.Pagamentos)
            .WithOne()
            .HasForeignKey(p => p.CobrancaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Partial index: only pending charges (useful for queries filtering by status)
        builder.HasIndex(c => c.Status)
            .HasFilter("\"Status\" = 1")
            .HasDatabaseName("ix_cobrancas_status_pendente");
    }
}