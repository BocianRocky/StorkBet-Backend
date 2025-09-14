using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
}