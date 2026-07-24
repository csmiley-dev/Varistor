using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PDC
{
    public class MainForm : Form
    {
        private TextBox txtProjectNumber;
        private TextBox txtProjectName;
        private ComboBox cboClientName;
        private TextBox txtCustomClient;
        private Label lblCustomClient;
        private Button btnCreate;
        private Button btnCancel;
        private CheckBox chkCustomClient;

        private string clientsDbPath;
        private string zzFolderPath;

        public MainForm()
        {
            InitializeComponent();
            InitializePaths();
            LoadClients();
        }

        private void InitializeComponent()
        {
            this.Text = "Project Directory Creator (PDC)";
            this.Size = new Size(500, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            int labelWidth = 120;
            int controlWidth = 300;
            int leftMargin = 30;
            int topMargin = 20;
            int rowHeight = 35;

            // Project Number
            Label lblProjectNumber = new Label
            {
                Text = "Project Number:",
                Location = new Point(leftMargin, topMargin),
                Size = new Size(labelWidth, 20)
            };
            txtProjectNumber = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, topMargin),
                Size = new Size(controlWidth, 20)
            };

            // Project Name
            Label lblProjectName = new Label
            {
                Text = "Project Name:",
                Location = new Point(leftMargin, topMargin + rowHeight),
                Size = new Size(labelWidth, 20)
            };
            txtProjectName = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, topMargin + rowHeight),
                Size = new Size(controlWidth, 20)
            };

            // Client Name
            Label lblClientName = new Label
            {
                Text = "Client Name:",
                Location = new Point(leftMargin, topMargin + rowHeight * 2),
                Size = new Size(labelWidth, 20)
            };
            cboClientName = new ComboBox
            {
                Location = new Point(leftMargin + labelWidth, topMargin + rowHeight * 2),
                Size = new Size(controlWidth, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Custom Client Checkbox
            chkCustomClient = new CheckBox
            {
                Text = "Enter custom client name",
                Location = new Point(leftMargin + labelWidth, topMargin + rowHeight * 3),
                Size = new Size(controlWidth, 20)
            };
            chkCustomClient.CheckedChanged += ChkCustomClient_CheckedChanged;

            // Custom Client Name
            lblCustomClient = new Label
            {
                Text = "Custom Client:",
                Location = new Point(leftMargin, topMargin + rowHeight * 4),
                Size = new Size(labelWidth, 20),
                Visible = false
            };
            txtCustomClient = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, topMargin + rowHeight * 4),
                Size = new Size(controlWidth, 20),
                Visible = false
            };

            // Buttons
            btnCreate = new Button
            {
                Text = "Create Project",
                Location = new Point(leftMargin + labelWidth, topMargin + rowHeight * 5 + 10),
                Size = new Size(120, 30)
            };
            btnCreate.Click += BtnCreate_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(leftMargin + labelWidth + 130, topMargin + rowHeight * 5 + 10),
                Size = new Size(120, 30)
            };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.Add(lblProjectNumber);
            this.Controls.Add(txtProjectNumber);
            this.Controls.Add(lblProjectName);
            this.Controls.Add(txtProjectName);
            this.Controls.Add(lblClientName);
            this.Controls.Add(cboClientName);
            this.Controls.Add(chkCustomClient);
            this.Controls.Add(lblCustomClient);
            this.Controls.Add(txtCustomClient);
            this.Controls.Add(btnCreate);
            this.Controls.Add(btnCancel);
        }

        private void InitializePaths()
        {
            // PDC runs from X:\BMS\Programs\PDC\
            // clients.db is stored there
            string pdcFolder = @"X:\BMS\Programs\PDC";
            zzFolderPath = pdcFolder;
            clientsDbPath = Path.Combine(pdcFolder, "clients.db");

            // Create clients database if it doesn't exist
            DatabaseHelper.CreateClientsDatabase(clientsDbPath);
        }

        private void LoadClients()
        {
            var clients = DatabaseHelper.GetClients(clientsDbPath);
            cboClientName.Items.Clear();

            foreach (var client in clients)
            {
                cboClientName.Items.Add(client);
            }

            if (cboClientName.Items.Count > 0)
            {
                cboClientName.SelectedIndex = 0;
            }
        }

        private void ChkCustomClient_CheckedChanged(object? sender, EventArgs e)
        {
            bool isCustom = chkCustomClient.Checked;
            lblCustomClient.Visible = isCustom;
            txtCustomClient.Visible = isCustom;
            cboClientName.Enabled = !isCustom;
        }

        private void BtnCreate_Click(object? sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtProjectNumber.Text))
            {
                MessageBox.Show("Please enter a project number.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtProjectName.Text))
            {
                MessageBox.Show("Please enter a project name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string clientName;
            if (chkCustomClient.Checked)
            {
                if (string.IsNullOrWhiteSpace(txtCustomClient.Text))
                {
                    MessageBox.Show("Please enter a custom client name.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                clientName = txtCustomClient.Text.Trim();
            }
            else
            {
                if (cboClientName.SelectedItem == null)
                {
                    MessageBox.Show("Please select a client name.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                clientName = cboClientName.SelectedItem.ToString()!;
            }

            string projectNumber = txtProjectNumber.Text.Trim();
            string projectName = txtProjectName.Text.Trim();

            try
            {
                CreateProjectStructure(projectName, projectNumber, clientName);
                MessageBox.Show($"Project '{projectName} {projectNumber}' created successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating project: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateProjectStructure(string projectName, string projectNumber, string clientName)
        {
            // Projects are created in X:\Projects\
            string projectsFolder = @"X:\Projects";
            string projectFolderName = $"{projectName} {projectNumber}";
            string projectPath = Path.Combine(projectsFolder, projectFolderName);

            // Check if project folder already exists
            if (Directory.Exists(projectPath))
            {
                throw new Exception($"Project folder '{projectFolderName}' already exists!");
            }

            // Create main project folder
            Directory.CreateDirectory(projectPath);

            // Create subdirectory structure
            var dirPaths = new List<string>
            {
                "Tender/Superseded",
                "For Approval/Superseded",
                "Construction/Architectural/Superseded",
                "Construction/Electrical/Superseded",
                "Construction/Hydraulic/Superseded",
                $"Construction/Mechanical--{clientName}/Superseded",
                "Construction/Mechanical--Consultant/Superseded",
                $"Tech Data/{clientName} Tech Data/Superseded",
                "As Installed",
                "Site Pics",
                "BMS/Superseded",
                "Site Start",
                "Quotes and POs/Superseded",
                "Switchboard Photos",
                "Variations",
                "Commissioning/Superseded",
                "Finance"
            };

            foreach (var dirPath in dirPaths)
            {
                string fullPath = Path.Combine(projectPath, dirPath);
                Directory.CreateDirectory(fullPath);
            }

            // Create shortcut to VAR instead of copying all files
            string variationsFolder = Path.Combine(projectPath, "Variations");
            string varSourceFolder = @"X:\BMS\Programs\VAR";
            string varExePath = Path.Combine(varSourceFolder, "VAR.exe");

            if (File.Exists(varExePath))
            {
                // Create shortcut to centralized VAR
                CreateShortcut(
                    Path.Combine(variationsFolder, "VAR.lnk"),
                    varExePath,
                    variationsFolder  // Set working directory to Variations folder so it finds project.db
                );
            }
            else
            {
                // Fallback: Copy files if centralized installation doesn't exist
                string legacyVarFolder = Path.Combine(zzFolderPath, "VAR");
                if (Directory.Exists(legacyVarFolder))
                {
                    CopyAllFiles(legacyVarFolder, variationsFolder);
                }
            }

            // Create and seed project database
            string projectDbPath = Path.Combine(variationsFolder, "project.db");
            DatabaseHelper.CreateProjectDatabase(projectDbPath, projectName, projectNumber, clientName);

            // Copy client contacts to project database
            CopyClientContacts(clientName, projectDbPath);
        }

        private void CreateShortcut(string shortcutPath, string targetPath, string workingDirectory)
        {
            // Use PowerShell to create shortcut (works without additional COM references)
            string psCommand = $@"
                $WshShell = New-Object -comObject WScript.Shell;
                $Shortcut = $WshShell.CreateShortcut('{shortcutPath}');
                $Shortcut.TargetPath = '{targetPath}';
                $Shortcut.WorkingDirectory = '{workingDirectory}';
                $Shortcut.Save()
            ";

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            var process = System.Diagnostics.Process.Start(psi);
            process.WaitForExit();
        }

        private void CopyAllFiles(string sourceFolder, string destFolder)
        {
            // Copy all files
            foreach (string file in Directory.GetFiles(sourceFolder))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destFolder, fileName);
                File.Copy(file, destFile, true);
            }

            // Copy all subdirectories
            foreach (string dir in Directory.GetDirectories(sourceFolder))
            {
                string dirName = Path.GetFileName(dir);
                string destDir = Path.Combine(destFolder, dirName);
                CopyDirectory(dir, destDir);
            }
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            // Copy files
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);
                File.Copy(file, destFile, true);
            }

            // Copy subdirectories recursively
            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(dir);
                string destSubDir = Path.Combine(destDir, dirName);
                CopyDirectory(dir, destSubDir);
            }
        }

        private void CopyClientContacts(string clientName, string projectDbPath)
        {
            var contacts = DatabaseHelper.GetClientContacts(clientsDbPath, clientName);

            using var connection = new System.Data.SQLite.SQLiteConnection($"Data Source={projectDbPath};Version=3;");
            connection.Open();

            string createContactsTable = @"
                CREATE TABLE IF NOT EXISTS ClientContacts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ContactName TEXT NOT NULL UNIQUE
                );";

            using (var command = new System.Data.SQLite.SQLiteCommand(createContactsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            foreach (var contact in contacts)
            {
                string insertContact = "INSERT OR IGNORE INTO ClientContacts (ContactName) VALUES (@contact)";
                using var command = new System.Data.SQLite.SQLiteCommand(insertContact, connection);
                command.Parameters.AddWithValue("@contact", contact);
                command.ExecuteNonQuery();
            }
        }
    }
}
