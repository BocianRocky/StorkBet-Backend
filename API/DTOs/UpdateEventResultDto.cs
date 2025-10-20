namespace API.DTOs;

public class UpdateEventResultDto
{
    public int EventId { get; set; }
    public int Team1Id { get; set; }
    public int Team2Id { get; set; }
    public int Team1Score { get; set; }
    public int Team2Score { get; set; }
}