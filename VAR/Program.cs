using System;
using System.IO;
using System.Windows.Forms;

namespace VAR
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                // Check if project.db exists in the current working directory
                // Use GetCurrentDirectory() instead of BaseDirectory so shortcuts work correctly
                string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "project.db");
                if (!File.Exists(dbPath))
                {
                    MessageBox.Show(
                        $"Project database not found.\n\nLooking for: {dbPath}\n\nVaristor must be run from a project's Variations folder that was created by PDC.\n\nIf you're testing, you need to:\n1. Run PDC first to create a project\n2. Then run Varistor from that project's Variations folder",
                        "Database Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                using var singleInstance = new SingleInstanceHelper("Varistor_VariationsManager");

                if (!singleInstance.TryAcquire())
                {
                    MessageBox.Show(
                        "Another instance of Varistor is already running. Please close it first.",
                        "Varistor Already Running",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new SummaryForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while starting Varistor:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Varistor Startup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
