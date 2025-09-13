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
            RefreshTokenExp = registerPlayer.RefreshTokenExp
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
            RefreshTokenExp = dbPlayer.RefreshTokenExp
        };
    }
}