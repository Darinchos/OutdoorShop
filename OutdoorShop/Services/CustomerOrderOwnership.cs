using Microsoft.EntityFrameworkCore;
using OutdoorShop.Models;

namespace OutdoorShop.Services;

public static class CustomerOrderOwnership
{
    public static IQueryable<Order> ForCustomer(IQueryable<Order> orders, string userId) =>
        orders.Where(order => order.CustomerUserId == userId);
}
