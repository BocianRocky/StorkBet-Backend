namespace API.DTOs;

public class WithdrawalRequestDto
{
    public decimal Amount { get; set; }
    public int PaymentMethod { get; set; }
}

