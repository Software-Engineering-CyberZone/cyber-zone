using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public class TopUpViewModel
{
    [Required(ErrorMessage = "неправильна сума")]
    [Range(10, 10000, ErrorMessage = "від 10 до 10000 грн")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "неіснуючий номер")]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "вкажіть номер карти з 16 символів")]
    public string CardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "неправильна дата")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "введіть дату в форматі MM/YY")]
    public string ExpiryDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "неправильне CVV")]
    [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV")]
    public string Cvv { get; set; } = string.Empty;
}