using System.Text.Json.Serialization;

public class OddsApiEventDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("sport_key")]
    public string SportKey { get; set; } = null!;

    [JsonPropertyName("commence_time")]
    public DateTime CommenceTime { get; set; }

    [JsonPropertyName("home_team")]
    public string HomeTeam { get; set; } = null!;

    [JsonPropertyName("away_team")]
    public string AwayTeam { get; set; } = null!;

    [JsonPropertyName("bookmakers")]
    public List<OddsApiBookmakerDto> Bookmakers { get; set; } = new();
}

public class OddsApiBookmakerDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = null!;

    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    [JsonPropertyName("markets")]
    public List<OddsApiMarketDto> Markets { get; set; } = new();
}

public class OddsApiMarketDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = null!;

    [JsonPropertyName("outcomes")]
    public List<OddsApiOutcomeDto> Outcomes { get; set; } = new();
}

public class OddsApiOutcomeDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}
