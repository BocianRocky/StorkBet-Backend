using Application.DTOs;

namespace Application.Interfaces;

public interface IGroupService
{
    Task<int> CreateGroupAsync(int creatorId, string groupName);
    Task<List<GroupDto>> GetPlayerGroupsAsync(int playerId);
    Task<GroupDto?> GetGroupByIdAsync(int groupId);
    Task<bool> AddMemberAsync(int inviterId, int playerId, int groupId);
    Task<bool> RemoveMemberAsync(int playerId, int groupId);
    Task<bool> IsPlayerInGroupAsync(int playerId, int groupId);
    Task<int> SendMessageAsync(int playerId, int groupId, string messageText);
    Task<List<GroupMessageDto>> GetGroupMessagesAsync(int groupId);
}

