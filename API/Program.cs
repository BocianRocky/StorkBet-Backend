using Application.Config;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;
using Infrastructure.Repositories;
using Application.Services;
using Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);


builder.Services.AddScoped<ISportRepository, SportRepository>();
builder.Services.AddScoped<ISportService, SportService>();
builder.Services.AddScoped<ISportSyncService, SportSyncService>();
builder.Services.AddScoped<IOddsApiService, OddsApiService>();
builder.Services.AddHttpClient<IOddsApiService, OddsApiService>();

builder.Services.Configure<OddsApiOptions>(
    builder.Configuration.GetSection("OddsApi")
);


builder.Services.AddControllers();


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