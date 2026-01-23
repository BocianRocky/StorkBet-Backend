using Application.Interfaces;
using Domain.Interfaces;
using Domain.Entities;
using Application.DTOs;

namespace Application.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;

    public GroupService(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task<int> CreateGroupAsync(int creatorId, string groupName)
    {
        return await _groupRepository.CreateGroupAsync(creatorId, groupName);
    }

    public async Task<List<GroupDto>> GetPlayerGroupsAsync(int playerId)
    {
        var groups = await _groupRepository.GetPlayerGroupsAsync(playerId);
        
        return groups.Select(g => new GroupDto
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
    }

    public async Task<GroupDto?> GetGroupByIdAsync(int groupId)
    {
        var group = await _groupRepository.GetGroupByIdAsync(groupId);
        if (group == null) return null;

        return new GroupDto
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
    }

    public async Task<bool> AddMemberAsync(int inviterId, int playerId, int groupId)
    {
        // Sprawdź czy wywołujący (inviter) jest członkiem grupy
        var isInviterMember = await _groupRepository.IsPlayerInGroupAsync(inviterId, groupId);
        if (!isInviterMember)
        {
            throw new UnauthorizedAccessException("Tylko członkowie grupy mogą zapraszać innych graczy.");
        }

        // Sprawdź czy grupa istnieje
        var group = await _groupRepository.GetGroupByIdAsync(groupId);
        if (group == null)
        {
            throw new KeyNotFoundException("Grupa nie została znaleziona.");
        }

        return await _groupRepository.AddPlayerToGroupAsync(playerId, groupId);
    }

    public async Task<bool> RemoveMemberAsync(int playerId, int groupId)
    {
        return await _groupRepository.RemovePlayerFromGroupAsync(playerId, groupId);
    }

    public async Task<bool> IsPlayerInGroupAsync(int playerId, int groupId)
    {
        return await _groupRepository.IsPlayerInGroupAsync(playerId, groupId);
    }

    public async Task<int> SendMessageAsync(int playerId, int groupId, string messageText)
    {
        // Sprawdź czy użytkownik jest członkiem grupy
        var isMember = await _groupRepository.IsPlayerInGroupAsync(playerId, groupId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("Tylko członkowie grupy mogą pisać wiadomości.");
        }

        return await _groupRepository.SendMessageAsync(playerId, groupId, messageText);
    }

    public async Task<List<GroupMessageDto>> GetGroupMessagesAsync(int groupId)
    {
        var messages = await _groupRepository.GetGroupMessagesAsync(groupId);

        return messages.Select(m => new GroupMessageDto
        {
            Id = m.Id,
            GroupId = m.GroupId,
            PlayerId = m.PlayerId,
            PlayerName = m.Player.Name,
            PlayerLastName = m.Player.LastName,
            MessageText = m.MessageText
        }).ToList();
    }
}

