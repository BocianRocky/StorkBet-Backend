using Domain.Entities;
using Application.DTOs;

namespace Application.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByApiIdAsync(string apiId);
    Task AddAsync(Event ev);
    Task SaveChangesAsync();
    
    Task AddOddAsync(Odds odd);
    
    Task<Odds?> GetOddByEventAndTeamAsync(int eventId, int teamId);
    Task UpdateOddAsync(int oddId, decimal newOddsValue, DateTime lastUpdate);
    Task UpdateOddScoreAsync(int oddId, int score);
    
    Task<List<EventWithOddsDto>> GetEventsWithOddsBySportKeyAsync(string sportKey);

    Task<Event?>GetByHomeAwayTeamDateAsync(string homeTeam, string awayTeam, DateTime commenceTime);
    
    Task MarkEventAsCompletedAsync(int eventId);
    
    Task<List<PopularEventDto>> GetPopularEventsAsync(int limit = 10, int daysAhead = 7);
}