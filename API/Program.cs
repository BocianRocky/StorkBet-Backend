using Application.Config;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;
using Infrastructure.Repositories;
using Application.Services;
using Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Repositories
builder.Services.AddScoped<ISportRepository, SportRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();

// Services
builder.Services.AddScoped<ISportService, SportService>();
builder.Services.AddScoped<ISportSyncService, SportSyncService>();
builder.Services.AddScoped<IOddsApiService, OddsApiService>();
builder.Services.AddScoped<IOddsSyncService, OddsSyncService>();

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();