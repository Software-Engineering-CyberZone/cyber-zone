using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public class TopUpViewModel
{
    [Required(ErrorMessage = "������ ���� ����������")]
    [Range(10, 10000, ErrorMessage = "���� �� ���� �� 10 �� 10000 ���")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "������ ����� ������")]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "����� ������ �� ������ 16 ���� ��� ������")]
    public string CardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "������ ����� 䳿")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "������ �� ���� MM/YY")]
    public string ExpiryDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "������ CVV")]
    [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV �� ������ 3 �����")]
    public string Cvv { get; set; } = string.Empty;
}