using System.ComponentModel.DataAnnotations;

namespace OutdoorShop.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Името е задължително.")]
        [StringLength(100, ErrorMessage = "Името може да е най-много 100 символа.")]
        [Display(Name = "Име на категорията")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Описанието може да е най-много 500 символа.")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
