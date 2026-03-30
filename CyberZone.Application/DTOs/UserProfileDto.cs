namespace CyberZone.Application.DTOs;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public decimal Balance { get; set; }
    public List<TransactionDto> Transactions { get; set; } = new();
}