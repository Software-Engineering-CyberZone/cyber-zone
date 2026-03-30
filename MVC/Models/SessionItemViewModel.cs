namespace MVC.Models;

public class SessionItemViewModel
{
    public int Id { get; set; }
    public string ClubName { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PcNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}