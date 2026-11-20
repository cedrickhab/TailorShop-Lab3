using TailorShop.Data;
using TailorShop.Views;

namespace TailorShop;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (s, e) =>
            MessageBox.Show("An error occurred: " + e.Exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        try
        {
            Database.Initialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not open the database: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        Application.Run(new LoginForm());
    }
}
