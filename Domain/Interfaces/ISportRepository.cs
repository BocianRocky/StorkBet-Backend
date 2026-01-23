using Domain.Entities;

namespace Domain.Interfaces;

public interface ISportRepository
{
    Task<IEnumerable<Sport>> GetAllAsync();
    Task<Sport?> GetByKeyAsync(string key);
    Task AddOrUpdateAsync(Sport sport);
    Task SaveChangesAsync();
}

