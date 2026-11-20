using TailorShop.Controllers;
using TailorShop.Models;

namespace TailorShop.Views;

public class ReportsForm : Form
{
    private readonly ReportController _ctrl = new();
    private readonly DataGridView _overdue, _unpaid, _income;
    private readonly Label _overdueMsg, _unpaidMsg, _incomeMsg;
    private readonly DateTimePicker _start, _end;

    public ReportsForm()
    {
        Ui.Setup(this, "Reports", 820, 520);
        var tabs = new TabControl { Left = 10, Top = 10, Width = 800, Height = 455, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
        Controls.Add(tabs);

        var t1 = new TabPage("Overdue orders");
        _overdueMsg = Ui.Label(t1, "", 10, 10, 700);
        _overdue = Ui.Grid(t1, 10, 40, 770, 375);
        Ui.Column(_overdue, "Order number", nameof(Order.OrderNumber));
        Ui.Column(_overdue, "Customer", nameof(Order.CustomerName));
        Ui.Column(_overdue, "Phone", nameof(Order.CustomerPhone));
        Ui.Column(_overdue, "Due date", nameof(Order.DueDate), "yyyy-MM-dd");
        Ui.Column(_overdue, "Status", nameof(Order.Status));
        tabs.TabPages.Add(t1);

        var t2 = new TabPage("Unpaid balances");
        _unpaidMsg = Ui.Label(t2, "", 10, 10, 700);
        _unpaid = Ui.Grid(t2, 10, 40, 770, 375);
        Ui.Column(_unpaid, "Order number", nameof(Order.OrderNumber));
        Ui.Column(_unpaid, "Customer", nameof(Order.CustomerName));
        Ui.Column(_unpaid, "Total", nameof(Order.Total), "N0");
        Ui.Column(_unpaid, "Balance", nameof(Order.Balance), "N0");
        tabs.TabPages.Add(t2);

        var t3 = new TabPage("Income");
        Ui.Label(t3, "Start date", 10, 10, 70);
        _start = new DateTimePicker { Left = 85, Top = 10, Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
        Ui.Label(t3, "End date", 230, 10, 65);
        _end = new DateTimePicker { Left = 300, Top = 10, Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
        t3.Controls.Add(_start);
        t3.Controls.Add(_end);
        Ui.Button(t3, "Show", 445, 8, (s, e) => ShowIncome(), 90);
        _incomeMsg = Ui.Label(t3, "", 10, 45, 700);
        _incomeMsg.Font = new Font(Font, FontStyle.Bold);
        _income = Ui.Grid(t3, 10, 75, 770, 340);
        Ui.Column(_income, "Date", nameof(DailyIncome.Date));
        Ui.Column(_income, "Amount", nameof(DailyIncome.Amount), "N0");
        tabs.TabPages.Add(t3);

        Ui.Button(this, "Back", 700, 478, (s, e) => Close()).Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        Load += (s, e) => LoadReports();
    }

    private void LoadReports()
    {
        var overdue = _ctrl.Overdue();
        _overdue.DataSource = overdue;
        _overdueMsg.Text = overdue.Count == 0 ? "No overdue orders." : $"{overdue.Count} overdue order(s).";

        var unpaid = _ctrl.Unpaid();
        _unpaid.DataSource = unpaid;
        _unpaidMsg.Text = unpaid.Count == 0
            ? "No unpaid balances."
            : "Total owed: " + BusinessRules.Money(unpaid.Sum(o => o.Balance));
        _unpaidMsg.Font = new Font(Font, FontStyle.Bold);
    }

    private void ShowIncome()
    {
        if (_start.Value.Date > _end.Value.Date)
        {
            _income.DataSource = null;
            _incomeMsg.Text = "";
            Ui.Error("The start date must be before or on the end date.");
            return;
        }
        var rows = _ctrl.Income(_start.Value.Date, _end.Value.Date);
        _income.DataSource = rows;
        _incomeMsg.Text = rows.Count == 0
            ? "No payments in this range."
            : "Total income: " + BusinessRules.Money(rows.Sum(r => r.Amount));
    }
}
