using System.Globalization;
using TailorShop.Data;
using TailorShop.Models;

namespace TailorShop.Controllers;

public class ReportController
{
    public List<Order> Overdue() =>
        OrderController.Query("WHERE o.DueDate < $today AND o.Status NOT IN ('Ready','Collected') ORDER BY o.DueDate ASC",
            ("$today", DateTime.Today.ToString(Database.DateFormat, CultureInfo.InvariantCulture)));

    public List<Order> Unpaid() =>
        OrderController.Query("ORDER BY o.Id").Where(o => o.Balance > 0).OrderByDescending(o => o.Balance).ToList();

    public List<DailyIncome> Income(DateTime start, DateTime end)
    {
        var list = new List<DailyIncome>();
        using var c = Database.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"SELECT substr(PaymentDate, 1, 10) AS D, SUM(Amount) FROM Payments
            WHERE substr(PaymentDate, 1, 10) BETWEEN $s AND $e AND Amount > 0
            GROUP BY D ORDER BY D";
        cmd.Parameters.AddWithValue("$s", start.ToString(Database.DateFormat, CultureInfo.InvariantCulture));
        cmd.Parameters.AddWithValue("$e", end.ToString(Database.DateFormat, CultureInfo.InvariantCulture));
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(new DailyIncome { Date = r.GetString(0), Amount = r.GetInt64(1) });
        return list;
    }
}
