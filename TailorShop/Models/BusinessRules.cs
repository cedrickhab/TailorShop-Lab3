namespace TailorShop.Models;

/// <summary>Business rules from section 6 of the requirements.</summary>
public static class BusinessRules
{
    public const string ShopName = "Tailor Shop Kigali";
    public static readonly string[] Garments = { "Shirt", "Trousers", "Dress", "Suit", "Skirt", "Other" };
    public static readonly string[] Statuses = { "Received", "Cutting", "Sewing", "Ready", "Collected" };

    /// <summary>20% of price (rounded down) when the due date is 3 days or fewer after the order date.</summary>
    public static long RushFee(long price, DateTime orderDate, DateTime dueDate)
    {
        int days = (dueDate.Date - orderDate.Date).Days;
        return days <= 3 ? price * 20 / 100 : 0;
    }

    public static long Total(long price, long rushFee) => price + rushFee;

    /// <summary>The only allowed next status, or null when the order is Collected.</summary>
    public static string NextStatus(string current)
    {
        int i = Array.IndexOf(Statuses, current);
        return i >= 0 && i < Statuses.Length - 1 ? Statuses[i + 1] : null;
    }

    public static bool IsOverdue(Order o, DateTime today) =>
        o.DueDate.Date < today.Date && o.Status != "Ready" && o.Status != "Collected";

    public static string FormatOrderNumber(long n) => "ORD-" + n.ToString("D4");

    public static string Money(long v) => v.ToString("N0") + " RWF";
}
