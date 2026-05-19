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
