using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using TailorShop.Models;

namespace TailorShop.Data;

public static class Database
{
    public const string DateFormat = "yyyy-MM-dd";
    public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    public static string DbPath => Path.Combine(AppContext.BaseDirectory, "shop.db");

    private static string ConnectionString =>
        new SqliteConnectionStringBuilder { DataSource = DbPath, ForeignKeys = true }.ToString();

    public static SqliteConnection Open()
    {
        var c = new SqliteConnection(ConnectionString);
        c.Open();
        return c;
    }

    public static string Hash(string password) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

    public static void Initialize()
    {
        using var c = Open();
        Exec(c, @"
CREATE TABLE IF NOT EXISTS Users (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Username TEXT NOT NULL UNIQUE COLLATE NOCASE,
  PasswordHash TEXT NOT NULL,
  Role TEXT NOT NULL CHECK (Role IN ('Administrator','Clerk')),
  FailedAttempts INTEGER NOT NULL DEFAULT 0);
CREATE TABLE IF NOT EXISTS Customers (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Name TEXT NOT NULL,
  Phone TEXT NOT NULL UNIQUE,
  Chest INTEGER NOT NULL, Waist INTEGER NOT NULL, Hip INTEGER NOT NULL,
  Shoulder INTEGER NOT NULL, Sleeve INTEGER NOT NULL, Length INTEGER NOT NULL);
CREATE TABLE IF NOT EXISTS Orders (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  OrderNumber TEXT NOT NULL UNIQUE,
  CustomerId INTEGER NOT NULL REFERENCES Customers(Id),
  Garment TEXT NOT NULL,
  Description TEXT,
  OrderDate TEXT NOT NULL,
  DueDate TEXT NOT NULL,
  Price INTEGER NOT NULL,
  RushFee INTEGER NOT NULL,
  Total INTEGER NOT NULL,
  Status TEXT NOT NULL,
  CreatedBy INTEGER NOT NULL REFERENCES Users(Id));
CREATE TABLE IF NOT EXISTS Payments (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  OrderId INTEGER NOT NULL REFERENCES Orders(Id) ON DELETE CASCADE,
  Amount INTEGER NOT NULL,
  PaymentDate TEXT NOT NULL);
CREATE INDEX IF NOT EXISTS IX_Orders_Customer ON Orders(CustomerId);
CREATE INDEX IF NOT EXISTS IX_Payments_Order ON Payments(OrderId);
UPDATE Users SET FailedAttempts = 0;");

        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Users";
        if ((long)cmd.ExecuteScalar() == 0)
        {
            AddUser(c, "admin", "admin123", Roles.Administrator);
            AddUser(c, "clerk", "clerk123", Roles.Clerk);
        }
    }

    private static void AddUser(SqliteConnection c, string u, string p, string role)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO Users (Username, PasswordHash, Role) VALUES ($u, $h, $r)";
        cmd.Parameters.AddWithValue("$u", u);
        cmd.Parameters.AddWithValue("$h", Hash(p));
        cmd.Parameters.AddWithValue("$r", role);
        cmd.ExecuteNonQuery();
    }

    private static void Exec(SqliteConnection c, string sql)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
