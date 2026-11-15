using System.ComponentModel.DataAnnotations;

namespace OutdoorShop.ViewModels;

public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "Имейлът е задължителен.")]
    [EmailAddress(ErrorMessage = "Въведи валиден имейл.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Паролата е задължителна.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Паролата трябва да е поне 8 символа.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Потвърди паролата.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Паролите не съвпадат.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Имейлът е задължителен.")]
    [EmailAddress(ErrorMessage = "Въведи валиден имейл.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Паролата е задължителна.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Запомни ме")]
    public bool RememberMe { get; set; }
}

public sealed class CustomerProfileViewModel
{
    [Required(ErrorMessage = "Името е задължително.")]
    [StringLength(150)]
    [Display(Name = "Име и фамилия")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Въведи валиден телефон.")]
    [StringLength(30)]
    [Display(Name = "Телефон")]
    public string? Phone { get; set; }

    [StringLength(300)]
    [Display(Name = "Адрес за доставка")]
    public string? Address { get; set; }

    public string Email { get; set; } = string.Empty;
}
