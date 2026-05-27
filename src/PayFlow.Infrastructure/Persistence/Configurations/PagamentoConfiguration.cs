using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayFlow.Domain.Entities;

namespace PayFlow.Infrastructure.Persistence.Configurations;

public class PagamentoConfiguration : IEntityTypeConfiguration<Pagamento>
{
    public void Configure(EntityTypeBuilder<Pagamento> builder)
    {
        builder.ToTable("pagamentos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ValorPago)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.CodigoTransacao)
            .HasMaxLength(100);

        builder.Property(p => p.MotivoRecusa)
            .HasMaxLength(500);

        builder.Property(p => p.Status)
            .IsRequired();

        builder.Property(p => p.CriadoEm)
            .IsRequired();

        builder.HasIndex(p => p.CobrancaId)
            .HasDatabaseName("ix_pagamentos_cobranca_id");
    }
}