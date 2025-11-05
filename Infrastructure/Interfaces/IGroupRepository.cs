using Infrastructure.Data;

namespace Infrastructure.Interfaces;

public interface IGroupRepository
{
    Task<int> CreateGroupAsync(int creatorId, string groupName);
    Task<List<Group>> GetPlayerGroupsAsync(int playerId);
    Task<bool> AddPlayerToGroupAsync(int playerId, int groupId);
    Task<bool> RemovePlayerFromGroupAsync(int playerId, int groupId);
    Task<Group?> GetGroupByIdAsync(int groupId);
    Task<List<PlayerGroup>> GetGroupMembersAsync(int groupId);
    Task<bool> IsPlayerInGroupAsync(int playerId, int groupId);
    Task<int> SendMessageAsync(int playerId, int groupId, string messageText);
    Task<List<GroupchatMessage>> GetGroupMessagesAsync(int groupId);
}
