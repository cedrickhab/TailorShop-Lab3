using TailorShop.Controllers;

namespace TailorShop.Views;

public class LoginForm : Form
{
    private readonly AuthController _auth = new();
    private readonly TextBox _user;
    private readonly TextBox _pass;
    private readonly Button _login;

    public LoginForm()
    {
        Ui.Setup(this, "Tailor Shop Kigali - Login", 340, 170, dialog: true);
        Ui.Label(this, "Username", 20, 25, 90);
        _user = Ui.Text(this, 110, 25);
        Ui.Label(this, "Password", 20, 65, 90);
        _pass = Ui.Text(this, 110, 65);
        _pass.UseSystemPasswordChar = true;
        _login = Ui.Button(this, "Login", 110, 110, (s, e) => DoLogin());
        AcceptButton = _login; // Enter in either box works like clicking Login
        _user.TextChanged += (s, e) => _login.Enabled = !_auth.IsLocked(_user.Text);
    }

    private void DoLogin()
    {
        var result = _auth.Login(_user.Text, _pass.Text);
        switch (result)
        {
            case LoginResult.Empty:
                Ui.Error("Username and password cannot be empty.");
                break;
            case LoginResult.Invalid:
                Ui.Error("Invalid username or password.");
                _pass.Clear();
                _pass.Focus();
                break;
            case LoginResult.Locked:
                Ui.Error("Too many failed attempts. Restart the application.");
                _pass.Clear();
                _login.Enabled = false;
                break;
            case LoginResult.Success:
                var main = new MainForm(_auth);
                main.FormClosed += (s, e) =>
                {
                    if (main.LoggedOut)
                    {
                        _user.Clear();
                        _pass.Clear();
                        Show();
                        _user.Focus();
                    }
                    else Close();
                };
                Hide();
                main.Show();
                break;
        }
    }
}
