using System;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _dbContext;

    public TransactionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CreateDepositAsync(int playerId, decimal amount, int paymentMethod)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var player = await _dbContext.Players.FirstOrDefaultAsync(p => p.Id == playerId);
            if (player == null)
            {
                throw new KeyNotFoundException("Player not found");
            }
            
            if (amount <= 0)
            {
                throw new InvalidOperationException("Amount must be positive");
            }
            
            var newTransaction = new Transaction
            {
                Amount = amount,
                Type = 1, // 1 = Deposit (wpłata)
                PaymentMethod = paymentMethod,
                PlayerId = playerId
            };

            _dbContext.Transactions.Add(newTransaction);
            
            player.AccountBalance += amount;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return newTransaction.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> CreateWithdrawalAsync(int playerId, decimal amount, int paymentMethod)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
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
            
            var newTransaction = new Transaction
            {
                Amount = amount,
                Type = 2, // 2 = Withdrawal (wypłata)
                PaymentMethod = paymentMethod,
                PlayerId = playerId
            };

            _dbContext.Transactions.Add(newTransaction);
            
            player.AccountBalance -= amount;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return newTransaction.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

