using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
namespace Application.Services;

public class SportService: ISportService
{
    private ISportRepository _repository;
    
    public SportService(ISportRepository repository)
    {
        _repository = repository;
    }
    public async Task<IEnumerable<Sport>> GetAllSportsAsync()
    {
        return await _repository.GetAllAsync();
    }
    

    public async Task AddOrUpdateSportAsync(Sport sport)
    {
        await _repository.AddOrUpdateAsync(sport);
        await _repository.SaveChangesAsync();
    }

    public async Task<List<GroupSportDto>> GetGroupedSportsAsync()
    {
        var groupedSports =await _repository.GetGroupedSportsAsync();
        var grouped = groupedSports
            .GroupBy(s => s.Group)
            .Select(g => new GroupSportDto
            {
                Group = g.Key,
                Sports = g.Select(s=>new SingleSportDto
                {
                    Title = s.Title,
                    Key = s.Key,
                    Group = s.Group
                }).ToList()
            })
            .ToList();
        return grouped;
    }
}