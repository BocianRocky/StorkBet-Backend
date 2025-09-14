using System.Net.Http.Json;
using Application.Config;
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
                EndTime = dto.CommenceTime.AddHours(3), 
                Odds = new List<Odds>()
            };
            Console.WriteLine($"Processing event: {dto.Id} - {dto.SportKey} - CommenceTime: {dto.CommenceTime} - HomeTeam: {dto.HomeTeam} - AwayTeam: {dto.AwayTeam}");
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
        Console.WriteLine("==== All Events ====");
        foreach (var e in events)
        {
            Console.WriteLine($"Event: {e.ApiId}, CommenceTime: {e.CommenceTime}, Odds count: {e.Odds.Count}");
            foreach (var odd in e.Odds)
            {
                Console.WriteLine($"    Team: {odd.Team.TeamName}, Price: {odd.OddsValue}");
            }
        }

        return events;
    }
}
