using Application.DTOs;
using Application.Interfaces;

namespace Application.Services;

public class ScoreSyncService : IScoreSyncService
{
    private readonly IOddsApiService _oddsApiService;
    private readonly IEventRepository _eventRepository;
    private readonly ISportRepository _sportRepository;
    private readonly IBetSlipRepository _betSlipRepository;

    public ScoreSyncService(
        IOddsApiService oddsApiService,
        IEventRepository eventRepository,
        ISportRepository sportRepository,
        IBetSlipRepository betSlipRepository)
    {
        _oddsApiService = oddsApiService;
        _eventRepository = eventRepository;
        _sportRepository = sportRepository;
        _betSlipRepository = betSlipRepository;
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
            
            Console.WriteLine("wydarzenie: "+eventEntity.EventName);

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
            
            // Oznacz event jako zakończony po zaktualizowaniu wyników
            await _eventRepository.MarkEventAsCompletedAsync(eventEntity.Id);
        }

        await _eventRepository.SaveChangesAsync();
        
        // Po zakończeniu synchronizacji wyników, sprawdź betslipy
        await _betSlipRepository.CheckAndUpdateAllBetSlipsResultsAsync();
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
