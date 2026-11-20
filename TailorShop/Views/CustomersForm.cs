using TailorShop.Controllers;
using TailorShop.Models;

namespace TailorShop.Views;

public class CustomersForm : Form
{
    private readonly CustomerController _ctrl = new();
    private readonly User _user;
    private readonly TextBox _search;
    private readonly DataGridView _grid;

    public CustomersForm(User user)
    {
        _user = user;
        Ui.Setup(this, "Customers", 640, 460);
        Ui.Label(this, "Search", 15, 15, 60);
        _search = Ui.Text(this, 80, 15, 300);
        _search.TextChanged += (s, e) => LoadData();
        _grid = Ui.Grid(this, 15, 50, 610, 350);
        Ui.Column(_grid, "Name", nameof(Customer.Name));
        Ui.Column(_grid, "Phone", nameof(Customer.Phone));
        Ui.Column(_grid, "Number of orders", nameof(Customer.OrderCount));
        _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Edit(); };

        int y = 415;
        var bottom = AnchorStyles.Bottom | AnchorStyles.Left;
        Ui.Button(this, "Add", 15, y, (s, e) => Add()).Anchor = bottom;
        Ui.Button(this, "Edit", 135, y, (s, e) => Edit()).Anchor = bottom;
        var del = Ui.Button(this, "Delete", 255, y, (s, e) => Delete());
        del.Anchor = bottom;
        del.Visible = _user.IsAdmin;
        var back = Ui.Button(this, "Back", 515, y, (s, e) => Close());
        back.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        Load += (s, e) => LoadData();
    }

    private void LoadData() => _grid.DataSource = _ctrl.GetAll(_search.Text);

    private Customer Selected => _grid.CurrentRow?.DataBoundItem as Customer;

    private void Add()
    {
        using var d = new CustomerDialog(_ctrl, new Customer());
        if (d.ShowDialog(this) == DialogResult.OK) LoadData();
    }

    private void Edit()
    {
        if (Selected == null) { Ui.Error("Select a customer first."); return; }
        using var d = new CustomerDialog(_ctrl, Selected);
        if (d.ShowDialog(this) == DialogResult.OK) LoadData();
    }

    private void Delete()
    {
        if (Selected == null) { Ui.Error("Select a customer first."); return; }
        if (!Ui.Confirm("Delete this customer?")) return;
        var err = _ctrl.Delete(_user, Selected.Id);
        if (err != null) Ui.Error(err);
        LoadData();
    }
}
