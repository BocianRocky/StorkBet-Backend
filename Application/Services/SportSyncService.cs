using Application.Interfaces;

namespace Application.Services;

public class SportSyncService: ISportSyncService
{
    private readonly ISportRepository _repository;
    private readonly IOddsApiService _oddsApiService;
    
    public SportSyncService(ISportRepository repository, IOddsApiService oddsApiService)
    {
        _repository = repository;
        _oddsApiService = oddsApiService;
    }
    public async Task SyncSportsAsync()
    {
        // pobranie z OddsAPI
        var sportsFromApi = await _oddsApiService.GetSportsAsync();

        foreach (var sport in sportsFromApi)
        {
            // dodanie lub aktualizacja w bazie danych
            await _repository.AddOrUpdateAsync(sport);
        }

        await _repository.SaveChangesAsync();
    }
}