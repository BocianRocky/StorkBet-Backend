using System.Data.Common;
using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces;


public interface ISportRepository
{
    Task<IEnumerable<Sport>> GetAllAsync();
    Task<Sport?> GetByKeyAsync(string key);
    Task AddOrUpdateAsync(Sport sport);
    Task SaveChangesAsync();
    Task<List<SingleSportDto>> GetGroupedSportsAsync();
}