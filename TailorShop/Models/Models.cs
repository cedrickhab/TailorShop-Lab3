namespace TailorShop.Models;

public class User
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
    public int FailedAttempts { get; set; }
    public bool IsAdmin => Role == Roles.Administrator;
}

public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Clerk = "Clerk";
}

public class Customer
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public int Chest { get; set; }
    public int Waist { get; set; }
    public int Hip { get; set; }
    public int Shoulder { get; set; }
    public int Sleeve { get; set; }
    public int Length { get; set; }
    public int OrderCount { get; set; }
    public override string ToString() => $"{Name} ({Phone})";
}

public class Order
{
    public long Id { get; set; }
    public string OrderNumber { get; set; }
    public long CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerPhone { get; set; }
    public string Garment { get; set; }
    public string Description { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime DueDate { get; set; }
    public long Price { get; set; }
    public long RushFee { get; set; }
    public long Total { get; set; }
    public string Status { get; set; }
    public long CreatedBy { get; set; }
    public long Paid { get; set; }
    public long Balance => Total - Paid;
}

public class DailyIncome
{
    public string Date { get; set; }
    public long Amount { get; set; }
}
