using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Domain.Repositories;

public interface ICobrancaRepository
{
    Task<Cobranca?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cobranca>> ListarAsync(StatusCobranca? status = null, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Cobranca cobranca, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Cobranca cobranca, CancellationToken cancellationToken = default);
}

// Explanation: This is just a list of promises. "Whoever implements this will know how to
// search, list, add, and update charges."
// It doesn't say how. There's no real code. It's like a menu lists what's available, but doesn't cook anything.

