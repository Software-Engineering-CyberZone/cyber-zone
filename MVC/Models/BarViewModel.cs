using CyberZone.Domain.Entities;

namespace MVC.Models;

public class BarViewModel
{
    public List<MenuItem> Drinks { get; set; } = new();
    public List<MenuItem> Snacks { get; set; } = new();
}