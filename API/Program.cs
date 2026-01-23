using Application.Config;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Application.Services;
using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Repositories - Domain Interfaces
builder.Services.AddScoped<Domain.Interfaces.ISportRepository, SportRepository>();
builder.Services.AddScoped<Domain.Interfaces.IEventRepository, EventRepository>();
builder.Services.AddScoped<Domain.Interfaces.ITeamRepository, TeamRepository>();
builder.Services.AddScoped<Domain.Interfaces.IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<Domain.Interfaces.IBetSlipRepository, BetSlipRepository>();
builder.Services.AddScoped<Domain.Interfaces.IGroupRepository, GroupRepository>();
builder.Services.AddScoped<Domain.Interfaces.ITransactionRepository, TransactionRepository>();

// Repositories - Application Interfaces (extended with DTOs)
builder.Services.AddScoped<ISportRepositoryExtended, SportRepository>();
builder.Services.AddScoped<IEventRepositoryExtended, EventRepository>();
builder.Services.AddScoped<IBetSlipZoneRepository, BetSlipZoneRepository>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();

// Services
builder.Services.AddScoped<ISportService, SportService>();
builder.Services.AddScoped<ISportSyncService, SportSyncService>();
builder.Services.AddScoped<IOddsApiService, OddsApiService>();
builder.Services.AddScoped<IOddsSyncService, OddsSyncService>();
builder.Services.AddScoped<IOddsService, OddsService>();
builder.Services.AddScoped<IScoreSyncService, ScoreSyncService>();
builder.Services.AddScoped<IAdminStatisticsService, Infrastructure.Services.AdminStatisticsService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IEventService, EventService>();

// HttpClient dla OddsApiService
builder.Services.AddHttpClient<IOddsApiService, OddsApiService>();

// Konfiguracja OddsApi
builder.Services.Configure<OddsApiOptions>(
    builder.Configuration.GetSection("OddsApi")
);

// Controllers
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authentication & Authorization
var secretKey = builder.Configuration["SecretKey"] ?? "ThisIsAVeryLongSecretKeyForJWTTokenGenerationThatIsAtLeast32CharactersLong123456789";
var keyBytes = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = "https://localhost:5001",
        ValidAudience = "https://localhost:5001",
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("PlayerOnly", policy => policy.RequireRole("Player"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable static files serving
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();