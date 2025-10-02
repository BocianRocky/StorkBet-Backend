using Domain.Entities;
using Application.DTOs;

namespace Application.Interfaces;

public interface IOddsApiService
{
    Task<IEnumerable<Sport>> GetSportsAsync();
    Task<IEnumerable<Event>> GetEventsAndOddsBySportAsync(string sportKey);
    Task<IEnumerable<ScoreApiResponseDto>> GetScoresBySportAsync(string sportKey, int daysFrom = 3);
}