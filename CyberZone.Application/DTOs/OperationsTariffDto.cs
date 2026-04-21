using System.ComponentModel.DataAnnotations;
using CyberZone.Domain.Enums;

namespace CyberZone.Application.DTOs;

public class CreateTariffDto
{
    public Guid ClubId { get; set; }

    [Required(ErrorMessage = "Назва обов'язкова")]
    public string Name { get; set; } = string.Empty;

    public TariffType Type { get; set; }

    [Required(ErrorMessage = "Ціна обов'язкова")]
    [Range(0, 10000, ErrorMessage = "Ціна повинна бути між 0 та 10000")]
    public decimal PricePerHour { get; set; }

    public string? Description { get; set; }
}

public class EditTariffDto
{
    public Guid Id { get; set; }
    public Guid ClubId { get; set; }

    [Required(ErrorMessage = "Назва обов'язкова")]
    public string Name { get; set; } = string.Empty;

    public TariffType Type { get; set; }

    [Required(ErrorMessage = "Ціна обов'язкова")]
    [Range(0, 10000, ErrorMessage = "Ціна повинна бути між 0 та 10000")]
    public decimal PricePerHour { get; set; }

    public string? Description { get; set; }
}
