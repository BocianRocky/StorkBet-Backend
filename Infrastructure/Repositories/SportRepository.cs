using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using Infrastructure.Data;
namespace Infrastructure.Repositories;

public class SportRepository: ISportRepository
{
    private readonly AppDbContext _context;
    
    public SportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Domain.Entities.Sport>> GetAllAsync()
    {
        return await _context.Sports
            .Select(s => new Domain.Entities.Sport
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

    public Task<Domain.Entities.Sport?> GetByKeyAsync(string key)
    {
        var sport = _context.Sports
            .Where(s => s.Key == key)
            .Select(s => new Domain.Entities.Sport
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

    public async Task AddOrUpdateAsync(Domain.Entities.Sport sport)
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
            
            var newEntity = new Sport
            {
                Key = sport.Key,
                Group = sport.Group,
                Title = sport.Title,
                Description = sport.Description,
                HasOutrights = sport.HasOutrights
            };
            await _context.Sports.AddAsync(newEntity);


            
        }

        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    public async Task<List<SingleSportDto>> GetGroupedSportsAsync()
    {
        var sports = await _context.Sports
            .AsNoTracking()
            .Select(s => new SingleSportDto()
            {
                Title = s.Title,
                Group = s.Group,
                Key = s.Key
            })
            .ToListAsync();
        return sports;
    }
}