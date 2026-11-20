using TailorShop.Models;

namespace TailorShop.Views;

public class OrderSlipForm : Form
{
    public OrderSlipForm(Order o)
    {
        Ui.Setup(this, "Order slip " + o.OrderNumber, 360, 450, dialog: true);
        var title = Ui.Label(this, BusinessRules.ShopName, 20, 15, 320);
        title.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
        title.Height = 32;
        title.TextAlign = ContentAlignment.MiddleCenter;

        var rows = new (string, string)[]
        {
            ("Order number", o.OrderNumber),
            ("Order date", o.OrderDate.ToString("yyyy-MM-dd HH:mm")),
            ("Customer", o.CustomerName),
            ("Garment", o.Garment),
            ("Due date", o.DueDate.ToString("yyyy-MM-dd")),
            ("Price", BusinessRules.Money(o.Price)),
            ("Rush fee", BusinessRules.Money(o.RushFee)),
            ("Total", BusinessRules.Money(o.Total)),
            ("Deposit paid", BusinessRules.Money(o.Paid)),
            ("Balance", BusinessRules.Money(o.Balance)),
        };
        int y = 60;
        foreach (var (k, v) in rows)
        {
            Ui.Label(this, k, 30, y, 120);
            Ui.Label(this, v, 160, y, 180).Font = new Font(Font, FontStyle.Bold);
            y += 28;
        }
        var thanks = Ui.Label(this, "Thank you.", 20, y + 5, 320);
        thanks.TextAlign = ContentAlignment.MiddleCenter;
        var close = Ui.Button(this, "Close", 125, y + 40, (s, e) => Close());
        AcceptButton = close;
        CancelButton = close;
    }
}
