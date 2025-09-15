using Domain.Entities;

namespace Application.Interfaces;

public interface IOddsService
{
    Task<List<Odds>> GetOddsBySportAsync(string sportKey);
}