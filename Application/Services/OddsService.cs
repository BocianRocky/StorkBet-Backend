using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class OddsService:IOddsService
{
    private readonly IEventRepository _eventRepository;
    
    public OddsService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }
    
    public async Task<List<Odds>> GetOddsBySportAsync(string sportKey)
    {
        var odds = await _eventRepository.GetOddsBySportKeyAsync(sportKey);
        return odds;
    }
}