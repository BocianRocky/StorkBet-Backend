using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AvailablePromotion> AvailablePromotions { get; set; }

    public virtual DbSet<BetSlip> BetSlips { get; set; }

    public virtual DbSet<BetSlipOdd> BetSlipOdds { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<GroupchatMessage> GroupchatMessages { get; set; }

    public virtual DbSet<Odd> Odds { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<PlayerGroup> PlayerGroups { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Sport> Sports { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AvailablePromotion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("AvailablePromotion_pk");

            entity.ToTable("AvailablePromotion");

            entity.Property(e => e.Availability)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("availability");

            entity.HasOne(d => d.Player).WithMany(p => p.AvailablePromotions)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AvailablePromotion_Gracz");

            entity.HasOne(d => d.Promotion).WithMany(p => p.AvailablePromotions)
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AvailablePromotion_Promotion");
        });

        modelBuilder.Entity<BetSlip>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BetSlip_pk");

            entity.ToTable("BetSlip");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Date)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Wynik).HasColumnName("wynik");

            entity.HasOne(d => d.Player).WithMany(p => p.BetSlips)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BetSlip_Gracz");
        });

        modelBuilder.Entity<BetSlipOdd>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BetSlipOdds_pk");

            entity.Property(e => e.OddsId).HasColumnName("Odds_Id");
            entity.Property(e => e.Wynik).HasColumnName("wynik");

            entity.HasOne(d => d.BetSlip).WithMany(p => p.BetSlipOdds)
                .HasForeignKey(d => d.BetSlipId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BetSlipOdds_BetSlip");

            entity.HasOne(d => d.Odds).WithMany(p => p.BetSlipOdds)
                .HasForeignKey(d => d.OddsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BetSlipOdds_Odds");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Event_pk");

            entity.ToTable("Event");

            entity.Property(e => e.ApiId)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.EventDate).HasColumnType("datetime");
            entity.Property(e => e.EventDateEnd).HasColumnType("datetime");
            entity.Property(e => e.EventName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.EventStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SportId).HasColumnName("Sport_Id");

            entity.HasOne(d => d.Sport).WithMany(p => p.Events)
                .HasForeignKey(d => d.SportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Event_Sport");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Group_pk");

            entity.ToTable("Group");

            entity.Property(e => e.GroupName).HasColumnName("Group_name");
        });

        modelBuilder.Entity<GroupchatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Groupchat_messages_pk");

            entity.ToTable("Groupchat_messages");

            entity.Property(e => e.MessageText)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Time)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Group).WithMany(p => p.GroupchatMessages)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Groupchat_messages_Group");

            entity.HasOne(d => d.Player).WithMany(p => p.GroupchatMessages)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Groupchat_messages_Gracz");
        });

        modelBuilder.Entity<Odd>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Odds_pk");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.LastUpdate).HasColumnType("datetime");
            entity.Property(e => e.OddsValue).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TeamId).HasColumnName("Team_Id");

            entity.HasOne(d => d.Event).WithMany(p => p.Odds)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Odds_Event");

            entity.HasOne(d => d.Team).WithMany(p => p.Odds)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Odds_Team");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Player_pk");

            entity.ToTable("Player");

            entity.Property(e => e.AccountBalance).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Email)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RefreshTokenExp).HasColumnType("datetime");
            entity.Property(e => e.Salt)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PlayerGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Player_Group_pk");

            entity.ToTable("Player_Group");

            entity.Property(e => e.IsGroupOwner).HasColumnName("isGroupOwner");
            entity.Property(e => e.JoinedAt)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Group).WithMany(p => p.PlayerGroups)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Player_Group_Group");

            entity.HasOne(d => d.Player).WithMany(p => p.PlayerGroups)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Player_Group_Gracz");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Promotion_pk");

            entity.ToTable("Promotion");

            entity.Property(e => e.PromotionName)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Sport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Sport_pk");

            entity.ToTable("Sport");

            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Group)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("group");
            entity.Property(e => e.HasOutrights).HasColumnName("has_outrights");
            entity.Property(e => e.Key)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("key");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Team_pk");

            entity.ToTable("Team");

            entity.Property(e => e.SportId).HasColumnName("Sport_id");
            entity.Property(e => e.TeamName)
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.HasOne(d => d.Sport).WithMany(p => p.Teams)
                .HasForeignKey(d => d.SportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Team_Sport");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Transaction_pk");

            entity.ToTable("Transaction");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Player).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Transaction_Gracz");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
