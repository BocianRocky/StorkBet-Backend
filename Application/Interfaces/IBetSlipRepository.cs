using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IBetSlipRepository
{
    Task<int> CreateBetSlipAsync(int playerId, decimal amount, IEnumerable<int> oddsIds);
    Task<IEnumerable<object>> GetPlayerBetSlipsAsync(int playerId);
    Task<object?> GetBetSlipDetailsAsync(int betSlipId, int playerId);
}


