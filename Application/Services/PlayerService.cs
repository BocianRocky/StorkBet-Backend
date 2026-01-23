using Application.Interfaces;
using Domain.Interfaces;
using Domain.Entities;
using Application.DTOs;
using Application.Security;

namespace Application.Services;

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _playerRepository;

    public PlayerService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public async Task RegisterPlayerAsync(RegisterPlayerRequestDto request)
    {
        if (await _playerRepository.ExistsByEmailAsync(request.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        var hashedPasswordAndSalt = SecurityHelper.GetHashedPasswordAndSalt(request.Password);
        var player = new RegisterPlayer()
        {
            Name = request.Name,
            LastName = request.LastName,
            Email = request.Email,
            Password = hashedPasswordAndSalt.Item1,
            Salt = hashedPasswordAndSalt.Item2,
            RefreshToken = SecurityHelper.GenerateRefreshToken(),
            RefreshTokenExp = DateTime.UtcNow.AddDays(1)
        };
        await _playerRepository.AddAsync(player);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string secretKey)
    {
        var player = await _playerRepository.GetByEmailAsync(request.Username);
        
        if (player == null)
        {
            throw new UnauthorizedAccessException("Nieprawidłowy email lub hasło");
        }
        
        // Sprawdź hasło
        string hashedPasswordFromDb = player.Password;
        string currentHashedPassword = SecurityHelper.GetHashedPasswordWithSalt(request.Password, player.Salt);

        if (hashedPasswordFromDb != currentHashedPassword)
        {
            throw new UnauthorizedAccessException("Nieprawidłowy email lub hasło");
        }

        // Generuj nowy refresh token
        string newRefreshToken = SecurityHelper.GenerateRefreshToken();
        DateTime refreshTokenExp = DateTime.UtcNow.AddDays(1);
        
        // Aktualizuj refresh token w bazie
        await _playerRepository.UpdateRefreshTokenAsync(player.Id, newRefreshToken, refreshTokenExp);
        
        // Generuj JWT token
        string accessToken = SecurityHelper.GenerateJwtToken(player.Id, player.Email, player.Role.ToString(), secretKey);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            RefreshTokenExp = refreshTokenExp,
            UserId = player.Id,
            Name = player.Name,
            LastName = player.LastName,
            Email = player.Email,
            AccountBalance = player.AccountBalance
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string secretKey)
    {
        var player = await _playerRepository.GetByRefreshTokenAsync(request.RefreshToken);
        
        if (player == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Sprawdź czy refresh token nie wygasł
        if (player.RefreshTokenExp < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token expired");
        }

        // Generuj nowy refresh token
        string newRefreshToken = SecurityHelper.GenerateRefreshToken();
        DateTime refreshTokenExp = DateTime.UtcNow.AddDays(1);
        
        // Aktualizuj refresh token w bazie
        await _playerRepository.UpdateRefreshTokenAsync(player.Id, newRefreshToken, refreshTokenExp);

        // Generuj nowy JWT token
        string accessToken = SecurityHelper.GenerateJwtToken(player.Id, player.Email, player.Role.ToString(), secretKey);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            RefreshTokenExp = refreshTokenExp,
            UserId = player.Id,
            Name = player.Name,
            LastName = player.LastName,
            Email = player.Email,
            AccountBalance = player.AccountBalance
        };
    }

    public async Task<Player?> GetPlayerByIdAsync(int playerId)
    {
        return await _playerRepository.GetByIdAsync(playerId);
    }

    public async Task<List<Player>> SearchPlayersByNameAsync(string searchTerm)
    {
        return await _playerRepository.SearchPlayersByNameAsync(searchTerm);
    }

    public async Task DeletePlayerAsync(int playerId)
    {
        await _playerRepository.DeletePlayerAsync(playerId);
    }

    public async Task<bool> UpdatePlayerAsync(int playerId, string? name, string? lastName, string? email, decimal? accountBalance, int? role)
    {
        return await _playerRepository.UpdatePlayerAsync(playerId, name, lastName, email, accountBalance, role);
    }
}

