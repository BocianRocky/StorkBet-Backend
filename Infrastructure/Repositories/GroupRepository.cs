using Domain.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using DataGroup = Infrastructure.Data.Group;
using DataPlayerGroup = Infrastructure.Data.PlayerGroup;
using DataGroupchatMessage = Infrastructure.Data.GroupchatMessage;
using DomainGroup = Domain.Entities.Group;
using DomainPlayerGroup = Domain.Entities.PlayerGroup;
using DomainGroupchatMessage = Domain.Entities.GroupchatMessage;
using DomainPlayer = Domain.Entities.Player;

namespace Infrastructure.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly AppDbContext _context;

    public GroupRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateGroupAsync(int creatorId, string groupName)
    {
        // Create the group
        var group = new DataGroup
        {
            GroupName = groupName,
            GroupchatMessages = new List<DataGroupchatMessage>(),
            PlayerGroups = new List<DataPlayerGroup>()
        };

        await _context.Groups.AddAsync(group);
        await _context.SaveChangesAsync();

        // Add creator as group owner
        var playerGroup = new DataPlayerGroup
        {
            PlayerId = creatorId,
            GroupId = group.Id,
            IsGroupOwner = 1 // Owner flag
        };

        await _context.PlayerGroups.AddAsync(playerGroup);
        await _context.SaveChangesAsync();

        return group.Id;
    }

    public async Task<List<DomainGroup>> GetPlayerGroupsAsync(int playerId)
    {
        var groups = await _context.PlayerGroups
            .Where(pg => pg.PlayerId == playerId)
            .Include(pg => pg.Group)
                .ThenInclude(g => g.PlayerGroups)
                    .ThenInclude(pg => pg.Player)
            .Select(pg => pg.Group)
            .ToListAsync();
        
        return groups.Select(g => new DomainGroup
        {
            Id = g.Id,
            GroupName = g.GroupName,
            PlayerGroups = g.PlayerGroups.Select(pg => new DomainPlayerGroup
            {
                Id = pg.Id,
                PlayerId = pg.PlayerId,
                GroupId = pg.GroupId,
                IsGroupOwner = pg.IsGroupOwner,
                JoinedAt = pg.JoinedAt,
                Player = new DomainPlayer
                {
                    Id = pg.Player.Id,
                    Name = pg.Player.Name,
                    LastName = pg.Player.LastName,
                    Email = pg.Player.Email
                }
            }).ToList(),
            GroupchatMessages = g.GroupchatMessages.Select(m => new DomainGroupchatMessage
            {
                Id = m.Id,
                GroupId = m.GroupId,
                PlayerId = m.PlayerId,
                MessageText = m.MessageText,
                Time = m.Time
            }).ToList()
        }).ToList();
    }

    public async Task<bool> AddPlayerToGroupAsync(int playerId, int groupId)
    {
        // Check if player is already in group
        var exists = await _context.PlayerGroups
            .AnyAsync(pg => pg.PlayerId == playerId && pg.GroupId == groupId);

        if (exists)
        {
            return false; // Player already in group
        }

        var playerGroup = new DataPlayerGroup
        {
            PlayerId = playerId,
            GroupId = groupId,
            IsGroupOwner = 0 // Not owner
        };

        await _context.PlayerGroups.AddAsync(playerGroup);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemovePlayerFromGroupAsync(int playerId, int groupId)
    {
        var playerGroup = await _context.PlayerGroups
            .FirstOrDefaultAsync(pg => pg.PlayerId == playerId && pg.GroupId == groupId);

        if (playerGroup == null)
        {
            return false;
        }

        _context.PlayerGroups.Remove(playerGroup);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<DomainGroup?> GetGroupByIdAsync(int groupId)
    {
        var group = await _context.Groups
            .Include(g => g.PlayerGroups)
                .ThenInclude(pg => pg.Player)
            .Include(g => g.GroupchatMessages)
            .FirstOrDefaultAsync(g => g.Id == groupId);
        
        if (group == null) return null;
        
        return new DomainGroup
        {
            Id = group.Id,
            GroupName = group.GroupName,
            PlayerGroups = group.PlayerGroups.Select(pg => new DomainPlayerGroup
            {
                Id = pg.Id,
                PlayerId = pg.PlayerId,
                GroupId = pg.GroupId,
                IsGroupOwner = pg.IsGroupOwner,
                JoinedAt = pg.JoinedAt,
                Player = new DomainPlayer
                {
                    Id = pg.Player.Id,
                    Name = pg.Player.Name,
                    LastName = pg.Player.LastName,
                    Email = pg.Player.Email
                }
            }).ToList(),
            GroupchatMessages = group.GroupchatMessages.Select(m => new DomainGroupchatMessage
            {
                Id = m.Id,
                GroupId = m.GroupId,
                PlayerId = m.PlayerId,
                MessageText = m.MessageText,
                Time = m.Time
            }).ToList()
        };
    }

    public async Task<List<DomainPlayerGroup>> GetGroupMembersAsync(int groupId)
    {
        var members = await _context.PlayerGroups
            .Where(pg => pg.GroupId == groupId)
            .Include(pg => pg.Player)
            .ToListAsync();
        
        return members.Select(pg => new DomainPlayerGroup
        {
            Id = pg.Id,
            PlayerId = pg.PlayerId,
            GroupId = pg.GroupId,
            IsGroupOwner = pg.IsGroupOwner,
            JoinedAt = pg.JoinedAt,
            Player = new DomainPlayer
            {
                Id = pg.Player.Id,
                Name = pg.Player.Name,
                LastName = pg.Player.LastName,
                Email = pg.Player.Email
            }
        }).ToList();
    }

    public async Task<bool> IsPlayerInGroupAsync(int playerId, int groupId)
    {
        return await _context.PlayerGroups
            .AnyAsync(pg => pg.PlayerId == playerId && pg.GroupId == groupId);
    }

    public async Task<int> SendMessageAsync(int playerId, int groupId, string messageText)
    {
        var message = new DataGroupchatMessage
        {
            PlayerId = playerId,
            GroupId = groupId,
            MessageText = messageText
        };

        await _context.GroupchatMessages.AddAsync(message);
        await _context.SaveChangesAsync();

        return message.Id;
    }

    public async Task<List<DomainGroupchatMessage>> GetGroupMessagesAsync(int groupId)
    {
        var messages = await _context.GroupchatMessages
            .Where(m => m.GroupId == groupId)
            .Include(m => m.Player)
            .OrderBy(m => m.Id) // Order by ID as Time is RowVersion and can't be used for ordering
            .ToListAsync();
        
        return messages.Select(m => new DomainGroupchatMessage
        {
            Id = m.Id,
            GroupId = m.GroupId,
            PlayerId = m.PlayerId,
            MessageText = m.MessageText,
            Time = m.Time,
            Player = new DomainPlayer
            {
                Id = m.Player.Id,
                Name = m.Player.Name,
                LastName = m.Player.LastName,
                Email = m.Player.Email
            }
        }).ToList();
    }
}
