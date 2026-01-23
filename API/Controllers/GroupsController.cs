using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly IPlayerService _playerService;

    public GroupsController(IGroupService groupService, IPlayerService playerService)
    {
        _groupService = groupService;
        _playerService = playerService;
    }

    /// <summary>
    /// Tworzy nową grupę
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        try
        {
            var groupId = await _groupService.CreateGroupAsync(userId, request.GroupName);
            return Ok(new { id = groupId, message = "Grupa została utworzona pomyślnie." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas tworzenia grupy: {ex.Message}");
        }
    }

    /// <summary>
    /// Pobiera wszystkie grupy gracza
    /// </summary>
    [HttpGet("my-groups")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> GetMyGroups()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        try
        {
            var result = await _groupService.GetPlayerGroupsAsync(userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas pobierania grup: {ex.Message}");
        }
    }

    /// <summary>
    /// Pobiera szczegóły grupy po ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> GetGroupDetails(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        try
        {
            var result = await _groupService.GetGroupByIdAsync(id);
            if (result == null)
            {
                return NotFound("Grupa nie została znaleziona.");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas pobierania szczegółów grupy: {ex.Message}");
        }
    }

    /// <summary>
    /// Dodaje gracza do grupy (tylko członkowie grupy mogą zapraszać)
    /// </summary>
    [HttpPost("{groupId}/members")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> AddMember([FromRoute] int groupId, [FromBody] AddMemberToGroupRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        if (!int.TryParse(userIdClaim, out var inviterId)) return Unauthorized();

        try
        {
            var result = await _groupService.AddMemberAsync(inviterId, request.PlayerId, groupId);
            
            if (!result)
            {
                return BadRequest("Nie można dodać gracza do grupy (prawdopodobnie już jest w grupie).");
            }

            return Ok("Gracz został dodany do grupy.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas dodawania gracza do grupy: {ex.Message}");
        }
    }

    /// <summary>
    /// Usuwa gracza z grupy
    /// </summary>
    [HttpDelete("{groupId}/members")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> RemoveMember([FromRoute] int groupId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        try
        {
            var result = await _groupService.RemoveMemberAsync(userId, groupId);
            
            if (!result)
            {
                return NotFound("Gracz nie jest członkiem tej grupy.");
            }

            return Ok("Gracz został usunięty z grupy.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas usuwania gracza z grupy: {ex.Message}");
        }
    }

    /// <summary>
    /// Wysyła wiadomość do grupy (tylko członkowie grupy mogą pisać)
    /// </summary>
    [HttpPost("{groupId}/messages")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> SendMessage([FromRoute] int groupId, [FromBody] SendGroupMessageRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        try
        {
            var messageId = await _groupService.SendMessageAsync(userId, groupId, request.MessageText);

            return Ok(new { id = messageId, message = "Wiadomość została wysłana." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas wysyłania wiadomości: {ex.Message}");
        }
    }

    /// <summary>
    /// Pobiera wszystkie wiadomości z grupy (tylko członkowie grupy mogą czytać)
    /// </summary>
    [HttpGet("{groupId}/messages")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> GetGroupMessages([FromRoute] int groupId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        try
        {
            var result = await _groupService.GetGroupMessagesAsync(groupId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas pobierania wiadomości: {ex.Message}");
        }
    }

    /// <summary>
    /// Wyszukuje graczy po imieniu lub nazwisku (używane do dodawania do grupy)
    /// </summary>
    [HttpGet("search-players")]
    [Authorize(Policy = "PlayerOnly")]
    public async Task<IActionResult> SearchPlayers([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query parameter jest wymagany.");
        }

        if (query.Length < 2)
        {
            return BadRequest("Query musi mieć co najmniej 2 znaki.");
        }

        try
        {
            var players = await _playerService.SearchPlayersByNameAsync(query);

            var result = players.Select(p => new PlayerSearchResultDto
            {
                PlayerId = p.Id,
                Name = p.Name,
                LastName = p.LastName
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas wyszukiwania graczy: {ex.Message}");
        }
    }
}
