namespace Domain.Interfaces;

public interface ITransactionRepository
{
    Task<int> CreateDepositAsync(int playerId, decimal amount, int paymentMethod);
    Task<int> CreateWithdrawalAsync(int playerId, decimal amount, int paymentMethod);
}

