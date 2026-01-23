using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController:ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IPlayerService _playerService;
    

    public AuthController(IConfiguration configuration, IPlayerService playerService)
    {
        _configuration = configuration;
        _playerService = playerService;
    }
    
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterPlayer(RegisterPlayerRequestDto request)
    {
        try
        {
            await _playerService.RegisterPlayerAsync(request);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
    
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        try
        {
            string secretKey = _configuration["SecretKey"] ?? "ThisIsAVeryLongSecretKeyForJWTTokenGenerationThatIsAtLeast32CharactersLong123456789";
            var response = await _playerService.LoginAsync(request, secretKey);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
    
    
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshTokenRequestDto request)
    {
        try
        {
            string secretKey = _configuration["SecretKey"] ?? "ThisIsAVeryLongSecretKeyForJWTTokenGenerationThatIsAtLeast32CharactersLong123456789";
            var response = await _playerService.RefreshTokenAsync(request, secretKey);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error refreshing token: {ex.Message}");
        }
    }
    
}