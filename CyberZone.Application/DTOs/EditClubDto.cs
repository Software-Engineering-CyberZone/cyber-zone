using System.ComponentModel.DataAnnotations;

namespace CyberZone.Application.DTOs;

public class EditClubDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Назва клубу є обов'язковою")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Phone { get; set; }
    
    [EmailAddress(ErrorMessage = "Невірний формат email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Вулиця обов'язкова")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Місто обов'язкове")]
    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    // WorkHours can be a JSON string or Dictionary in MVC, we will simplify as Dictionary
    public Dictionary<string, string> WorkHours { get; set; } = new();
}
