using Application.DTOs;

namespace Application.Interfaces;

public interface IEventService
{
    Task<List<PopularEventDto>> GetPopularEventsAsync(int limit = 10, int daysAhead = 7);
}

