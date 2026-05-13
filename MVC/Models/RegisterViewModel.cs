using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "неправильне ім'я")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "неправильне ім'я")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "такий Email уже зареєстрований")]
    [EmailAddress(ErrorMessage = "Введіть Email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "некоректний пароль")]
    [StringLength(100, ErrorMessage = "Придумайте пароль", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Підтвердіть пароль")]
    [Compare("Password", ErrorMessage = "Паролі не збігаються")]
    public string ConfirmPassword { get; set; } = null!;
}