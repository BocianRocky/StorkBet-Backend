using Domain.Entities;

namespace Application.Interfaces;

public interface ITeamRepository
{
    Task<Team?> GetByNameAsync(string name);
    Task AddAsync(Team team);
    Task SaveChangesAsync();
}