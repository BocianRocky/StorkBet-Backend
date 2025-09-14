using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class OddsSyncService : IOddsSyncService
{
    private readonly IOddsApiService _oddsApiService;
    private readonly ISportRepository _sportRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ITeamRepository _teamRepository;
    
    public OddsSyncService(IOddsApiService oddsApiService)
    {
        _oddsApiService = oddsApiService;
    }
    
    public async Task SyncEventsAndOddsAsync(string sportKey)
    {
        var events = await _oddsApiService.GetEventsAndOddsBySportAsync(sportKey);

        foreach (var ev in events)
        {
            var sport = await _sportRepository.GetByKeyAsync(sportKey);
            if (sport == null)
            {
                Console.WriteLine($"Sport {sportKey} not found in DB, skipping event {ev.Id}");
                continue;
            }
            var existingEventInDb = await _eventRepository.GetByApiIdAsync(ev.ApiId);

            if (existingEventInDb == null)
            {
                existingEventInDb=new Event
                {
                    ApiId = ev.ApiId,
                    SportId = sport.Id,
                    CommenceTime = ev.CommenceTime,
                    EventStatus = ev.EventStatus,
                    EventName = ev.EventName,
                    EndTime = ev.EndTime,
                    Odds = new List<Odds>()
                };
            }

            foreach (var odd in ev.Odds)
            {
                var teamInDb = await _teamRepository.GetByNameAsync(odd.Team.TeamName);
                if (teamInDb == null)
                {
                    teamInDb = new Team
                    {
                        TeamName = odd.Team.TeamName,
                        SportId = sport.Id
                    };
                    await _teamRepository.AddAsync(teamInDb);
                }
                var newOdd = new Odds
                {
                    OddsValue = odd.OddsValue,
                    LastUpdate = DateTime.UtcNow,
                    Team = teamInDb
                };
                
                existingEventInDb.Odds.Add(newOdd);
            }
            
            
            
            
            
            
        }
        await _teamRepository.SaveChangesAsync();
        await _eventRepository.SaveChangesAsync();
    }
    
}