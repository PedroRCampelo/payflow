using Microsoft.EntityFrameworkCore;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;
using PayFlow.Domain.Repositories;

namespace PayFlow.Infrastructure.Persistence.Repositories;

public class CobrancaRepository : ICobrancaRepository
{
    private readonly PayFlowDbContext _context;

    public CobrancaRepository(PayFlowDbContext context)
    {
        _context = context;
    }

    public async Task<Cobranca?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Cobrancas
            .Include(c => c.Pagamentos)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Cobranca>> ListarAsync(StatusCobranca? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Cobrancas.AsQueryable();

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        return await query
            .OrderByDescending(c => c.CriadaEm)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Cobranca cobranca, CancellationToken cancellationToken = default)
    {
        await _context.Cobrancas.AddAsync(cobranca, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Cobranca cobranca, CancellationToken cancellationToken = default)
    {
        _context.Cobrancas.Update(cobranca);
        await _context.SaveChangesAsync(cancellationToken);
    }
}