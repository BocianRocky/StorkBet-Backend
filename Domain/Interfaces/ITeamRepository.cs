using Domain.Entities;

namespace Domain.Interfaces;

public interface ITeamRepository
{
    Task<Team?> GetByNameAsync(string name);
    Task AddAsync(Team team);
    Task SaveChangesAsync();
}

