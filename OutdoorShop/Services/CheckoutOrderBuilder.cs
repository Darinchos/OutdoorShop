using OutdoorShop.Models;

namespace OutdoorShop.Services;

public static class CheckoutOrderBuilder
{
    public const int MaximumItemQuantity = 100_000;

    public static Dictionary<int, int> GetQuantities(IEnumerable<CartItem> cart)
    {
        var items = cart.ToList();
        if (items.Any(item => item.ProductId <= 0 || item.Quantity <= 0 || item.Quantity > MaximumItemQuantity))
            throw new ArgumentException("Количката съдържа невалидно количество.", nameof(cart));

        var quantities = new Dictionary<int, int>();
        foreach (var group in items.GroupBy(item => item.ProductId))
        {
            var total = group.Sum(item => (long)item.Quantity);
            if (total > MaximumItemQuantity)
                throw new ArgumentException("Количката съдържа невалидно количество.", nameof(cart));
            quantities[group.Key] = (int)total;
        }

        return quantities;
    }

    public static List<OrderItem> BuildItems(
        IReadOnlyDictionary<int, int> quantities,
        IReadOnlyDictionary<int, Product> products)
    {
        var result = new List<OrderItem>(quantities.Count);
        foreach (var (productId, quantity) in quantities)
        {
            if (quantity <= 0 || quantity > MaximumItemQuantity)
                throw new ArgumentOutOfRangeException(nameof(quantities));
            if (!products.TryGetValue(productId, out var product))
                throw new KeyNotFoundException($"Product {productId} was not found.");
            if (product.Stock < quantity)
                throw new InvalidOperationException($"Insufficient stock for product {productId}.");

            result.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity,
                TotalPrice = product.Price * quantity
            });
        }
        return result;
    }

    public static decimal CalculateTotal(IEnumerable<OrderItem> items) => items.Sum(item => item.TotalPrice);
}
