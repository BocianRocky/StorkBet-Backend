using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateBetSlipRequestDto
{
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [MinLength(1)]
    public List<int> OddsIds { get; set; } = new();
}


