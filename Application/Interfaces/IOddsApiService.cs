using Domain.Entities;

namespace Application.Interfaces;

public interface IOddsApiService
{
    Task<IEnumerable<Sport>> GetSportsAsync();
}