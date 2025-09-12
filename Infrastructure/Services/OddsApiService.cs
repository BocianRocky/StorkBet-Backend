using System.Net.Http.Json;
using Application.Config;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class OddsApiService:IOddsApiService
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
}