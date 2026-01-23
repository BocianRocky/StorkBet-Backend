using Domain.Interfaces;
using Application.DTOs;

namespace Application.Interfaces;

public interface ISportRepositoryExtended : ISportRepository
{
    Task<List<SingleSportDto>> GetGroupedSportsAsync();
}

