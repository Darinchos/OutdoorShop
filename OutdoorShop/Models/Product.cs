using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutdoorShop.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Името е задължително.")]
        [StringLength(150, ErrorMessage = "Името може да е най-много 150 символа.")]
        [Display(Name = "Име на продукта")]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Описанието може да е най-много 2000 символа.")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 999999.99, ErrorMessage = "Цената трябва да е по-голяма от 0.")]
        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Наличността не може да е отрицателна.")]
        [Display(Name = "Наличност")]
        public int Stock { get; set; }

        [StringLength(500, ErrorMessage = "URL адресът може да е най-много 500 символа.")]
        [Display(Name = "URL на снимката")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Категория")]
        [Range(1, int.MaxValue, ErrorMessage = "Избери категория.")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }
    }
}
