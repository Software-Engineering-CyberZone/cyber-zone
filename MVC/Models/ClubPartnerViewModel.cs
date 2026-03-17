using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public class ClubPartnerViewModel
{
    [Required(ErrorMessage = "Введіть ваш Email")]
    [EmailAddress(ErrorMessage = "Некоректний формат Email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Введіть номер телефону")]
    [Phone(ErrorMessage = "Некоректний формат номеру")]
    public string Phone { get; set; } = null!;
}