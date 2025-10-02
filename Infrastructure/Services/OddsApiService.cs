using System.Net.Http.Json;
using Application.Config;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class OddsApiService : IOddsApiService
{
    private readonly HttpClient _httpClient;
    private readonly OddsApiOptions _options;

    public OddsApiService(HttpClient httpClient, IOptions<OddsApiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IEnumerable<Sport>> GetSportsAsync()
    {
        var url = $"https://api.the-odds-api.com/v4/sports/?apiKey={_options.ApiKey}";
        var sports = await _httpClient.GetFromJsonAsync<List<Sport>>(url);
        return sports ?? new List<Sport>();
    }

    public async Task<IEnumerable<Event>> GetEventsAndOddsBySportAsync(string sportKey)
    {
        var url = $"https://api.the-odds-api.com/v4/sports/{sportKey}/odds/?regions=eu&markets=h2h&bookmakers=onexbet&oddsFormat=decimal&apiKey={_options.ApiKey}";
        var eventsDto = await _httpClient.GetFromJsonAsync<List<OddsApiEventDto>>(url);
        var response = await _httpClient.GetStringAsync(url);
        Console.WriteLine(response);
        if (eventsDto == null || !eventsDto.Any())
            return new List<Event>();

        var events = new List<Event>();

        foreach (var dto in eventsDto)
        {
            var ev = new Event
            {
                ApiId = dto.Id,
                EventStatus = "scheduled", 
                CommenceTime = dto.CommenceTime,
                EventName = dto.HomeTeam+" vs "+dto.AwayTeam,
                EndTime = dto.CommenceTime.AddHours(3), 
                Odds = new List<Odds>()
            };
            
            var bookmaker = dto.Bookmakers.FirstOrDefault(b => b.Key == "onexbet");
            
            var market = bookmaker?.Markets.FirstOrDefault(m => m.Key == "h2h");

            if (market != null)
            {
                foreach (var outcome in market.Outcomes)
                {
                    var team = new Team
                    {
                        TeamName = outcome.Name
                    };

                    ev.Odds.Add(new Odds
                    {
                        OddsValue = outcome.Price,
                        LastUpdate = DateTime.UtcNow,
                        Team = team
                    });
                }
                events.Add(ev);
            }
            else
            {
                Console.WriteLine($"No market found for event: {dto.Id}");
            }
            

            
        }
        

        return events;
    }

    public async Task<IEnumerable<ScoreApiResponseDto>> GetScoresBySportAsync(string sportKey, int daysFrom = 3)
    {
        var url = $"https://api.the-odds-api.com/v4/sports/{sportKey}/scores/?daysFrom={daysFrom}&apiKey={_options.ApiKey}";
        var scores = await _httpClient.GetFromJsonAsync<List<ScoreApiResponseDto>>(url);
        return scores ?? new List<ScoreApiResponseDto>();
    }
}
