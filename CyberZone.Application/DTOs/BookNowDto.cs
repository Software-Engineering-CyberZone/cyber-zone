using System.ComponentModel.DataAnnotations;
using CyberZone.Domain.Enums;

namespace CyberZone.Application.DTOs;

/// <summary>
/// Form model for booking a single PC from the club map.
/// User picks a tariff, start time, and duration in hours.
/// </summary>
public class BookNowDto
{
    public Guid ClubId { get; set; }
    public string? ClubName { get; set; }

    public Guid HardwareId { get; set; }
    public string? PcNumber { get; set; }

    [Required]
    public Guid TariffId { get; set; }

    [Required]
    public DateTime StartTime { get; set; } = DateTime.Now;

    [Range(1, 24, ErrorMessage = "Тривалість має бути від 1 до 24 годин.")]
    public int Hours { get; set; } = 1;

    [StringLength(500)]
    public string? Notes { get; set; }

    // Populated on GET for the form dropdown.
    public List<TariffOption> AvailableTariffs { get; set; } = [];

    // Populated on GET to show the user their current balance.
    public decimal UserBalance { get; set; }
}

public class TariffOption
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public TariffType Type { get; set; }
    public decimal PricePerHour { get; set; }
}
