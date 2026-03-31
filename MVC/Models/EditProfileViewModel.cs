using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MVC.Models;

public class EditProfileViewModel
{
    [Required(ErrorMessage = "Поле Email є обов'язковим")]
    [EmailAddress(ErrorMessage = "Некоректний формат Email")]
    public string Email { get; set; } = null!;

    [StringLength(200, ErrorMessage = "Ім'я не може перевищувати 200 символів")]
    [Display(Name = "Повне ім'я")]
    public string? FullName { get; set; }

    [StringLength(500, ErrorMessage = "Біографія не може перевищувати 500 символів")]
    [Display(Name = "Про себе")]
    public string? Bio { get; set; }

    [RegularExpression(@"^\+?[\d\s\-\(\)]{7,20}$",
        ErrorMessage = "Некоректний формат телефону")]
    [Display(Name = "Телефон")]
    public string? Phone { get; set; }

    [StringLength(200, ErrorMessage = "Локація не може перевищувати 200 символів")]
    [Display(Name = "Локація")]
    public string? Location { get; set; }

    [Url(ErrorMessage = "Некоректний формат URL")]
    [StringLength(500, ErrorMessage = "URL не може перевищувати 500 символів")]
    [Display(Name = "Веб-сайт")]
    public string? WebsiteUrl { get; set; }

    [Display(Name = "Фото профілю")]
    public IFormFile? ProfileImage { get; set; }

    public string? ExistingProfileImagePath { get; set; }
}
