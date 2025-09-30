using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Application.DTOs;

namespace Infrastructure.Repositories;

public class PromotionRepository : IPromotionRepository
{
    private readonly AppDbContext _dbContext;

    public PromotionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CreateAsync(
        string promotionName,
        DateOnly dateStart,
        DateOnly dateEnd,
        string bonusType,
        decimal bonusValue,
        string? promoCode,
        decimal? minDeposit,
        decimal? maxDeposit,
        string image,
        string description)
        
    {
        var promotion = new Promotion
        {
            PromotionName = promotionName,
            DateStart = dateStart,
            DateEnd = dateEnd,
            BonusType = bonusType,
            BonusValue = bonusValue,
            PromoCode = promoCode,
            MinDeposit = minDeposit,
            MaxDeposit = maxDeposit,
            Image = image,
            Description = description
            
        };

        await _dbContext.Promotions.AddAsync(promotion);
        await _dbContext.SaveChangesAsync();
        return promotion.Id;
    }

    public async Task<IEnumerable<PromotionReadModel>> GetAllAsync()
    {
        return await _dbContext.Promotions
            .OrderByDescending(p => p.Id)
            .Select(p => new PromotionReadModel
            {
                Id = p.Id,
                PromotionName = p.PromotionName,
                DateStart = p.DateStart,
                DateEnd = p.DateEnd,
                BonusType = p.BonusType,
                BonusValue = p.BonusValue,
                PromoCode = p.PromoCode,
                MinDeposit = p.MinDeposit,
                MaxDeposit = p.MaxDeposit,
                Image = p.Image,
                Description = p.Description
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<PromotionReadModel>> GetTodayAsync(DateOnly today)
    {
        return await _dbContext.Promotions
            .Where(p => p.DateStart <= today && p.DateEnd >= today)
            .OrderBy(p => p.DateEnd)
            .Select(p => new PromotionReadModel
            {
                Id = p.Id,
                PromotionName = p.PromotionName,
                DateStart = p.DateStart,
                DateEnd = p.DateEnd,
                BonusType = p.BonusType,
                BonusValue = p.BonusValue,
                PromoCode = p.PromoCode,
                MinDeposit = p.MinDeposit,
                MaxDeposit = p.MaxDeposit,
                Image = p.Image,
                Description = p.Description
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<PromotionReadModel>> GetAvailableAsync(DateOnly today)
    {
        return await _dbContext.Promotions
            .Where(p => p.DateEnd >= today)
            .OrderBy(p => p.DateStart)
            .Select(p => new PromotionReadModel
            {
                Id = p.Id,
                PromotionName = p.PromotionName,
                DateStart = p.DateStart,
                DateEnd = p.DateEnd,
                BonusType = p.BonusType,
                BonusValue = p.BonusValue,
                PromoCode = p.PromoCode,
                MinDeposit = p.MinDeposit,
                MaxDeposit = p.MaxDeposit,
                Image = p.Image,
                Description = p.Description
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<PlayerPromotionReadModel>> GetPlayerPromotionsAsync(int playerId)
    {
        return await _dbContext.AvailablePromotions
            .Where(ap => ap.PlayerId == playerId)
            .Include(ap => ap.Promotion)
            .Select(ap => new PlayerPromotionReadModel
            {
                Id = ap.Promotion.Id,
                PromotionName = ap.Promotion.PromotionName,
                DateStart = ap.Promotion.DateStart,
                DateEnd = ap.Promotion.DateEnd,
                BonusType = ap.Promotion.BonusType,
                BonusValue = ap.Promotion.BonusValue,
                PromoCode = ap.Promotion.PromoCode,
                MinDeposit = ap.Promotion.MinDeposit,
                MaxDeposit = ap.Promotion.MaxDeposit,
                Availability = ap.Availability,
                Image = ap.Promotion.Image,
                Description = ap.Promotion.Description
            })
            .ToListAsync();
    }
}


