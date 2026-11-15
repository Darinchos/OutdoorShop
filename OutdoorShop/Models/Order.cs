using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutdoorShop.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Името е задължително.")]
        [StringLength(150, ErrorMessage = "Името може да е най-много 150 символа.")]
        [Display(Name = "Име")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Имейлът е задължителен.")]
        [EmailAddress(ErrorMessage = "Въведи валиден имейл.")]
        [StringLength(150, ErrorMessage = "Имейлът може да е най-много 150 символа.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Телефонът е задължителен.")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Въведи валиден телефон.")]
        [RegularExpression(@"^[0-9+\s\-()]+$", ErrorMessage = "Телефонът може да съдържа само цифри и + - ( ).")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Адресът е задължителен.")]
        [StringLength(300, ErrorMessage = "Адресът може да е най-много 300 символа.")]
        [Display(Name = "Адрес за доставка")]
        public string Address { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = OrderStatuses.Pending;

        [StringLength(255)]
        public string? CustomerUserId { get; set; }

        [StringLength(36)]
        public string? CheckoutKey { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<OrderItem> Items { get; set; } = new();
    }
}
