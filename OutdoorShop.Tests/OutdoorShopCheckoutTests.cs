using OutdoorShop.Models;
using OutdoorShop.Services;
using OutdoorShop.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace OutdoorShop.Tests;

public class OutdoorShopCheckoutTests
{
    [Fact]
    public void BuildItems_UsesDatabasePriceAndCalculatesLineAndOrderTotals()
    {
        var quantities = CheckoutOrderBuilder.GetQuantities([new CartItem { ProductId = 4, Quantity = 3, Price = 0.01m }]);
        var products = new Dictionary<int, Product>
        {
            [4] = new Product { Id = 4, Name = "Палатка", Price = 79.90m, Stock = 8 }
        };

        var items = CheckoutOrderBuilder.BuildItems(quantities, products);

        Assert.Single(items);
        Assert.Equal(79.90m, items[0].Price);
        Assert.Equal(3, items[0].Quantity);
        Assert.Equal(239.70m, items[0].TotalPrice);
        Assert.Equal(239.70m, CheckoutOrderBuilder.CalculateTotal(items));
    }

    [Fact]
    public void GetQuantities_MergesDuplicateProductRows()
    {
        var result = CheckoutOrderBuilder.GetQuantities(
        [
            new CartItem { ProductId = 4, Quantity = 2 },
            new CartItem { ProductId = 4, Quantity = 3 }
        ]);

        Assert.Equal(5, result[4]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(100001)]
    public void GetQuantities_RejectsInvalidQuantities(int quantity)
    {
        Assert.Throws<ArgumentException>(() => CheckoutOrderBuilder.GetQuantities(
            [new CartItem { ProductId = 4, Quantity = quantity }]));
    }

    [Fact]
    public void BuildItems_RejectsInsufficientStock()
    {
        var quantities = new Dictionary<int, int> { [4] = 3 };
        var products = new Dictionary<int, Product>
        {
            [4] = new Product { Id = 4, Name = "Раница", Price = 50m, Stock = 2 }
        };

        Assert.Throws<InvalidOperationException>(() => CheckoutOrderBuilder.BuildItems(quantities, products));
    }
}

public class OrderInventoryPolicyTests
{
    [Theory]
    [InlineData(OrderStatuses.Pending, OrderStatuses.Cancelled, 4, 4)]
    [InlineData(OrderStatuses.Processing, OrderStatuses.Cancelled, 2, 2)]
    [InlineData(OrderStatuses.Cancelled, OrderStatuses.Pending, 5, -5)]
    [InlineData(OrderStatuses.Pending, OrderStatuses.Processing, 3, 0)]
    [InlineData(OrderStatuses.Shipped, OrderStatuses.Delivered, 1, 0)]
    public void StockDelta_MatchesTransition(string current, string next, int quantity, int expected)
    {
        Assert.Equal(expected, OrderInventoryPolicy.StockDelta(current, next, quantity));
    }

    [Theory]
    [InlineData(OrderStatuses.Shipped, OrderStatuses.Cancelled)]
    [InlineData(OrderStatuses.Delivered, OrderStatuses.Cancelled)]
    [InlineData(OrderStatuses.Delivered, OrderStatuses.Pending)]
    public void AllowedTransitions_PreventInvalidRestockingTransitions(string current, string next)
    {
        Assert.DoesNotContain(next, OrderStatuses.AllowedTransitions(current));
    }
}

public class AuthorizationAndOwnershipTests
{
    [Fact]
    public void AdminOrderController_RequiresAdminRole()
    {
        var authorization = typeof(OrdersController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>().Single();

        Assert.Equal("Admin", authorization.Roles);
    }

    [Fact]
    public void CustomerOrderDetails_RequiresAuthentication()
    {
        var action = typeof(AccountController).GetMethod(nameof(AccountController.MyOrder));

        Assert.NotNull(action);
        Assert.Contains(action!.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true),
            attribute => attribute is AuthorizeAttribute);
    }

    [Fact]
    public void CustomerOrderQuery_ReturnsOnlyTheSignedInCustomersOrders()
    {
        var orders = new[]
        {
            new Order { Id = 1, CustomerUserId = "customer-a" },
            new Order { Id = 2, CustomerUserId = "customer-b" },
            new Order { Id = 3, CustomerUserId = null }
        }.AsQueryable();

        var visibleOrders = CustomerOrderOwnership.ForCustomer(orders, "customer-a").ToList();

        Assert.Equal(new[] { 1 }, visibleOrders.Select(order => order.Id));
    }

    [Fact]
    public void ProductCatalog_IsAnonymousButManagementControllerRequiresAdmin()
    {
        var productsAction = typeof(ProductsController).GetMethod(nameof(ProductsController.Index));
        var productControllerAuth = typeof(ProductsController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Single();

        Assert.Equal("Admin", productControllerAuth.Roles);
        Assert.NotNull(productsAction);
        Assert.Contains(productsAction!.GetCustomAttributes(inherit: true), attribute => attribute is AllowAnonymousAttribute);
    }
}
