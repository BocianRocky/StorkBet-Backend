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
    [HttpGet("profile")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> GetMyProfileDetails()
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
            email = player.Email,
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
            var id = await _betSlipRepository.CreateBetSlipAsync(userId, request.Amount, request.OddsIds, request.AvailablePromotionId);
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

    [HttpGet("betslips")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> GetMyBetSlips()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var betSlips = await _betSlipRepository.GetPlayerBetSlipsAsync(userId);

        var result = betSlips.Cast<Infrastructure.Data.BetSlip>().Select(bs =>
        {
            var totalOdds = bs.BetSlipOdds.Any() ? bs.BetSlipOdds.Aggregate(1m, (acc, odd) => acc * odd.ConstOdd) : 1m;
            return new BetSlipListItemDto
            {
                Id = bs.Id,
                Amount = bs.Amount,
                Date = bs.Date,
                Wynik = bs.Wynik,
                TotalOdds = totalOdds,
                PotentialWin = bs.PotentialWin ?? (bs.Amount * totalOdds),
                OddsCount = bs.BetSlipOdds.Count
            };
        }).ToList();

        return Ok(result);
    }

    [HttpGet("betslips/{id}")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> GetBetSlipDetails(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var betSlip = await _betSlipRepository.GetBetSlipDetailsAsync(id, userId);
        if (betSlip == null) return NotFound();
        
        var betSlipEntity = (Infrastructure.Data.BetSlip)betSlip;

        var totalOdds = betSlipEntity.BetSlipOdds.Any() ? betSlipEntity.BetSlipOdds.Aggregate(1m, (acc, odd) => acc * odd.ConstOdd) : 1m;
        
        var result = new BetSlipDetailsDto
        {
            Id = betSlipEntity.Id,
            Amount = betSlipEntity.Amount,
            Date = betSlipEntity.Date,
            Wynik = betSlipEntity.Wynik,
            TotalOdds = totalOdds,
            PotentialWin = betSlipEntity.PotentialWin ?? (betSlipEntity.Amount * totalOdds),
            BetSlipOdds = betSlipEntity.BetSlipOdds.Select(bso => new BetSlipOddDetailsDto
            {
                Id = bso.Id,
                ConstOdd = bso.ConstOdd,
                Wynik = bso.Wynik,
                Event = new EventDetailsDto
                {
                    Id = bso.Odds.Event.Id,
                    Name = bso.Odds.Event.EventName,
                    Date = bso.Odds.Event.EventDate,
                    Group = bso.Odds.Event.Sport.Group,
                    Title = bso.Odds.Event.Sport.Title
                },
                Team = new TeamDetailsDto
                {
                    Id = bso.Odds.Team.Id,
                    Name = bso.Odds.Team.TeamName
                }
            }).ToList()
        };

        return Ok(result);
    }
    
    [HttpDelete("delete-self")]
    [Authorize (Policy = "PlayerOnly")] 
    public async Task<IActionResult> DeleteMyAccount()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();
        await _playerRepository.DeletePlayerAsync(userId);
        
        return Ok("Konto użytkownika zostało usunięte.");
    }
        
    
    
}


