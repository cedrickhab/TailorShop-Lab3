using System.Globalization;
using Microsoft.Data.Sqlite;
using TailorShop.Data;
using TailorShop.Models;

namespace TailorShop.Controllers;

public class OrderController
{
    private const string SelectSql = @"SELECT o.Id, o.OrderNumber, o.CustomerId, cu.Name, cu.Phone, o.Garment, o.Description,
            o.OrderDate, o.DueDate, o.Price, o.RushFee, o.Total, o.Status, o.CreatedBy,
            COALESCE((SELECT SUM(p.Amount) FROM Payments p WHERE p.OrderId = o.Id), 0)
        FROM Orders o JOIN Customers cu ON cu.Id = o.CustomerId ";

    internal static Order Read(SqliteDataReader r) => new Order
    {
        Id = r.GetInt64(0), OrderNumber = r.GetString(1), CustomerId = r.GetInt64(2),
        CustomerName = r.GetString(3), CustomerPhone = r.GetString(4), Garment = r.GetString(5),
        Description = r.IsDBNull(6) ? "" : r.GetString(6),
        OrderDate = DateTime.ParseExact(r.GetString(7), Database.DateTimeFormat, CultureInfo.InvariantCulture),
        DueDate = DateTime.ParseExact(r.GetString(8), Database.DateFormat, CultureInfo.InvariantCulture),
        Price = r.GetInt64(9), RushFee = r.GetInt64(10), Total = r.GetInt64(11), Status = r.GetString(12),
        CreatedBy = r.GetInt64(13), Paid = r.GetInt64(14)
    };

    public static List<Order> Query(string where, params (string, object)[] args)
    {
        var list = new List<Order>();
        using var c = Database.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = SelectSql + where;
        foreach (var (k, v) in args) cmd.Parameters.AddWithValue(k, v);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(Read(r));
        return list;
    }

    public List<Order> GetAll(string statusFilter)
    {
        if (string.IsNullOrEmpty(statusFilter) || statusFilter == "All")
            return Query("ORDER BY o.Id DESC");
        return Query("WHERE o.Status = $st ORDER BY o.Id DESC", ("$st", statusFilter));
    }

    public Order Get(long id) => Query("WHERE o.Id = $id", ("$id", id)).FirstOrDefault();

    public string ValidatePrice(string text, out long price)
    {
        price = 0;
        var t = (text ?? "").Trim().Replace(",", "");
        if (!long.TryParse(t, NumberStyles.None, CultureInfo.InvariantCulture, out price) || price < 1000 || price > 5000000)
            return "Price must be a whole number between 1,000 and 5,000,000.";
        return null;
    }

    public string ValidateDeposit(string text, long total, out long deposit)
    {
        deposit = 0;
        var t = (text ?? "").Trim().Replace(",", "");
        if (t == "") return "Deposit must be a whole number from 0 up to the total.";
        if (!long.TryParse(t, NumberStyles.None, CultureInfo.InvariantCulture, out deposit))
            return "Deposit must be a whole number from 0 up to the total.";
        if (deposit > total) return "Deposit cannot be more than the total.";
        return null;
    }

    public string ValidateDueDate(DateTime due) =>
        due.Date < DateTime.Today ? "The due date cannot be in the past." : null;

    public string ValidateDescription(string d) =>
        (d ?? "").Trim().Length > 100 ? "Description cannot be more than 100 characters." : null;

