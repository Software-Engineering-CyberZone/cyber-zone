namespace MVC.Models;

public class SessionItemViewModel
{
    public Guid Id { get; set; }
    public string ClubName { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PcNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string SessionState { get; set; } = "Pending";
    public DateTime SortDate { get; set; }
    public string TargetTime { get; set; } = string.Empty;
}