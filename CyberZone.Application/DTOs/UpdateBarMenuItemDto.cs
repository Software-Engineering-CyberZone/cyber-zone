using System;
using System.ComponentModel.DataAnnotations;

namespace CyberZone.Application.DTOs;

public class UpdateBarMenuItemDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public string? Category { get; set; }

    public bool IsAvailable { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public bool IsActive { get; set; }

    public string? ImageUrl { get; set; }
}
