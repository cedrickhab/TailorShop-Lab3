using TailorShop.Controllers;
using TailorShop.Models;

namespace TailorShop.Views;

public class CustomerDialog : Form
{
    private readonly CustomerController _ctrl;
    private readonly Customer _c;
    private readonly TextBox _name, _phone;
    private readonly (string Field, TextBox Box)[] _measures;

    public CustomerDialog(CustomerController ctrl, Customer c)
    {
        _ctrl = ctrl;
        _c = c;
        Ui.Setup(this, c.Id == 0 ? "Add customer" : "Edit customer", 360, 420, dialog: true);
        int y = 15;
        Ui.Label(this, "Name", 20, y); _name = Ui.Text(this, 140, y); y += 35;
        Ui.Label(this, "Phone", 20, y); _phone = Ui.Text(this, 140, y); y += 35;
        string[] fields = { "Chest", "Waist", "Hip", "Shoulder", "Sleeve", "Length" };
        _measures = new (string, TextBox)[fields.Length];
        for (int i = 0; i < fields.Length; i++)
        {
            Ui.Label(this, fields[i] + " (cm)", 20, y);
            _measures[i] = (fields[i], Ui.Text(this, 140, y, 80));
            y += 35;
        }
        var save = Ui.Button(this, "Save", 140, y + 10, (s, e) => Save(), 90);
        var cancel = Ui.Button(this, "Cancel", 240, y + 10, (s, e) => { DialogResult = DialogResult.Cancel; Close(); }, 90);
        AcceptButton = save;
        CancelButton = cancel;

        if (c.Id != 0)
        {
            _name.Text = c.Name;
            _phone.Text = c.Phone;
            int[] vals = { c.Chest, c.Waist, c.Hip, c.Shoulder, c.Sleeve, c.Length };
            for (int i = 0; i < vals.Length; i++) _measures[i].Box.Text = vals[i].ToString();
        }
    }

    private void Save()
    {
        var err = _ctrl.ValidateName(_name.Text);
        if (err != null) { Fail(_name, err); return; }
        err = _ctrl.ValidatePhone(_phone.Text, _c.Id);
        if (err != null) { Fail(_phone, err); return; }
        var values = new int[_measures.Length];
        for (int i = 0; i < _measures.Length; i++)
        {
            err = _ctrl.ValidateMeasurement(_measures[i].Field, _measures[i].Box.Text, out values[i]);
            if (err != null) { Fail(_measures[i].Box, err); return; }
        }
        var c = new Customer
        {
            Id = _c.Id, Name = _name.Text.Trim(), Phone = _phone.Text.Trim(),
            Chest = values[0], Waist = values[1], Hip = values[2],
            Shoulder = values[3], Sleeve = values[4], Length = values[5]
        };
        try
        {
            _ctrl.Save(c);
        }
        catch (Exception ex)
        {
            Ui.Error("The customer could not be saved: " + ex.Message);
            return;
        }
        DialogResult = DialogResult.OK;
        Close();
    }

    private static void Fail(Control box, string msg)
    {
        box.Focus();
        Ui.Error(msg);
        box.Focus();
    }
}
