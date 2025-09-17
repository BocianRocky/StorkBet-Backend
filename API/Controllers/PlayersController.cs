using System.Security.Claims;
using Application.Interfaces;
using API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IBetSlipRepository _betSlipRepository;

    public PlayersController(IPlayerRepository playerRepository, IBetSlipRepository betSlipRepository)
    {
        _playerRepository = playerRepository;
        _betSlipRepository = betSlipRepository;
    } 

    [HttpGet("me")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var player = await _playerRepository.GetByIdAsync(userId);
        if (player == null) return NotFound();

        return Ok(new
        {
            name = player.Name,
            lastName = player.LastName,
            accountBalance = player.AccountBalance
        });
    }

    [HttpPost("betslips")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> CreateBetSlip([FromBody] CreateBetSlipRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        try
        {
            var id = await _betSlipRepository.CreateBetSlipAsync(userId, request.Amount, request.OddsIds);
            return Ok(new { id });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}