    /// <summary>Saves the order and its deposit payment in one transaction. Returns the new order id.</summary>
    public long Create(Order o, long deposit, User user)
    {
        o.OrderDate = DateTime.Now;
        o.RushFee = BusinessRules.RushFee(o.Price, o.OrderDate, o.DueDate);
        o.Total = BusinessRules.Total(o.Price, o.RushFee);
        if (deposit < 0 || deposit > o.Total) throw new InvalidOperationException("Deposit cannot be more than the total.");

        using var c = Database.Open();
        using var tx = c.BeginTransaction();
        using var cmd = c.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"INSERT INTO Orders (OrderNumber, CustomerId, Garment, Description, OrderDate, DueDate, Price, RushFee, Total, Status, CreatedBy)
            VALUES ($tmp, $cid, $g, $d, $od, $dd, $p, $r, $t, 'Received', $u); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$tmp", Guid.NewGuid().ToString());
        cmd.Parameters.AddWithValue("$cid", o.CustomerId);
        cmd.Parameters.AddWithValue("$g", o.Garment);
        cmd.Parameters.AddWithValue("$d", (o.Description ?? "").Trim());
        cmd.Parameters.AddWithValue("$od", o.OrderDate.ToString(Database.DateTimeFormat, CultureInfo.InvariantCulture));
        cmd.Parameters.AddWithValue("$dd", o.DueDate.ToString(Database.DateFormat, CultureInfo.InvariantCulture));
        cmd.Parameters.AddWithValue("$p", o.Price);
        cmd.Parameters.AddWithValue("$r", o.RushFee);
        cmd.Parameters.AddWithValue("$t", o.Total);
        cmd.Parameters.AddWithValue("$u", user.Id);
        long id = (long)cmd.ExecuteScalar();

        // AUTOINCREMENT ids are never reused by SQLite, so order numbers are unique for ever.
        using var upd = c.CreateCommand();
        upd.Transaction = tx;
        upd.CommandText = "UPDATE Orders SET OrderNumber = $n WHERE Id = $id";
        upd.Parameters.AddWithValue("$n", BusinessRules.FormatOrderNumber(id));
        upd.Parameters.AddWithValue("$id", id);
        upd.ExecuteNonQuery();

        using var pay = c.CreateCommand();
        pay.Transaction = tx;
        pay.CommandText = "INSERT INTO Payments (OrderId, Amount, PaymentDate) VALUES ($o, $a, $d)";
        pay.Parameters.AddWithValue("$o", id);
        pay.Parameters.AddWithValue("$a", deposit);
        pay.Parameters.AddWithValue("$d", o.OrderDate.ToString(Database.DateTimeFormat, CultureInfo.InvariantCulture));
        pay.ExecuteNonQuery();

        tx.Commit();
        return id;
    }

    /// <summary>Moves the order to the next status. Returns an error message, or null on success.</summary>
    public string AdvanceStatus(long orderId, string requested)
    {
        var o = Get(orderId);
        if (o == null) return "The order was not found.";
        var next = BusinessRules.NextStatus(o.Status);
        if (next == null) return "This order is already Collected.";
        if (requested != next) return $"Refused. Only {next} is allowed next.";
        if (next == "Collected" && o.Balance > 0) return "The balance must be paid before collection.";
        using var c = Database.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE Orders SET Status = $s WHERE Id = $id";
        cmd.Parameters.AddWithValue("$s", next);
        cmd.Parameters.AddWithValue("$id", orderId);
        cmd.ExecuteNonQuery();
        return null;
    }

    public string RecordPayment(long orderId, string text)
    {
        var o = Get(orderId);
        if (o == null) return "The order was not found.";
        var t = (text ?? "").Trim().Replace(",", "");
        if (!long.TryParse(t, NumberStyles.None, CultureInfo.InvariantCulture, out var amount) || amount < 1 || amount > o.Balance)
            return $"Payment must be between 1 and {o.Balance:N0}.";
        using var c = Database.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO Payments (OrderId, Amount, PaymentDate) VALUES ($o, $a, $d)";
        cmd.Parameters.AddWithValue("$o", orderId);
        cmd.Parameters.AddWithValue("$a", amount);
        cmd.Parameters.AddWithValue("$d", DateTime.Now.ToString(Database.DateTimeFormat, CultureInfo.InvariantCulture));
        cmd.ExecuteNonQuery();
        return null;
    }

    public string Delete(User user, long orderId)
    {
        if (user == null || !user.IsAdmin) return "Only the Administrator can delete orders.";
        var o = Get(orderId);
        if (o == null) return "The order was not found.";
        if (o.Status == "Collected") return "A collected order cannot be deleted.";
        using var c = Database.Open();
        using var tx = c.BeginTransaction();
        using var cmd = c.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "DELETE FROM Payments WHERE OrderId = $id; DELETE FROM Orders WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", orderId);
        cmd.ExecuteNonQuery();
        tx.Commit();
        return null;
    }
}
