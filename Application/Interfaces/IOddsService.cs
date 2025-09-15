using Domain.Entities;
using Application.DTOs;

namespace Application.Interfaces;

public interface IOddsService
{
    Task<List<EventWithOddsDto>> GetEventsWithOddsBySportAsync(string sportKey);
}