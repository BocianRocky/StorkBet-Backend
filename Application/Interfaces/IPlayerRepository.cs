using Domain.Entities;

namespace Application.Interfaces;

public interface IPlayerRepository
{
    Task AddAsync(RegisterPlayer player);
    Task<bool> ExistsByEmailAsync(string email);
    Task<Player?> GetByEmailAsync(string email);
}