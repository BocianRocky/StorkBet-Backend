using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Sport = Domain.Entities.Sport;

namespace Infrastructure.Repositories;

public class SportRepository: ISportRepository
{
    private readonly AppDbContext _context;
    
    public SportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Sport>> GetAllAsync()
    {
        return await _context.Sports
            .Select(s => new Sport
            {
                Id = s.Id,
                Key = s.Key,
                Group = s.Group,
                Title = s.Title,
                Description = s.Description,
                HasOutrights = s.HasOutrights
            })
            .ToListAsync();
    }

    public Task<Sport?> GetByKeyAsync(string key)
    {
        var sport = _context.Sports
            .Where(s => s.Key == key)
            .Select(s => new Sport
            {
                Id = s.Id,
                Key = s.Key,
                Group = s.Group,
                Title = s.Title,
                Description = s.Description,
                HasOutrights = s.HasOutrights
            })
            .FirstOrDefaultAsync();
        return sport;
    }

    public async Task AddAsync(Sport sport)
    {
        var existing = await _context.Sports
            .FirstOrDefaultAsync(s => s.Key == sport.Key);

        if (existing != null)
        {
            existing.Group = sport.Group;
            existing.Title = sport.Title;
            existing.Description = sport.Description;
            existing.HasOutrights = sport.HasOutrights;
        }
        else
        {
            await _context.Sports.AddAsync(new Infrastructure.Data.Sport
            {
                Key = sport.Key,
                Group = sport.Group,
                Title = sport.Title,
                Description = sport.Description,
                HasOutrights = sport.HasOutrights
            });
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}