using System;
using System.Windows.Forms;

namespace PDC
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            using var singleInstance = new SingleInstanceHelper("PDC_ProjectDirectoryCreator");

            if (!singleInstance.TryAcquire())
            {
                MessageBox.Show(
                    "Another instance of PDC is already running. Please close it first.",
                    "PDC Already Running",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
