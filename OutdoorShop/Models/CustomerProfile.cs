using System.ComponentModel.DataAnnotations;

namespace OutdoorShop.Models;

public class CustomerProfile
{
    [Key, StringLength(255)]
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }
}
