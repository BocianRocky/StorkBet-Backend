namespace Application.Interfaces;

public interface IScoreSyncService
{
    Task SyncScoresBySportAsync(string sportKey, int daysFrom = 3);
    Task SyncScoresForAllSportsAsync(int daysFrom = 3);
}

