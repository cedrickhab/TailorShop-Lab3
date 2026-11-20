using TailorShop.Controllers;
using TailorShop.Models;

namespace TailorShop.Views;

public class OrdersForm : Form
{
    private readonly OrderController _ctrl = new();
    private readonly User _user;
    private readonly ComboBox _filter;
    private readonly DataGridView _grid;

    public OrdersForm(User user)
    {
        _user = user;
        Ui.Setup(this, "Orders", 900, 500);
        Ui.Label(this, "Status", 15, 15, 60);
        _filter = new ComboBox { Left = 80, Top = 15, Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
        _filter.Items.Add("All");
        _filter.Items.AddRange(BusinessRules.Statuses);
        _filter.SelectedIndex = 0;
        _filter.SelectedIndexChanged += (s, e) => LoadData();
        Controls.Add(_filter);

        _grid = Ui.Grid(this, 15, 50, 870, 390);
        Ui.Column(_grid, "Order number", nameof(Order.OrderNumber));
        Ui.Column(_grid, "Customer", nameof(Order.CustomerName));
        Ui.Column(_grid, "Garment", nameof(Order.Garment));
        Ui.Column(_grid, "Due date", nameof(Order.DueDate), "yyyy-MM-dd");
        Ui.Column(_grid, "Status", nameof(Order.Status));
        Ui.Column(_grid, "Total", nameof(Order.Total), "N0");
        Ui.Column(_grid, "Balance", nameof(Order.Balance), "N0");

        int y = 455;
        var bottom = AnchorStyles.Bottom | AnchorStyles.Left;
        Ui.Button(this, "New order", 15, y, (s, e) => NewOrder(), 120).Anchor = bottom;
        Ui.Button(this, "Update status", 145, y, (s, e) => UpdateStatus(), 120).Anchor = bottom;
        Ui.Button(this, "Record payment", 275, y, (s, e) => RecordPayment(), 130).Anchor = bottom;
        var del = Ui.Button(this, "Delete", 415, y, (s, e) => Delete());
        del.Anchor = bottom;
        del.Visible = _user.IsAdmin;
        Ui.Button(this, "Back", 775, y, (s, e) => Close()).Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        Load += (s, e) => LoadData();
    }

    private void LoadData() => _grid.DataSource = _ctrl.GetAll(_filter.SelectedItem as string);

    private Order Selected => _grid.CurrentRow?.DataBoundItem as Order;

    private void NewOrder()
    {
        if (new CustomerController().GetAll().Count == 0) { Ui.Error("Add a customer first."); return; }
        using var d = new OrderDialog(_ctrl, _user);
        if (d.ShowDialog(this) == DialogResult.OK)
        {
            LoadData();
            using var slip = new OrderSlipForm(_ctrl.Get(d.SavedOrderId));
            slip.ShowDialog(this);
        }
    }

    private void UpdateStatus()
    {
        var o = Selected;
        if (o == null) { Ui.Error("Select an order first."); return; }
        if (o.Status == "Collected") { Ui.Error("This order is already Collected."); return; }
        var target = Prompt.Choose("Update status", $"{o.OrderNumber} is {o.Status}. New status:",
            BusinessRules.Statuses.Where(s => s != o.Status).ToArray(), BusinessRules.NextStatus(o.Status));
        if (target == null) return;
        var err = _ctrl.AdvanceStatus(o.Id, target);
        if (err != null) Ui.Error(err);
        LoadData();
    }

    private void RecordPayment()
    {
        var o = Selected;
        if (o == null) { Ui.Error("Select an order first."); return; }
        if (o.Balance <= 0) { Ui.Error("This order has no balance to pay."); return; }
        var text = Prompt.Input("Record payment", $"{o.OrderNumber} balance: {o.Balance:N0} RWF. Amount:");
        if (text == null) return;
        var err = _ctrl.RecordPayment(o.Id, text);
        if (err != null) Ui.Error(err);
        else Ui.Info("Payment saved.");
        LoadData();
    }

    private void Delete()
    {
        var o = Selected;
        if (o == null) { Ui.Error("Select an order first."); return; }
        if (!Ui.Confirm("Delete this order?")) return;
        var err = _ctrl.Delete(_user, o.Id);
        if (err != null) Ui.Error(err);
        LoadData();
    }
}

/// <summary>Simple modal prompts used by the Orders screen.</summary>
internal static class Prompt
{
    public static string Input(string title, string label)
    {
        using var f = new Form();
        Ui.Setup(f, title, 380, 130, dialog: true);
        Ui.Label(f, label, 15, 15, 350);
        var t = Ui.Text(f, 15, 45, 350);
        var ok = Ui.Button(f, "OK", 165, 85, (s, e) => { f.DialogResult = DialogResult.OK; }, 95);
        var cancel = Ui.Button(f, "Cancel", 270, 85, (s, e) => { f.DialogResult = DialogResult.Cancel; }, 95);
        f.AcceptButton = ok;
        f.CancelButton = cancel;
        return f.ShowDialog() == DialogResult.OK ? t.Text : null;
    }

    public static string Choose(string title, string label, string[] options, string selected)
    {
        using var f = new Form();
        Ui.Setup(f, title, 380, 130, dialog: true);
        Ui.Label(f, label, 15, 15, 350);
        var cb = new ComboBox { Left = 15, Top = 45, Width = 350, DropDownStyle = ComboBoxStyle.DropDownList };
        cb.Items.AddRange(options);
        cb.SelectedItem = selected ?? options.FirstOrDefault();
        f.Controls.Add(cb);
        var ok = Ui.Button(f, "OK", 165, 85, (s, e) => { f.DialogResult = DialogResult.OK; }, 95);
        var cancel = Ui.Button(f, "Cancel", 270, 85, (s, e) => { f.DialogResult = DialogResult.Cancel; }, 95);
        f.AcceptButton = ok;
        f.CancelButton = cancel;
        return f.ShowDialog() == DialogResult.OK ? cb.SelectedItem as string : null;
    }
}
