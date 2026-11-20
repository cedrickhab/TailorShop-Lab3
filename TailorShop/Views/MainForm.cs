using TailorShop.Controllers;

namespace TailorShop.Views;

public class MainForm : Form
{
    private readonly AuthController _auth;
    public bool LoggedOut { get; private set; }

    public MainForm(AuthController auth)
    {
        _auth = auth;
        var u = auth.CurrentUser;
        Ui.Setup(this, $"Tailor Shop Kigali - {u.Username} ({u.Role})", 300, u.IsAdmin ? 250 : 200, dialog: true);
        int y = 20;
        Ui.Button(this, "Customers", 50, y, (s, e) => OpenChild(new CustomersForm(_auth.CurrentUser)), 200); y += 45;
        Ui.Button(this, "Orders", 50, y, (s, e) => OpenChild(new OrdersForm(_auth.CurrentUser)), 200); y += 45;
        if (u.IsAdmin) { Ui.Button(this, "Reports", 50, y, (s, e) => OpenChild(new ReportsForm()), 200); y += 45; }
        Ui.Button(this, "Logout", 50, y + 10, (s, e) => { LoggedOut = true; _auth.Logout(); Close(); }, 200);
    }

    private void OpenChild(Form f)
    {
        Hide();
        f.ShowDialog();
        f.Dispose();
        Show();
    }
}
