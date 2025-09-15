using Domain.Entities;

namespace Application.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByApiIdAsync(string apiId);
    Task AddAsync(Event ev);
    Task SaveChangesAsync();
    
    Task AddOddAsync(Odds odd);
    
    Task<Odds?> GetOddByEventAndTeamAsync(int eventId, int teamId);
    Task UpdateOddAsync(int oddId, decimal newOddsValue, DateTime lastUpdate);
}