using System.Collections.Concurrent;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;
using PayFlow.Domain.Repositories;

namespace PayFlow.Infrastructure.Repositories;

// Temporary Repository
public class InMemoryCobrancaRepository : ICobrancaRepository
{
    private readonly ConcurrentDictionary<Guid, Cobranca> _store = new(); // ?

    public Task<Cobranca?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var cobranca);
        return Task.FromResult(cobranca);
    }

    public Task<IReadOnlyList<Cobranca>> ListarAsync(StatusCobranca? status = null, CancellationToken cancellationToken = default)
    {
        var query = _store.Values.AsEnumerable();

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        IReadOnlyList<Cobranca> result = query
            .OrderByDescending(c => c.CriadaEm)
            .ToList();

        return Task.FromResult(result);
    }

    public Task AdicionarAsync(Cobranca cobranca, CancellationToken cancellationToken = default)
    {
        _store[cobranca.Id] = cobranca;
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Cobranca cobranca, CancellationToken cancellationToken = default)
    {
        _store[cobranca.Id] = cobranca;
        return Task.CompletedTask;
    }
}