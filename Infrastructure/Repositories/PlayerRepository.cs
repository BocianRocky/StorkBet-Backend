using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Player = Domain.Entities.Player;

namespace Infrastructure.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly AppDbContext _context;

    public PlayerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RegisterPlayer registerPlayer)
    {
        var player = new Data.Player()
        {
            Name = registerPlayer.Name,
            LastName = registerPlayer.LastName,
            Email = registerPlayer.Email,
            AccountBalance = 0.00m,
            Password = registerPlayer.Password,
            Salt = registerPlayer.Salt,
            RefreshToken = registerPlayer.RefreshToken,
            RefreshTokenExp = registerPlayer.RefreshTokenExp,
            Role = (int)Role.Player
        };

        _context.Players.Add(player);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Players.AnyAsync(p => p.Email == email);
    }

    public async Task<Player?> GetByEmailAsync(string email)
    {
        var dbPlayer = await _context.Players.FirstOrDefaultAsync(p => p.Email == email);
        
        if (dbPlayer == null)
            return null;
            
        return new Player
        {
            Id = dbPlayer.Id,
            Name = dbPlayer.Name,
            LastName = dbPlayer.LastName,
            Email = dbPlayer.Email,
            Password = dbPlayer.Password,
            Salt = dbPlayer.Salt,
            AccountBalance = dbPlayer.AccountBalance,
            RefreshToken = dbPlayer.RefreshToken,
            RefreshTokenExp = dbPlayer.RefreshTokenExp,
            Role = (Role)dbPlayer.Role
        };
    }

    public async Task<Player?> GetByRefreshTokenAsync(string refreshToken)
    {
        var dbPlayer = await _context.Players.FirstOrDefaultAsync(p => p.RefreshToken == refreshToken);
        
        if (dbPlayer == null)
            return null;
            
        return new Player
        {
            Id = dbPlayer.Id,
            Name = dbPlayer.Name,
            LastName = dbPlayer.LastName,
            Email = dbPlayer.Email,
            Password = dbPlayer.Password,
            Salt = dbPlayer.Salt,
            AccountBalance = dbPlayer.AccountBalance,
            RefreshToken = dbPlayer.RefreshToken,
            RefreshTokenExp = dbPlayer.RefreshTokenExp,
            Role = (Role)dbPlayer.Role
        };
    }

    public async Task UpdateRefreshTokenAsync(int playerId, string refreshToken, DateTime refreshTokenExp)
    {
        var dbPlayer = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);
        
        if (dbPlayer != null)
        {
            dbPlayer.RefreshToken = refreshToken;
            dbPlayer.RefreshTokenExp = refreshTokenExp;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Player?> GetByIdAsync(int playerId)
    {
        var dbPlayer = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);
        if (dbPlayer == null) return null;
        return new Player
        {
            Id = dbPlayer.Id,
            Name = dbPlayer.Name,
            LastName = dbPlayer.LastName,
            Email = dbPlayer.Email,
            Password = dbPlayer.Password,
            Salt = dbPlayer.Salt,
            AccountBalance = dbPlayer.AccountBalance,
            RefreshToken = dbPlayer.RefreshToken,
            RefreshTokenExp = dbPlayer.RefreshTokenExp,
            Role = (Role)dbPlayer.Role
        };
    }
    public async Task DeletePlayerAsync(int playerId)
    {
        var dbPlayer = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);
        if (dbPlayer != null)
        {
            _context.Players.Remove(dbPlayer);
            await _context.SaveChangesAsync();
        }
    }
}