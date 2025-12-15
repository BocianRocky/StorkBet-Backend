using System.Linq;
using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BetSlipZoneRepository : IBetSlipZoneRepository
{
    private readonly AppDbContext _context;

    public BetSlipZoneRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BetSlipZoneItemDto>> GetActivePostsAsync(int page, int pageSize, string sortBy, int? currentUserId = null)
    {
        var query = _context.BetSlipPosts
            .Include(p => p.Player)
            .Include(p => p.BetSlip)
                .ThenInclude(bs => bs!.BetSlipOdds)
                    .ThenInclude(bso => bso.Odds)
            .Include(p => p.BetslipReactions)
            .Where(p => p.IsActive && (p.BetSlip.Wynik == null || p.BetSlip.Wynik == 0)) // Tylko kupony w trakcie
            .AsQueryable();

        
        query = sortBy?.ToLower() switch
        {
            "reactions" => query.OrderByDescending(p => 
                p.BetslipReactions.Count(r => r.ReactionType == 1) +
                p.BetslipReactions.Count(r => r.ReactionType == 2) +
                p.BetslipReactions.Count(r => r.ReactionType == 3) +
                p.BetslipReactions.Count(r => r.ReactionType == 4)),
            "time" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return posts.Select(p => MapToItemDto(p, currentUserId));
    }

    public async Task<BetSlipZoneDetailsDto?> GetPostDetailsAsync(int postId, int? currentUserId = null)
    {
        var post = await _context.BetSlipPosts
            .Include(p => p.Player)
            .Include(p => p.BetSlip)
                .ThenInclude(bs => bs!.BetSlipOdds)
                    .ThenInclude(bso => bso.Odds)
                        .ThenInclude(o => o.Event)
                            .ThenInclude(e => e!.Sport)
            .Include(p => p.BetSlip)
                .ThenInclude(bs => bs!.BetSlipOdds)
                    .ThenInclude(bso => bso.Odds)
                        .ThenInclude(o => o.Team)
            .Include(p => p.BetslipReactions)
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null) return null;

        var totalOdds = post.BetSlip.BetSlipOdds.Any() 
            ? post.BetSlip.BetSlipOdds.Aggregate(1m, (acc, odd) => acc * odd.ConstOdd) 
            : 1m;

        var reactions = post.BetslipReactions.ToList();
        var userReaction = currentUserId.HasValue
            ? reactions.FirstOrDefault(r => r.PlayerId == currentUserId.Value)?.ReactionType
            : null;

        return new BetSlipZoneDetailsDto
        {
            PostId = post.Id,
            BetSlipId = post.BetSlipId,
            PlayerName = post.Player.Name,
            PlayerLastName = post.Player.LastName,
            CreatedAt = post.CreatedAt,
            IsActive = post.IsActive,
            BetSlip = new BetSlipDetailsDto
            {
                Id = post.BetSlip.Id,
                Amount = post.BetSlip.Amount,
                Date = post.BetSlip.Date,
                Wynik = post.BetSlip.Wynik,
                TotalOdds = totalOdds,
                PotentialWin = post.BetSlip.PotentialWin ?? (post.BetSlip.Amount * totalOdds),
                BetSlipOdds = post.BetSlip.BetSlipOdds.Select(bso => new BetSlipOddDetailsDto
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
            },
            FireCount = reactions.Count(r => r.ReactionType == 1),
            ColdCount = reactions.Count(r => r.ReactionType == 2),
            SafeCount = reactions.Count(r => r.ReactionType == 3),
            CrazyCount = reactions.Count(r => r.ReactionType == 4),
            UserReaction = userReaction
        };
    }

    public async Task<int> CreatePostAsync(int playerId, int betSlipId)
    {
        var post = new BetSlipPost
        {
            PlayerId = playerId,
            BetSlipId = betSlipId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.BetSlipPosts.Add(post);
        await _context.SaveChangesAsync();

        return post.Id;
    }

    public async Task<bool> PostExistsForBetSlipAsync(int betSlipId)
    {
        return await _context.BetSlipPosts.AnyAsync(p => p.BetSlipId == betSlipId);
    }

    public async Task<bool> IsBetSlipActiveAsync(int betSlipId)
    {
        var betSlip = await _context.BetSlips.FirstOrDefaultAsync(bs => bs.Id == betSlipId);
        return betSlip != null && (betSlip.Wynik == null || betSlip.Wynik == 0);
    }

    public async Task<bool> IsBetSlipOwnedByPlayerAsync(int betSlipId, int playerId)
    {
        return await _context.BetSlips.AnyAsync(bs => bs.Id == betSlipId && bs.PlayerId == playerId);
    }

    public async Task SetReactionAsync(int postId, int playerId, int reactionType)
    {
        var existingReaction = await _context.BetslipReactions
            .FirstOrDefaultAsync(r => r.BetSlipPostId == postId && r.PlayerId == playerId);

        if (existingReaction != null)
        {
            if (existingReaction.ReactionType == reactionType)
            {
                // Toggle: usuń reakcję jeśli jest taka sama
                _context.BetslipReactions.Remove(existingReaction);
            }
            else
            {
                // Zmień reakcję na nową
                existingReaction.ReactionType = reactionType;
                existingReaction.CreatedAt = DateTime.UtcNow;
            }
        }
        else
        {
            // Dodaj nową reakcję
            var reaction = new BetslipReaction
            {
                BetSlipPostId = postId,
                PlayerId = playerId,
                ReactionType = reactionType,
                CreatedAt = DateTime.UtcNow
            };
            _context.BetslipReactions.Add(reaction);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int?> GetUserReactionAsync(int postId, int playerId)
    {
        var reaction = await _context.BetslipReactions
            .FirstOrDefaultAsync(r => r.BetSlipPostId == postId && r.PlayerId == playerId);
        
        return reaction?.ReactionType;
    }

    public async Task<BetSlipReactionSummaryDto> GetReactionSummaryAsync(int postId, int? currentUserId = null)
    {
        var reactions = await _context.BetslipReactions
            .Where(r => r.BetSlipPostId == postId)
            .ToListAsync();

        var userReaction = currentUserId.HasValue
            ? reactions.FirstOrDefault(r => r.PlayerId == currentUserId.Value)?.ReactionType
            : null;

        return new BetSlipReactionSummaryDto
        {
            FireCount = reactions.Count(r => r.ReactionType == 1),
            ColdCount = reactions.Count(r => r.ReactionType == 2),
            SafeCount = reactions.Count(r => r.ReactionType == 3),
            CrazyCount = reactions.Count(r => r.ReactionType == 4),
            UserReaction = userReaction
        };
    }

    public async Task<IEnumerable<BetSlipZoneItemDto>> GetMyPostsAsync(int playerId, int page, int pageSize)
    {
        var posts = await _context.BetSlipPosts
            .Include(p => p.Player)
            .Include(p => p.BetSlip)
                .ThenInclude(bs => bs!.BetSlipOdds)
                    .ThenInclude(bso => bso.Odds)
            .Include(p => p.BetslipReactions)
            .Where(p => p.PlayerId == playerId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return posts.Select(p => MapToItemDto(p, playerId));
    }

    private BetSlipZoneItemDto MapToItemDto(BetSlipPost post, int? currentUserId)
    {
        var totalOdds = post.BetSlip.BetSlipOdds.Any()
            ? post.BetSlip.BetSlipOdds.Aggregate(1m, (acc, odd) => acc * odd.ConstOdd)
            : 1m;

        var reactions = post.BetslipReactions.ToList();
        var userReaction = currentUserId.HasValue
            ? reactions.FirstOrDefault(r => r.PlayerId == currentUserId.Value)?.ReactionType
            : null;

        return new BetSlipZoneItemDto
        {
            PostId = post.Id,
            BetSlipId = post.BetSlipId,
            PlayerName = post.Player.Name,
            PlayerLastName = post.Player.LastName,
            CreatedAt = post.CreatedAt,
            IsActive = post.IsActive,
            Amount = post.BetSlip.Amount,
            TotalOdds = totalOdds,
            PotentialWin = post.BetSlip.PotentialWin ?? (post.BetSlip.Amount * totalOdds),
            OddsCount = post.BetSlip.BetSlipOdds.Count,
            Wynik = post.BetSlip.Wynik,
            FireCount = reactions.Count(r => r.ReactionType == 1),
            ColdCount = reactions.Count(r => r.ReactionType == 2),
            SafeCount = reactions.Count(r => r.ReactionType == 3),
            CrazyCount = reactions.Count(r => r.ReactionType == 4),
            UserReaction = userReaction
        };
    }
}

