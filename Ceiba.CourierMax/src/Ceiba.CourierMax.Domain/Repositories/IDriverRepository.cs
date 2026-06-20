using Ceiba.CourierMax.Domain.Entities;

namespace Ceiba.CourierMax.Domain.Repositories;

public interface IDriverRepository
{
    Task<Driver?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Driver>> GetAllActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Driver>> GetAllAsync(CancellationToken ct = default);
}
