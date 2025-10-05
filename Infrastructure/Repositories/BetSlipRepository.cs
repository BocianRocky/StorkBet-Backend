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

    public async Task<int> CreateBetSlipAsync(int playerId, decimal amount, IEnumerable<int> oddsIds)
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

        // Calculate total odds upfront to compute PotentialWin
        var totalOdds = odds.Aggregate(1m, (acc, o) => acc * o.OddsValue);

        var betSlip = new BetSlip
        {
            PlayerId = playerId,
            Amount = amount,
            Date = DateTime.UtcNow,
            Wynik = null,
            PotentialWin = amount * totalOdds
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
            .Where(bs => bs.Wynik == null)
            .ToListAsync();

        foreach (var betSlip in pendingBetSlips)
        {
            // Sprawdź czy wszystkie eventy w betslipie są zakończone
            var allEventsCompleted = betSlip.BetSlipOdds.All(bso => 
                bso.Odds.Event.IsCompleted == true);

            if (allEventsCompleted)
            {
                // Sprawdź wyniki dla każdego odd w betslipie
                foreach (var betSlipOdd in betSlip.BetSlipOdds)
                {
                    if (!betSlipOdd.Wynik.HasValue)
                    {
                        // Sprawdź czy drużyna wygrała (porównaj wyniki)
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
                    }
                }

                // Sprawdź czy wszystkie oddsy mają wyniki
                var allOddsHaveResults = betSlip.BetSlipOdds.All(bso => bso.Wynik.HasValue);
                
                if (allOddsHaveResults)
                {
                    // Sprawdź czy wszystkie oddsy są wygrane (Wynik = 1)
                    var allOddsWon = betSlip.BetSlipOdds.All(bso => bso.Wynik == 1);
                    
                    if (allOddsWon)
                    {
                        betSlip.Wynik = 1; // Cały betslip wygrany
                    }
                    else
                    {
                        betSlip.Wynik = 0; // Cały betslip przegrany (przynajmniej jeden odd przegrany)
                    }
                }
            }
        }

        await _dbContext.SaveChangesAsync();
    }
}


