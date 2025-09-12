using Application.DTOs;

namespace Application.Interfaces;
using Domain.Entities;

public interface ISportService
{
    Task<IEnumerable<Sport>> GetAllSportsAsync();
    Task AddOrUpdateSportAsync(Sport sport);
    Task<List<GroupSportDto>> GetGroupedSportsAsync();
}