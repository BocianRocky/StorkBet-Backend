namespace Application.Interfaces;

public interface IOddsSyncService
{
    Task SyncEventsAndOddsAsync(string sportKey);
}