namespace Domain.Interfaces;

public interface IBetSlipRepository
{
    Task<int> CreateBetSlipAsync(int playerId, decimal amount, IEnumerable<int> oddsIds, int? availablePromotionId);
    Task<IEnumerable<object>> GetPlayerBetSlipsAsync(int playerId);
    Task<object?> GetBetSlipDetailsAsync(int betSlipId, int playerId);
    
    // Automatyczne sprawdzanie wyników betslipów
    Task CheckAndUpdateAllBetSlipsResultsAsync();
}

