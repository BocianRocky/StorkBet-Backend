using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/betslip-zone")]
public class BetSlipZoneController : ControllerBase
{
    private readonly IBetSlipZoneRepository _betSlipZoneRepository;

    public BetSlipZoneController(IBetSlipZoneRepository betSlipZoneRepository)
    {
        _betSlipZoneRepository = betSlipZoneRepository;
    }

    /// <summary>
    /// Pobiera listę aktywnych kuponów w Strefie Kuponów
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetActivePosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "reactions")
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        int? currentUserId = null;
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
        {
            currentUserId = userId;
        }

        var posts = await _betSlipZoneRepository.GetActivePostsAsync(page, pageSize, sortBy, currentUserId);
        return Ok(posts);
    }

    /// <summary>
    /// Pobiera szczegóły konkretnego posta w Strefie Kuponów
    /// </summary>
    [HttpGet("{postId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPostDetails(int postId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        int? currentUserId = null;
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
        {
            currentUserId = userId;
        }

        var post = await _betSlipZoneRepository.GetPostDetailsAsync(postId, currentUserId);
        if (post == null) return NotFound(new { message = "Post nie został znaleziony" });

        return Ok(post);
    }

    /// <summary>
    /// Tworzy nowy post w Strefie Kuponów (udostępnia kupon)
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> CreatePost([FromBody] CreateBetSlipPostRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        
        var isOwned = await _betSlipZoneRepository.IsBetSlipOwnedByPlayerAsync(request.BetSlipId, userId);
        if (!isOwned)
        {
            return BadRequest(new { message = "Możesz udostępnić tylko swoje kupony" });
        }

        
        var isActive = await _betSlipZoneRepository.IsBetSlipActiveAsync(request.BetSlipId);
        if (!isActive)
        {
            return BadRequest(new { message = "Możesz udostępnić tylko kupony w trakcie" });
        }

        
        var exists = await _betSlipZoneRepository.PostExistsForBetSlipAsync(request.BetSlipId);
        if (exists)
        {
            return BadRequest(new { message = "Ten kupon został już udostępniony" });
        }

        try
        {
            var postId = await _betSlipZoneRepository.CreatePostAsync(userId, request.BetSlipId);
            var post = await _betSlipZoneRepository.GetPostDetailsAsync(postId, userId);
            
            return CreatedAtAction(nameof(GetPostDetails), new { postId }, post);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Błąd podczas tworzenia posta: {ex.Message}" });
        }
    }

    /// <summary>
    /// Ustawia reakcję na kupon (lub usuwa jeśli jest taka sama)
    /// </summary>
    [HttpPost("{postId}/reactions")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> SetReaction(int postId, [FromBody] SetBetSlipReactionRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();
        
        if (request.ReactionType < 1 || request.ReactionType > 4)
        {
            return BadRequest(new { message = "ReactionType musi być między 1 a 4" });
        }
        
        var post = await _betSlipZoneRepository.GetPostDetailsAsync(postId, userId);
        if (post == null)
        {
            return NotFound(new { message = "Post nie został znaleziony" });
        }
        

        try
        {
            await _betSlipZoneRepository.SetReactionAsync(postId, userId, request.ReactionType);
            var summary = await _betSlipZoneRepository.GetReactionSummaryAsync(postId, userId);
            
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Błąd podczas ustawiania reakcji: {ex.Message}" });
        }
    }

    /// <summary>
    /// Pobiera moje posty w Strefie Kuponów
    /// </summary>
    [HttpGet("my-posts")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> GetMyPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var posts = await _betSlipZoneRepository.GetMyPostsAsync(userId, page, pageSize);
        return Ok(posts);
    }
}

