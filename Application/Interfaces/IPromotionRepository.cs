using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces;

public interface IPromotionRepository
{
    Task<int> CreateAsync(
        string promotionName,
        DateOnly dateStart,
        DateOnly dateEnd,
        string bonusType,
        decimal bonusValue,
        string? promoCode,
        decimal? minDeposit,
        decimal? maxDeposit,
        string image,
        string description);
    Task<IEnumerable<PromotionReadModel>> GetAllAsync();
    Task<IEnumerable<PromotionReadModel>> GetTodayAsync(DateOnly today);
    Task<IEnumerable<PromotionReadModel>> GetAvailableAsync(DateOnly today);
    Task<IEnumerable<PlayerPromotionReadModel>> GetPlayerPromotionsAsync(int playerId);
}


