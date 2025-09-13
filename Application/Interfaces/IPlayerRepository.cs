using Domain.Entities;

namespace Application.Interfaces;

public interface IPlayerRepository
{
    Task AddAsync(RegisterPlayer player);
    Task<bool> ExistsByEmailAsync(string email);
    Task<Player?> GetByEmailAsync(string email);
    Task<Player?> GetByRefreshTokenAsync(string refreshToken);
    Task UpdateRefreshTokenAsync(int playerId, string refreshToken, DateTime refreshTokenExp);
}