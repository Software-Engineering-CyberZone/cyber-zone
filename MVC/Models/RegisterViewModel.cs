using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Поле Нікнейм є обов'язковим")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "Поле Ім'я є обов'язковим")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Поле Email є обов'язковим")]
    [EmailAddress(ErrorMessage = "Некоректний формат Email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Поле Пароль є обов'язковим")]
    [StringLength(100, ErrorMessage = "Пароль має містити щонайменше {2} символів.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Підтвердження паролю")]
    [Compare("Password", ErrorMessage = "Паролі не співпадають.")]
    public string ConfirmPassword { get; set; } = null!;
}