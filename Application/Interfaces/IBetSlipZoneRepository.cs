using Application.DTOs;

namespace Application.Interfaces;

public interface IBetSlipZoneRepository
{
    Task<IEnumerable<BetSlipZoneItemDto>> GetActivePostsAsync(int page, int pageSize, string sortBy, int? currentUserId = null);
    Task<BetSlipZoneDetailsDto?> GetPostDetailsAsync(int postId, int? currentUserId = null);
    Task<int> CreatePostAsync(int playerId, int betSlipId);
    Task<bool> PostExistsForBetSlipAsync(int betSlipId);
    Task<bool> IsBetSlipActiveAsync(int betSlipId);
    Task<bool> IsBetSlipOwnedByPlayerAsync(int betSlipId, int playerId);
    Task SetReactionAsync(int postId, int playerId, int reactionType);
    Task<int?> GetUserReactionAsync(int postId, int playerId);
    Task<BetSlipReactionSummaryDto> GetReactionSummaryAsync(int postId, int? currentUserId = null);
    Task<IEnumerable<BetSlipZoneItemDto>> GetMyPostsAsync(int playerId, int page, int pageSize);
}

