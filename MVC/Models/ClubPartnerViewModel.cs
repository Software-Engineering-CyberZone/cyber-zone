using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public class ClubPartnerViewModel
{
    [Required(ErrorMessage = " Email")]
    [EmailAddress(ErrorMessage = " Email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Номер телефону")]
    [Phone(ErrorMessage = "Некоректний номер телефону")]
    public string Phone { get; set; } = null!;
}