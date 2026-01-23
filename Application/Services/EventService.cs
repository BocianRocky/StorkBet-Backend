using Application.Interfaces;
using Application.DTOs;

namespace Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepositoryExtended _eventRepository;

    public EventService(IEventRepositoryExtended eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<List<PopularEventDto>> GetPopularEventsAsync(int limit = 10, int daysAhead = 7)
    {
        return await _eventRepository.GetPopularEventsAsync(limit, daysAhead);
    }
}

