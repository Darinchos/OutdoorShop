namespace OutdoorShop.Models;

public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Shipped = "Shipped";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlyList<string> All =
        [Pending, Processing, Shipped, Delivered, Cancelled];

    public static IReadOnlyList<string> AllowedTransitions(string currentStatus) => currentStatus switch
    {
        Pending => [Pending, Processing, Cancelled],
        Processing => [Processing, Shipped, Cancelled],
        Shipped => [Shipped, Delivered],
        Delivered => [Delivered],
        Cancelled => [Cancelled, Pending],
        _ => []
    };

    public static string BulgarianLabel(string status) => status switch
    {
        Processing => "В обработка",
        Shipped => "Изпратена",
        Delivered => "Доставена",
        Cancelled => "Отказана",
        _ => "Нова"
    };
}
