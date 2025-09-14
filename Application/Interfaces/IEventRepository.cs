using Domain.Entities;

namespace Application.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByApiIdAsync(string apiId);
    Task AddAsync(Event ev);
    Task SaveChangesAsync();
}