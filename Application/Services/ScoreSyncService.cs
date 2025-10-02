using Application.DTOs;
using Application.Interfaces;

namespace Application.Services;

public class ScoreSyncService : IScoreSyncService
{
    private readonly IOddsApiService _oddsApiService;
    private readonly IEventRepository _eventRepository;
    private readonly ISportRepository _sportRepository;

    public ScoreSyncService(
        IOddsApiService oddsApiService,
        IEventRepository eventRepository,
        ISportRepository sportRepository)
    {
        _oddsApiService = oddsApiService;
        _eventRepository = eventRepository;
        _sportRepository = sportRepository;
    }

    public async Task SyncScoresBySportAsync(string sportKey, int daysFrom = 3)
    {
        // Pobierz wyniki z API
        var scores = await _oddsApiService.GetScoresBySportAsync(sportKey, daysFrom);
        
        
        foreach (var scoreResponse in scores)
        {
            if (!scoreResponse.Completed)
            {
                continue;
            }
            
            var eventEntity = await _eventRepository.GetByHomeAwayTeamDateAsync(scoreResponse.HomeTeam, scoreResponse.AwayTeam, scoreResponse.CommenceTime);
            
            if (eventEntity == null)
            {
                continue;
            }
            
            

            // Aktualizuj wyniki dla każdego zespołu
            foreach (var score in scoreResponse.Scores)
            {
                if (!int.TryParse(score.Score, out var scoreValue))
                {
                    continue;
                }

                // Znajdź odds dla tego zespołu w tym evencie
                var teamOdds = eventEntity.Odds.FirstOrDefault(o => o.Team.TeamName == score.Name);
                if (teamOdds != null)
                {
                    await _eventRepository.UpdateOddScoreAsync(teamOdds.Id, scoreValue);
                }
            }
        }

        await _eventRepository.SaveChangesAsync();
    }

    public async Task SyncScoresForAllSportsAsync(int daysFrom = 3)
    {
        // Pobierz wszystkie sporty z API
        var sports = await _oddsApiService.GetSportsAsync();

        foreach (var sport in sports)
        {
            try
            {
                await SyncScoresBySportAsync(sport.Key, daysFrom);
            }
            catch
            {
                // Kontynuuj z następnym sportem
            }
        }
    }
}
