using OutdoorShop.Models;

namespace OutdoorShop.Services;

public static class OrderInventoryPolicy
{
    public static int StockDelta(string currentStatus, string nextStatus, int quantity)
    {
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (currentStatus != OrderStatuses.Cancelled && nextStatus == OrderStatuses.Cancelled)
            return quantity;
        if (currentStatus == OrderStatuses.Cancelled && nextStatus != OrderStatuses.Cancelled)
            return -quantity;
        return 0;
    }
}
