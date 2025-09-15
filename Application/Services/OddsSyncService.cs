using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class OddsSyncService : IOddsSyncService
{
    private readonly IOddsApiService _oddsApiService;
    private readonly ISportRepository _sportRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ITeamRepository _teamRepository;
    
    public OddsSyncService(IOddsApiService oddsApiService, ISportRepository sportRepository, IEventRepository eventRepository, ITeamRepository teamRepository)
    {
        _sportRepository = sportRepository;
        _eventRepository = eventRepository;
        _teamRepository = teamRepository;
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
                    EventName = ev.EventName ?? "Unknown Event",
                    EndTime = ev.EndTime,
                };
                await _eventRepository.AddAsync(existingEventInDb);
            }
            else
            {
                existingEventInDb.EventStatus = ev.EventStatus;
                existingEventInDb.EndTime = ev.EndTime;
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
                var existingOdd =await _eventRepository.GetOddByEventAndTeamAsync(existingEventInDb.Id, teamInDb.Id);
                
                if (existingOdd != null)
                {
                    await _eventRepository.UpdateOddAsync(existingOdd.Id, odd.OddsValue, DateTime.UtcNow);
                }
                var newOdd = new Odds
                {
                    OddsValue = odd.OddsValue,
                    LastUpdate = DateTime.UtcNow,
                    TeamId = teamInDb.Id,
                    EventId = existingEventInDb.Id
                };
                await _eventRepository.AddOddAsync(newOdd);
                



            }
            
            
            
            
            
            
        }
        await _teamRepository.SaveChangesAsync();
        await _eventRepository.SaveChangesAsync();
    }
    
}