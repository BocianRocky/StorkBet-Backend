using Application.Interfaces;
using Application.DTOs;
using Domain.Entities;

namespace Application.Services;

public class OddsService:IOddsService
{
    private readonly IEventRepositoryExtended _eventRepository;
    
    public OddsService(IEventRepositoryExtended eventRepository)
    {
        _eventRepository = eventRepository;
    }
    

    public async Task<List<EventWithOddsDto>> GetEventsWithOddsBySportAsync(string sportKey)
    {
        var eventsWithOdds = await _eventRepository.GetEventsWithOddsBySportKeyAsync(sportKey);
        return eventsWithOdds;
    }
}