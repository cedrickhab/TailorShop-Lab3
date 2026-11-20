using System.Text.RegularExpressions;
using TailorShop.Data;
using TailorShop.Models;

namespace TailorShop.Controllers;

public class CustomerController
{
    public List<Customer> GetAll(string search = "")
    {
        var list = new List<Customer>();
        using var c = Database.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"SELECT cu.Id, cu.Name, cu.Phone, cu.Chest, cu.Waist, cu.Hip, cu.Shoulder, cu.Sleeve, cu.Length,
                (SELECT COUNT(*) FROM Orders o WHERE o.CustomerId = cu.Id)
            FROM Customers cu
            WHERE $s = '' OR lower(cu.Name) LIKE '%' || lower($s) || '%' OR cu.Phone LIKE '%' || $s || '%'
            ORDER BY cu.Name COLLATE NOCASE";
        cmd.Parameters.AddWithValue("$s", (search ?? "").Trim());
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Customer
            {
                Id = r.GetInt64(0), Name = r.GetString(1), Phone = r.GetString(2),
                Chest = r.GetInt32(3), Waist = r.GetInt32(4), Hip = r.GetInt32(5),
                Shoulder = r.GetInt32(6), Sleeve = r.GetInt32(7), Length = r.GetInt32(8),
                OrderCount = r.GetInt32(9)
            });
        return list;
    }

    public string ValidateName(string name)
    {
        name = (name ?? "").Trim();
        return name.Length < 2 || name.Length > 60 ? "Name must be between 2 and 60 characters." : null;
    }

    public string ValidatePhone(string phone, long excludeId)
    {
        phone = (phone ?? "").Trim();
        if (!Regex.IsMatch(phone, @"^07\d{8}$")) return "Phone must be 10 digits starting with 07.";
        using var c = Database.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Customers WHERE Phone = $p AND Id <> $id";
        cmd.Parameters.AddWithValue("$p", phone);
        cmd.Parameters.AddWithValue("$id", excludeId);
        return (long)cmd.ExecuteScalar() > 0 ? "This phone is already used." : null;
    }

    public string ValidateMeasurement(string field, string text, out int value)
    {
        value = 0;
        if (!Regex.IsMatch((text ?? "").Trim(), @"^\d{1,3}$") || !int.TryParse(text.Trim(), out value) || value < 20 || value > 200)
            return $"{field} must be a whole number between 20 and 200.";
        return null;
    }

    public void Save(Customer cu)
    {
        using var c = Database.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = cu.Id == 0
            ? @"INSERT INTO Customers (Name, Phone, Chest, Waist, Hip, Shoulder, Sleeve, Length)
                VALUES ($n, $p, $c, $w, $h, $s, $sl, $l)"
            : @"UPDATE Customers SET Name=$n, Phone=$p, Chest=$c, Waist=$w, Hip=$h, Shoulder=$s, Sleeve=$sl, Length=$l
                WHERE Id=$id";
        cmd.Parameters.AddWithValue("$n", cu.Name.Trim());
        cmd.Parameters.AddWithValue("$p", cu.Phone.Trim());
        cmd.Parameters.AddWithValue("$c", cu.Chest);
        cmd.Parameters.AddWithValue("$w", cu.Waist);
        cmd.Parameters.AddWithValue("$h", cu.Hip);
        cmd.Parameters.AddWithValue("$s", cu.Shoulder);
        cmd.Parameters.AddWithValue("$sl", cu.Sleeve);
        cmd.Parameters.AddWithValue("$l", cu.Length);
        cmd.Parameters.AddWithValue("$id", cu.Id);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Returns an error message, or null when the customer was deleted.</summary>
    public string Delete(User user, long id)
    {
        if (user == null || !user.IsAdmin) return "Only the Administrator can delete customers.";
        using var c = Database.Open();
        using (var chk = c.CreateCommand())
        {
            chk.CommandText = "SELECT COUNT(*) FROM Orders WHERE CustomerId = $id";
            chk.Parameters.AddWithValue("$id", id);
            if ((long)chk.ExecuteScalar() > 0) return "This customer cannot be deleted because they have orders.";
        }
        using var cmd = c.CreateCommand();
        cmd.CommandText = "DELETE FROM Customers WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
        return null;
    }
}
