namespace Application.Interfaces;
using Domain.Entities;

public interface ISportRepository
{
    Task<IEnumerable<Sport>> GetAllAsync();
    Task<Sport?> GetByKeyAsync(string key);
    Task AddAsync(Sport sport);
    Task SaveChangesAsync();
}