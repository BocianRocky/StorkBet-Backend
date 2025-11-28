using Application.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Event = Infrastructure.Data.Event;
using Team = Infrastructure.Data.Team;

namespace Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Event?> GetByApiIdAsync(string apiId)
    {
        var ev = await _context.Events
            .Include(e => e.Odds)
            .ThenInclude(o => o.Team)
            .FirstOrDefaultAsync(e => e.ApiId == apiId);
        
        if (ev == null) return null;
        return new Domain.Entities.Event
        {
            Id = ev.Id,
            ApiId = ev.ApiId,
            EventName = ev.EventName,
            EventStatus = ev.EventStatus,
            CommenceTime = ev.EventDate,
            EndTime = ev.EventDateEnd,
            Odds = ev.Odds.Select(o => new Domain.Entities.Odds
            {
                Id = o.Id,
                OddsValue = o.OddsValue,
                LastUpdate = o.LastUpdate,
                Team = new Domain.Entities.Team
                {
                    Id = o.Team.Id,
                    TeamName = o.Team.TeamName
                }
            }).ToList()
        };
    }

    public async Task AddAsync(Domain.Entities.Event ev)
    {
        // Mapowanie Domain -> Infrastructure
        var newEvent = new Event
        {
            ApiId = ev.ApiId,
            EventName = ev.EventName,
            EventStatus = ev.EventStatus,
            EventDate = ev.CommenceTime,
            EventDateEnd = ev.EndTime,
            SportId = ev.SportId,
            Odds = ev.Odds.Select(o => new Odd
            {
                OddsValue = o.OddsValue,
                LastUpdate = o.LastUpdate,
                Team = new Team
                {
                    TeamName = o.Team.TeamName,
                    SportId = ev.SportId
                }
            }).ToList()
        };

        await _context.Events.AddAsync(newEvent);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task AddOddAsync(Odds odd)
    {
        await _context.Odds.AddAsync(new Odd
        {
            OddsValue = odd.OddsValue,
            LastUpdate = odd.LastUpdate,
            TeamId = odd.TeamId,
            EventId = odd.EventId
        });
    }
    
    public async Task<Odds?> GetOddByEventAndTeamAsync(int eventId, int teamId)
    {
        var entityOdd = await _context.Odds
            .FirstOrDefaultAsync(o => o.EventId == eventId && o.TeamId == teamId);

        if (entityOdd == null) return null;
        
        return new Odds
        {
            Id = entityOdd.Id,
            EventId = entityOdd.EventId,
            OddsValue = entityOdd.OddsValue,
            LastUpdate = entityOdd.LastUpdate,
            Team = new Domain.Entities.Team { Id = entityOdd.TeamId, TeamName = entityOdd.Team.TeamName }
        };
    }
    public async Task UpdateOddAsync(int oddId, decimal newOddsValue, DateTime lastUpdate)
    {
        
        var oddEntity = await _context.Odds
            .FirstOrDefaultAsync(o => o.Id == oddId);

        if (oddEntity == null)
        {
            throw new Exception($"Nie znaleziono kursu o Id {oddId}.");
        }

        
        oddEntity.OddsValue = newOddsValue;
        oddEntity.LastUpdate = lastUpdate;
        
    }

    public async Task UpdateOddScoreAsync(int oddId, int score)
    {
        var oddEntity = await _context.Odds
            .FirstOrDefaultAsync(o => o.Id == oddId);

        if (oddEntity == null)
        {
            throw new Exception($"Nie znaleziono kursu o Id {oddId}.");
        }

        oddEntity.Wynik = score;
    }
    

    public async Task<List<EventWithOddsDto>> GetEventsWithOddsBySportKeyAsync(string sportKey)
    {
        return await _context.Events
            .Include(e => e.Odds)
            .ThenInclude(o => o.Team)
            .Include(e => e.Sport)
            .Where(e => e.Sport.Key == sportKey)
            .Where(e => e.EventDate > DateTime.UtcNow)
            .OrderBy(e => e.EventDate)
            .Select(e => new EventWithOddsDto
            {
                EventId = e.Id,
                EventDate = e.EventDate,
                Odds = e.Odds.Select(o => new OddDto
                {
                    OddId = o.Id,
                    TeamName = o.Team.TeamName,
                    OddsValue = o.OddsValue
                }).ToList()
            })
            .ToListAsync();
    }
    public async Task<Domain.Entities.Event?> GetByHomeAwayTeamDateAsync(string homeTeam, string awayTeam, DateTime commenceTime)
    {
        // Porównujemy tylko datę (bez godziny) i nazwy drużyn
        var commenceDate = commenceTime.Date;
        
        var ev = await _context.Events
            .Include(e => e.Odds)
            .ThenInclude(o => o.Team)
            .FirstOrDefaultAsync(e => e.EventDate.Date == commenceDate &&
                                      e.Odds.Any(o => o.Team.TeamName == homeTeam) &&
                                      e.Odds.Any(o => o.Team.TeamName == awayTeam));
        
        if (ev == null) 
        {
            return null;
        }
        
        Console.WriteLine("Znaleziono wydarzenie w bazie danych dla drużyn: " + ev.EventName +  " dnia " + ev.EventDate);
        return new Domain.Entities.Event
        {
            Id = ev.Id,
            ApiId = ev.ApiId,
            EventName = ev.EventName,
            EventStatus = ev.EventStatus,
            CommenceTime = ev.EventDate,
            EndTime = ev.EventDateEnd,
            Odds = ev.Odds.Select(o => new Domain.Entities.Odds
            {
                Id = o.Id,
                OddsValue = o.OddsValue,
                LastUpdate = o.LastUpdate,
                Team = new Domain.Entities.Team
                {
                    Id = o.Team.Id,
                    TeamName = o.Team.TeamName
                }
            }).ToList()
        };
    }

    public async Task MarkEventAsCompletedAsync(int eventId)
    {
        var ev = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId);
        
        if (ev != null)
        {
            ev.IsCompleted = true;
            _context.Events.Update(ev);
        }
    }

    public async Task<List<PopularEventDto>> GetPopularEventsAsync(int limit = 10, int daysAhead = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        
        var eventsQuery = _context.Events
            .Include(e => e.Odds)
                .ThenInclude(o => o.Team)
            .Include(e => e.Odds)
                .ThenInclude(o => o.BetSlipOdds)
            .Include(e => e.Sport)
            .Where(e => e.EventDate > DateTime.UtcNow && e.EventDate <= cutoffDate)
            .Where(e => e.IsCompleted != true);

        var events = await eventsQuery.ToListAsync();

        var eventsWithBetCounts = events
            .Select(e => new
            {
                Event = e,
                BetCount = e.Odds.Sum(o => o.BetSlipOdds.Count)
            })
            .OrderByDescending(x => x.BetCount)
            .ThenBy(x => x.Event.EventDate)
            .Take(limit)
            .ToList();

        return eventsWithBetCounts.Select(x => new PopularEventDto
        {
            EventId = x.Event.Id,
            EventName = x.Event.EventName,
            EventDate = x.Event.EventDate,
            SportId = x.Event.SportId,
            SportKey = x.Event.Sport.Key,
            SportTitle = x.Event.Sport.Title,
            SportGroup = x.Event.Sport.Group,
            Odds = x.Event.Odds.Select(o => new OddDto
            {
                OddId = o.Id,
                TeamName = o.Team.TeamName,
                OddsValue = o.OddsValue
            }).ToList(),
            BetCount = x.BetCount
        }).ToList();
    }
}