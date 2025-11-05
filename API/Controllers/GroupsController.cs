using System.Security.Claims;
using Infrastructure.Interfaces;
using API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly IGroupRepository _groupRepository;

    public GroupsController(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
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
            var groupId = await _groupRepository.CreateGroupAsync(userId, request.GroupName);
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
            var groups = await _groupRepository.GetPlayerGroupsAsync(userId);
            
            var result = groups.Select(g => new GroupDto
            {
                Id = g.Id,
                GroupName = g.GroupName,
                Members = g.PlayerGroups.Select(pg => new GroupMemberDto
                {
                    PlayerId = pg.Player.Id,
                    Name = pg.Player.Name,
                    LastName = pg.Player.LastName,
                    IsOwner = pg.IsGroupOwner == 1
                }).ToList(),
                MessageCount = g.GroupchatMessages?.Count ?? 0
            }).ToList();

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
            var group = await _groupRepository.GetGroupByIdAsync(id);
            if (group == null)
            {
                return NotFound("Grupa nie została znaleziona.");
            }

            var result = new GroupDto
            {
                Id = group.Id,
                GroupName = group.GroupName,
                Members = group.PlayerGroups.Select(pg => new GroupMemberDto
                {
                    PlayerId = pg.Player.Id,
                    Name = pg.Player.Name,
                    LastName = pg.Player.LastName,
                    IsOwner = pg.IsGroupOwner == 1
                }).ToList(),
                MessageCount = group.GroupchatMessages?.Count ?? 0
            };

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
            // Sprawdź czy wywołujący (inviter) jest członkiem grupy
            var isInviterMember = await _groupRepository.IsPlayerInGroupAsync(inviterId, groupId);
            if (!isInviterMember)
            {
                return Forbid("Tylko członkowie grupy mogą zapraszać innych graczy.");
            }

            // Sprawdź czy grupa istnieje
            var group = await _groupRepository.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return NotFound("Grupa nie została znaleziona.");
            }

            var result = await _groupRepository.AddPlayerToGroupAsync(request.PlayerId, groupId);
            
            if (!result)
            {
                return BadRequest("Nie można dodać gracza do grupy (prawdopodobnie już jest w grupie).");
            }

            return Ok("Gracz został dodany do grupy.");
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
            var result = await _groupRepository.RemovePlayerFromGroupAsync(userId, groupId);
            
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
            // Sprawdź czy użytkownik jest członkiem grupy
            var isMember = await _groupRepository.IsPlayerInGroupAsync(userId, groupId);
            if (!isMember)
            {
                return Forbid("Tylko członkowie grupy mogą pisać wiadomości.");
            }

            var messageId = await _groupRepository.SendMessageAsync(userId, groupId, request.MessageText);

            return Ok(new { id = messageId, message = "Wiadomość została wysłana." });
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
            // Sprawdź czy użytkownik jest członkiem grupy
            var isMember = await _groupRepository.IsPlayerInGroupAsync(userId, groupId);
            if (!isMember)
            {
                return Forbid("Tylko członkowie grupy mogą czytać wiadomości.");
            }

            var messages = await _groupRepository.GetGroupMessagesAsync(groupId);

            var result = messages.Select(m => new GroupMessageDto
            {
                Id = m.Id,
                GroupId = m.GroupId,
                PlayerId = m.PlayerId,
                PlayerName = m.Player.Name,
                PlayerLastName = m.Player.LastName,
                MessageText = m.MessageText
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd podczas pobierania wiadomości: {ex.Message}");
        }
    }
}
