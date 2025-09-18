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
            .Include(bs => bs.BetSlipOdds)
                .ThenInclude(bso => bso.Odds)
                    .ThenInclude(o => o.Team)
            .FirstOrDefaultAsync(bs => bs.Id == betSlipId && bs.PlayerId == playerId);
    }
}


