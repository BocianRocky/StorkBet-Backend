using Domain.Interfaces;
using Application.DTOs;

namespace Application.Interfaces;

public interface IEventRepositoryExtended : IEventRepository
{
    Task<List<EventWithOddsDto>> GetEventsWithOddsBySportKeyAsync(string sportKey);
    Task<List<PopularEventDto>> GetPopularEventsAsync(int limit = 10, int daysAhead = 7);
}

