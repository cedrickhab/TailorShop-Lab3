namespace TailorShop.Views;

/// <summary>Small helpers for building forms in code.</summary>
internal static class Ui
{
    public static Label Label(Control parent, string text, int x, int y, int w = 120)
    {
        var l = new Label { Text = text, Left = x, Top = y + 3, Width = w, AutoSize = false };
        parent.Controls.Add(l);
        return l;
    }

    public static TextBox Text(Control parent, int x, int y, int w = 200)
    {
        var t = new TextBox { Left = x, Top = y, Width = w };
        parent.Controls.Add(t);
        return t;
    }

    public static Button Button(Control parent, string text, int x, int y, EventHandler onClick, int w = 110)
    {
        var b = new Button { Text = text, Left = x, Top = y, Width = w, Height = 30 };
        b.Click += onClick;
        parent.Controls.Add(b);
        return b;
    }

    public static DataGridView Grid(Control parent, int x, int y, int w, int h)
    {
        var g = new DataGridView
        {
            Left = x, Top = y, Width = w, Height = h,
            ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false,
            AutoGenerateColumns = false, BackgroundColor = SystemColors.Window,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };
        parent.Controls.Add(g);
        return g;
    }

    public static void Column(DataGridView g, string header, string property, string format = null)
    {
        var c = new DataGridViewTextBoxColumn { HeaderText = header, DataPropertyName = property };
        if (format != null) c.DefaultCellStyle.Format = format;
        g.Columns.Add(c);
    }

    public static void Error(string msg) => MessageBox.Show(msg, "Tailor Shop", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public static void Info(string msg) => MessageBox.Show(msg, "Tailor Shop", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public static bool Confirm(string msg) =>
        MessageBox.Show(msg, "Tailor Shop", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    public static void Setup(Form f, string title, int w, int h, bool dialog = false)
    {
        f.Text = title;
        f.ClientSize = new Size(w, h);
        f.StartPosition = FormStartPosition.CenterScreen;
        f.Font = new Font("Segoe UI", 9.5f);
        if (dialog)
        {
            f.FormBorderStyle = FormBorderStyle.FixedDialog;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
        }
    }
}
