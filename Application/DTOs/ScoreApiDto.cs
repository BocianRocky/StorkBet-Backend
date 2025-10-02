using System.Text.Json.Serialization;

namespace Application.DTOs;

public class ScoreApiResponseDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("sport_key")]
    public string SportKey { get; set; } = null!;

    [JsonPropertyName("sport_title")]
    public string SportTitle { get; set; } = null!;

    [JsonPropertyName("commence_time")]
    public DateTime CommenceTime { get; set; }

    [JsonPropertyName("completed")]
    public bool Completed { get; set; }

    [JsonPropertyName("home_team")]
    public string HomeTeam { get; set; } = null!;

    [JsonPropertyName("away_team")]
    public string AwayTeam { get; set; } = null!;

    [JsonPropertyName("scores")]
    public List<ScoreDto> Scores { get; set; } = new();

    [JsonPropertyName("last_update")]
    public DateTime? LastUpdate { get; set; }
}

public class ScoreDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("score")]
    public string Score { get; set; } = null!;
}

