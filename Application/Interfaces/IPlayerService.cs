using Domain.Entities;
using Application.DTOs;

namespace Application.Interfaces;

public interface IPlayerService
{
    Task RegisterPlayerAsync(RegisterPlayerRequestDto request);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string secretKey);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string secretKey);
    Task<Player?> GetPlayerByIdAsync(int playerId);
    Task<List<Player>> SearchPlayersByNameAsync(string searchTerm);
    Task DeletePlayerAsync(int playerId);
    Task<bool> UpdatePlayerAsync(int playerId, string? name, string? lastName, string? email, decimal? accountBalance, int? role);
}

