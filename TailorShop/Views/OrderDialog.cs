using TailorShop.Controllers;
using TailorShop.Models;

namespace TailorShop.Views;

public class OrderDialog : Form
{
    private readonly OrderController _ctrl;
    private readonly User _user;
    private readonly ComboBox _customer, _garment;
    private readonly TextBox _desc, _price, _deposit;
    private readonly DateTimePicker _due;
    private readonly Label _rush, _total, _balance;

    public long SavedOrderId { get; private set; }

    public OrderDialog(OrderController ctrl, User user)
    {
        _ctrl = ctrl;
        _user = user;
        Ui.Setup(this, "New order", 420, 420, dialog: true);
        int y = 15;
        Ui.Label(this, "Customer", 20, y);
        _customer = new ComboBox { Left = 140, Top = y, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
        _customer.Items.AddRange(new CustomerController().GetAll().ToArray());
        if (_customer.Items.Count > 0) _customer.SelectedIndex = 0;
        Controls.Add(_customer); y += 35;

        Ui.Label(this, "Garment", 20, y);
        _garment = new ComboBox { Left = 140, Top = y, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
        _garment.Items.AddRange(BusinessRules.Garments);
        _garment.SelectedIndex = 0;
        Controls.Add(_garment); y += 35;

        Ui.Label(this, "Description", 20, y); _desc = Ui.Text(this, 140, y, 250); _desc.MaxLength = 100; y += 35;
        Ui.Label(this, "Price (RWF)", 20, y); _price = Ui.Text(this, 140, y, 150); y += 35;
        Ui.Label(this, "Deposit (RWF)", 20, y); _deposit = Ui.Text(this, 140, y, 150); _deposit.Text = "0"; y += 35;
        Ui.Label(this, "Due date", 20, y);
        _due = new DateTimePicker { Left = 140, Top = y, Width = 150, Format = DateTimePickerFormat.Short, MinDate = DateTime.Today.AddYears(-1), Value = DateTime.Today.AddDays(7) };
        Controls.Add(_due); y += 45;

        Ui.Label(this, "Rush fee", 20, y); _rush = Ui.Label(this, "0 RWF", 140, y, 200); y += 30;
        Ui.Label(this, "Total", 20, y); _total = Ui.Label(this, "0 RWF", 140, y, 200); y += 30;
        Ui.Label(this, "Balance", 20, y); _balance = Ui.Label(this, "0 RWF", 140, y, 200); y += 40;
        foreach (var l in new[] { _rush, _total, _balance }) l.Font = new Font(Font, FontStyle.Bold);

        var save = Ui.Button(this, "Save", 190, y, (s, e) => Save(), 95);
        var cancel = Ui.Button(this, "Cancel", 295, y, (s, e) => { DialogResult = DialogResult.Cancel; Close(); }, 95);
        AcceptButton = save;
        CancelButton = cancel;

        _price.TextChanged += (s, e) => Recalc();
        _deposit.TextChanged += (s, e) => Recalc();
        _due.ValueChanged += (s, e) => Recalc();
        Recalc();
    }

    private void Recalc()
    {
        long.TryParse(_price.Text.Trim().Replace(",", ""), out var price);
        long.TryParse(_deposit.Text.Trim().Replace(",", ""), out var deposit);
        if (price < 0) price = 0;
        var rush = BusinessRules.RushFee(price, DateTime.Now, _due.Value);
        var total = BusinessRules.Total(price, rush);
        _rush.Text = BusinessRules.Money(rush);
        _total.Text = BusinessRules.Money(total);
        _balance.Text = BusinessRules.Money(total - deposit);
    }

    private void Save()
    {
        if (_customer.SelectedItem is not Customer cu) { Fail(_customer, "Select a customer."); return; }
        var err = _ctrl.ValidateDescription(_desc.Text);
        if (err != null) { Fail(_desc, err); return; }
        err = _ctrl.ValidatePrice(_price.Text, out var price);
        if (err != null) { Fail(_price, err); return; }
        err = _ctrl.ValidateDueDate(_due.Value);
        if (err != null) { Fail(_due, err); return; }
        var total = BusinessRules.Total(price, BusinessRules.RushFee(price, DateTime.Now, _due.Value));
        err = _ctrl.ValidateDeposit(_deposit.Text, total, out var deposit);
        if (err != null) { Fail(_deposit, err); return; }

        var o = new Order
        {
            CustomerId = cu.Id, Garment = (string)_garment.SelectedItem, Description = _desc.Text.Trim(),
            DueDate = _due.Value.Date, Price = price
        };
        try
        {
            SavedOrderId = _ctrl.Create(o, deposit, _user);
        }
        catch (Exception ex)
        {
            Ui.Error("The order could not be saved: " + ex.Message);
            return;
        }
        DialogResult = DialogResult.OK;
        Close();
    }

    private static void Fail(Control c, string msg)
    {
        Ui.Error(msg);
        c.Focus();
    }
}
