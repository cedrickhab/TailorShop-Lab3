# Tailor Shop Order and Measurement Tracker

Habimana Cedrick - 27443 - C# (.NET) Lab 3

Windows Forms desktop application (C#, .NET) with a local SQLite database (`shop.db`, created automatically next to the executable on first run).

## Architecture (MVC)

- `TailorShop/Models` - entities (User, Customer, Order, DailyIncome) and `BusinessRules` (rush fee, total, status flow, overdue).
- `TailorShop/Data` - `Database`: SQLite connection, schema creation, seed accounts, SHA-256 password hashing.
- `TailorShop/Controllers` - `AuthController`, `CustomerController`, `OrderController`, `ReportController`: validation and database access.
- `TailorShop/Views` - Windows Forms: Login, Main menu, Customers, Customer dialog, Orders, Order dialog, Order slip, Reports.

## Run

Open `TailorShop.sln` in Visual Studio and press F5, or run `dotnet run --project TailorShop`.

Default accounts (created on first run):

| Username | Password | Role |
|----------|----------|------|
| admin | admin123 | Administrator |
| clerk | clerk123 | Clerk |

## Features

- Login with lockout after 3 failed attempts per username (until restart).
- Customers with six measurements: add, edit, delete (admin), live search.
- Orders: rush fee (20% when due in 3 days or fewer), deposit saved in the same transaction, status flow Received > Cutting > Sewing > Ready > Collected, payments, delete (admin).
- Order slip after saving.
- Reports (admin): overdue orders, unpaid balances with total owed, income per day for a date range.
