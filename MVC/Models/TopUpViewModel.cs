using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public class TopUpViewModel
{
    [Required(ErrorMessage = "Введіть суму поповнення")]
    [Range(10, 10000, ErrorMessage = "Сума має бути від 10 до 10000 грн")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Введіть номер картки")]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "Номер картки має містити 16 цифр без пробілів")]
    public string CardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть термін дії")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Формат має бути MM/YY")]
    public string ExpiryDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть CVV")]
    [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV має містити 3 цифри")]
    public string Cvv { get; set; } = string.Empty;
}