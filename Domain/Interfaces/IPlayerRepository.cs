using Domain.Entities;

namespace Domain.Interfaces;

public interface IPlayerRepository
{
    Task AddAsync(RegisterPlayer player);
    Task<bool> ExistsByEmailAsync(string email);
    Task<Player?> GetByEmailAsync(string email);
    Task<Player?> GetByRefreshTokenAsync(string refreshToken);
    Task UpdateRefreshTokenAsync(int playerId, string refreshToken, DateTime refreshTokenExp);
    Task<Player?> GetByIdAsync(int playerId);
    
    Task DeletePlayerAsync(int playerId);
    Task<bool> UpdatePlayerAsync(int playerId, string? name, string? lastName, string? email, decimal? accountBalance, int? role);
    Task<List<Player>> SearchPlayersByNameAsync(string searchTerm);
}

