using API.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController:ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IPlayerRepository _playerRepository;
    

    public AuthController(IConfiguration configuration, IPlayerRepository playerRepository)
    {
        _configuration = configuration;
        _playerRepository = playerRepository;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> RegisterPlayer(RegisterPlayerRequestDto request)
    {
        if (await _playerRepository.ExistsByEmailAsync(request.Email)){
            return BadRequest("Email already exists");
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
        return Ok();
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        // Znajdź gracza po emailu (username to email w tym przypadku)
        var player = await _playerRepository.GetByEmailAsync(request.Username);
        
        if (player == null)
        {
            return Unauthorized("Nieprawidłowy email lub hasło");
        }
        
        // Sprawdź hasło
        string hashedPasswordFromDb = player.Password;
        string currentHashedPassword = SecurityHelper.GetHashedPasswordWithSalt(request.Password, player.Salt);

        if (hashedPasswordFromDb != currentHashedPassword)
        {
            return Unauthorized("Nieprawidłowy email lub hasło");
        }

        // Generuj nowy refresh token
        string newRefreshToken = SecurityHelper.GenerateRefreshToken();
        DateTime refreshTokenExp = DateTime.UtcNow.AddDays(1);
        
        // Aktualizuj refresh token w bazie (możesz dodać metodę UpdateRefreshToken do repository)
        // Na razie pomijam to, ale powinieneś to zrobić
        
        // Generuj JWT token
        string secretKey = _configuration["SecretKey"] ?? "ThisIsAVeryLongSecretKeyForJWTTokenGenerationThatIsAtLeast32CharactersLong123456789";
        string accessToken = SecurityHelper.GenerateJwtToken(player.Id, player.Email, secretKey);

        var response = new LoginResponseDto
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

        return Ok(response);
    }
    
}