using OutdoorShop.Models;

namespace OutdoorShop.ViewModels;

public sealed class HomeViewModel
{
    public IReadOnlyList<Product> FeaturedProducts { get; init; } = [];
    public IReadOnlyList<Category> DiscoveryCategories { get; init; } = [];
}

public sealed record CheckoutSummaryLine(string Name, decimal UnitPrice, int Quantity, int Stock)
{
    public decimal Total => UnitPrice * Quantity;
}
