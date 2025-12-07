using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BetSlipRepository : IBetSlipRepository
{
    private readonly AppDbContext _dbContext;

    public BetSlipRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CreateBetSlipAsync(int playerId, decimal amount, IEnumerable<int> oddsIds, int? availablePromotionId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var player = await _dbContext.Players.FirstOrDefaultAsync(p => p.Id == playerId);
        if (player == null)
        {
            throw new KeyNotFoundException("Player not found");
        }

        if (amount <= 0)
        {
            throw new InvalidOperationException("Amount must be positive");
        }

        if (player.AccountBalance < amount)
        {
            throw new InvalidOperationException("Insufficient balance");
        }

        var oddsIdList = oddsIds?.Distinct().ToList() ?? new List<int>();
        if (oddsIdList.Count == 0)
        {
            throw new InvalidOperationException("At least one odd must be provided");
        }

        var odds = await _dbContext.Odds
            .Where(o => oddsIdList.Contains(o.Id))
            .ToListAsync();

        if (odds.Count != oddsIdList.Count)
        {
            throw new KeyNotFoundException("Some odds were not found");
        }

        AvailablePromotion? availablePromotion = null;
        if (availablePromotionId.HasValue)
        {
            
            availablePromotion = await _dbContext.AvailablePromotions
                .Include(ap => ap.Promotion)
                .FirstOrDefaultAsync(ap => ap.Id == availablePromotionId.Value && ap.PlayerId == playerId);

            
            if (availablePromotion == null)
            {
                availablePromotion = await _dbContext.AvailablePromotions
                    .Include(ap => ap.Promotion)
                    .FirstOrDefaultAsync(ap => ap.PromotionId == availablePromotionId.Value && ap.PlayerId == playerId);
            }

            
            if (availablePromotion == null)
            {
                var allPromotionsForPlayer = await _dbContext.AvailablePromotions
                    .Where(ap => ap.PlayerId == playerId)
                    .Select(ap => new { ap.Id, ap.PromotionId, ap.Availability })
                    .ToListAsync();
                
                var errorMessage = $"Promotion with ID {availablePromotionId.Value} not found for this player. ";
                
                var promotionExists = await _dbContext.Promotions
                    .FirstOrDefaultAsync(p => p.Id == availablePromotionId.Value);
                
                if (promotionExists != null)
                {
                    errorMessage += $"PromotionId {availablePromotionId.Value} exists but is not assigned to your account. Use /api/Promotions/redeem to redeem it first. ";
                }
                
                errorMessage += $"Available promotions for PlayerId {playerId}: [{string.Join(", ", allPromotionsForPlayer.Select(p => $"AvailablePromotionId={p.Id} (PromotionId={p.PromotionId}, Availability={p.Availability})"))}]";
                
                throw new KeyNotFoundException(errorMessage);
            }
            
            var validationErrors = new List<string>();
            var diagnosticInfo = new List<string>
            {
                $"AvailablePromotionId: {availablePromotion.Id}",
                $"PlayerId: {playerId}",
                $"PromotionId: {availablePromotion.PromotionId}",
                $"Availability (raw): '{availablePromotion.Availability}'",
                $"Availability (trimmed): '{availablePromotion.Availability?.Trim()}'"
            };

            var availability = availablePromotion.Availability?.Trim() ?? string.Empty;
            if (!string.Equals(availability, "available", StringComparison.OrdinalIgnoreCase))
            {
                validationErrors.Add($"Availability is '{availablePromotion.Availability}' but expected 'available'");
            }

            if (availablePromotion.Promotion == null)
            {
                validationErrors.Add("Promotion details are missing (Promotion is null)");
            }
            else
            {
                diagnosticInfo.Add($"PromotionName: {availablePromotion.Promotion.PromotionName}");
                diagnosticInfo.Add($"DateStart: {availablePromotion.Promotion.DateStart}");
                diagnosticInfo.Add($"DateEnd: {availablePromotion.Promotion.DateEnd}");
                
                var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
                diagnosticInfo.Add($"Today: {today}");

                if (availablePromotion.Promotion.DateStart > today || availablePromotion.Promotion.DateEnd < today)
                {
                    validationErrors.Add($"Promotion is not active. DateStart: {availablePromotion.Promotion.DateStart}, DateEnd: {availablePromotion.Promotion.DateEnd}, Today: {today}");
                }

                var minDeposit = availablePromotion.Promotion.MinDeposit;
                diagnosticInfo.Add($"MinDeposit: {minDeposit?.ToString() ?? "null"}");
                if (minDeposit.HasValue && amount < minDeposit.Value)
                {
                    validationErrors.Add($"Amount {amount:F2} is less than minimum deposit {minDeposit.Value:F2}");
                }

                var maxDeposit = availablePromotion.Promotion.MaxDeposit;
                diagnosticInfo.Add($"MaxDeposit: {maxDeposit?.ToString() ?? "null"}");
                if (maxDeposit.HasValue && amount > maxDeposit.Value)
                {
                    validationErrors.Add($"Amount {amount:F2} is greater than maximum deposit {maxDeposit.Value:F2}");
                }
            }

            if (validationErrors.Any())
            {
                var errorMessage = "Promotion validation failed:\n" + string.Join("\n", validationErrors);
                errorMessage += "\n\nDiagnostic information:\n" + string.Join("\n", diagnosticInfo);
                throw new InvalidOperationException(errorMessage);
            }
        }

        // Calculate total odds upfront to compute PotentialWin
        var totalOdds = odds.Aggregate(1m, (acc, o) => acc * o.OddsValue);

        var betSlip = new BetSlip
        {
            PlayerId = playerId,
            Amount = amount,
            Date = DateTime.UtcNow,
            Wynik = null,
            PotentialWin = amount * totalOdds,
            AvailablePromotionId = availablePromotion?.Id
        };

        await _dbContext.BetSlips.AddAsync(betSlip);
        await _dbContext.SaveChangesAsync();

        var betSlipOdds = odds.Select(o => new BetSlipOdd
        {
            BetSlipId = betSlip.Id,
            OddsId = o.Id,
            ConstOdd = o.OddsValue,
            Wynik = null
        }).ToList();

        await _dbContext.BetSlipOdds.AddRangeAsync(betSlipOdds);

        if (availablePromotion != null)
        {
            availablePromotion.Availability = "used";
            _dbContext.AvailablePromotions.Update(availablePromotion);
        }

        player.AccountBalance -= amount;
        _dbContext.Players.Update(player);

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return betSlip.Id;
    }

    public async Task<IEnumerable<object>> GetPlayerBetSlipsAsync(int playerId)
    {
        return await _dbContext.BetSlips
            .Where(bs => bs.PlayerId == playerId)
            .Include(bs => bs.BetSlipOdds)
            .OrderByDescending(bs => bs.Date)
            .ToListAsync();
    }

    public async Task<object?> GetBetSlipDetailsAsync(int betSlipId, int playerId)
    {
        return await _dbContext.BetSlips
            .Include(bs => bs.BetSlipOdds)
                .ThenInclude(bso => bso.Odds)
                    .ThenInclude(o => o.Event)
                        .ThenInclude(e => e.Sport)
            .Include(bs => bs.BetSlipOdds)
                .ThenInclude(bso => bso.Odds)
                    .ThenInclude(o => o.Team)
            .FirstOrDefaultAsync(bs => bs.Id == betSlipId && bs.PlayerId == playerId);
    }


    public async Task CheckAndUpdateAllBetSlipsResultsAsync()
    {
        // Znajdź wszystkie betslipy, które jeszcze nie mają wyniku
        var pendingBetSlips = await _dbContext.BetSlips
            .Include(bs => bs.BetSlipOdds)
                .ThenInclude(bso => bso.Odds)
                    .ThenInclude(o => o.Event)
            .Include(bs => bs.Player)
            .Where(bs => bs.Wynik == null)
            .ToListAsync();

        foreach (var betSlip in pendingBetSlips)
        {
            bool lost = false;
            foreach (var betSlipOdd in betSlip.BetSlipOdds)
            {
                // Jeśli event jest zakończony i wynik jeszcze nie został sprawdzony
                if (betSlipOdd.Odds.Event.IsCompleted==true && !betSlipOdd.Wynik.HasValue)
                {
                    var teamResult = betSlipOdd.Odds.Wynik ?? 0;
                    var otherOddsInEvent = await _dbContext.Odds
                        .Where(o => o.EventId == betSlipOdd.Odds.EventId && o.Id != betSlipOdd.Odds.Id)
                        .ToListAsync();

                    bool teamWon = true;
                    foreach (var otherOdd in otherOddsInEvent)
                    {
                        if ((otherOdd.Wynik ?? 0) >= teamResult)
                        {
                            teamWon = false;
                            break;
                        }
                    }

                    betSlipOdd.Wynik = teamWon ? 1 : 0;

                    // jeśli ten odd przegrał → cały kupon przegrany
                    if (!teamWon)
                    {
                        betSlip.Wynik = 0;
                        lost = true;
                        break; // nie ma sensu dalej sprawdzać reszty
                    }
                }
            }
            // jeśli żaden nie przegrał, ale wszystkie mecze się skończyły
            if (!lost)
            {
                bool allEventsCompleted = betSlip.BetSlipOdds.All(bso => bso.Odds.Event.IsCompleted==true);
                if (allEventsCompleted)
                {
                    bool allOddsWon = betSlip.BetSlipOdds.All(bso => bso.Wynik == 1);
                    if (allOddsWon)
                    {
                        betSlip.Wynik = 1; // kupon wygrany
                        
                        // Przypisz wygraną do konta użytkownika
                        if (betSlip.PotentialWin.HasValue)
                        {
                            betSlip.Player.AccountBalance += betSlip.PotentialWin.Value;
                        }
                    }
                    else
                    {
                        betSlip.Wynik = 0; // chociaż jeden przegrał (np. sprawdzony wcześniej)
                    }
                }
            }

           
        }

        await _dbContext.SaveChangesAsync();
    }
}


