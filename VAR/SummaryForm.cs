using System;
using System.Drawing;
using System.Windows.Forms;

namespace VAR
{
    public class SummaryForm : Form
    {
        private DatabaseHelper _dbHelper;
        private ProjectInfo _projectInfo;

        private Label lblProjectInfo;
        private Label lblDate;
        private DataGridView dgvVariations;
        private GroupBox grpAllVariations;
        private GroupBox grpApprovedVariations;
        private Label lblTotalAdditions;
        private Label lblTotalCredits;
        private Label lblNetValue;
        private Label lblApprovedAdditions;
        private Label lblApprovedCredits;
        private Label lblApprovedNetValue;
        private Button btnNewVariation;
        private Button btnEditVariation;
        private Button btnDeleteVariation;
        private Button btnRefresh;

        public SummaryForm()
        {
            // Use current working directory so shortcuts work correctly
            string dbPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "project.db");
            _dbHelper = new DatabaseHelper(dbPath);
            _projectInfo = _dbHelper.GetProjectInfo();

            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Varistor - Variations Manager";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Project Info
            lblProjectInfo = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(800, 25),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Text = $"Project: {_projectInfo.ProjectName} {_projectInfo.ProjectNumber} - Client: {_projectInfo.ClientName}"
            };

            lblDate = new Label
            {
                Location = new Point(850, 20),
                Size = new Size(300, 25),
                Font = new Font("Arial", 10),
                Text = $"Date: {DateTime.Now:yyyy-MM-dd}",
                TextAlign = ContentAlignment.MiddleRight
            };

            // DataGridView for variations
            dgvVariations = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(1150, 350),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvVariations.CellContentClick += DgvVariations_CellContentClick;
            dgvVariations.CellDoubleClick += DgvVariations_CellDoubleClick;

            // Buttons
            btnNewVariation = new Button
            {
                Location = new Point(20, 420),
                Size = new Size(150, 35),
                Text = "New Variation"
            };
            btnNewVariation.Click += BtnNewVariation_Click;

            btnEditVariation = new Button
            {
                Location = new Point(180, 420),
                Size = new Size(150, 35),
                Text = "Edit Variation"
            };
            btnEditVariation.Click += BtnEditVariation_Click;

            btnDeleteVariation = new Button
            {
                Location = new Point(340, 420),
                Size = new Size(150, 35),
                Text = "Delete Variation"
            };
            btnDeleteVariation.Click += BtnDeleteVariation_Click;

            btnRefresh = new Button
            {
                Location = new Point(500, 420),
                Size = new Size(150, 35),
                Text = "Refresh"
            };
            btnRefresh.Click += (s, e) => LoadData();

            // Summary boxes
            grpAllVariations = new GroupBox
            {
                Location = new Point(20, 470),
                Size = new Size(550, 150),
                Text = "All Variations Summary"
            };

            lblTotalAdditions = new Label
            {
                Location = new Point(20, 30),
                Size = new Size(500, 25),
                Font = new Font("Arial", 10)
            };

            lblTotalCredits = new Label
            {
                Location = new Point(20, 60),
                Size = new Size(500, 25),
                Font = new Font("Arial", 10)
            };

            lblNetValue = new Label
            {
                Location = new Point(20, 90),
                Size = new Size(500, 25),
                Font = new Font("Arial", 11, FontStyle.Bold)
            };

            grpAllVariations.Controls.Add(lblTotalAdditions);
            grpAllVariations.Controls.Add(lblTotalCredits);
            grpAllVariations.Controls.Add(lblNetValue);

            grpApprovedVariations = new GroupBox
            {
                Location = new Point(600, 470),
                Size = new Size(570, 150),
                Text = "Approved Variations Summary"
            };

            lblApprovedAdditions = new Label
            {
                Location = new Point(20, 30),
                Size = new Size(500, 25),
                Font = new Font("Arial", 10)
            };

            lblApprovedCredits = new Label
            {
                Location = new Point(20, 60),
                Size = new Size(500, 25),
                Font = new Font("Arial", 10)
            };

            lblApprovedNetValue = new Label
            {
                Location = new Point(20, 90),
                Size = new Size(500, 25),
                Font = new Font("Arial", 11, FontStyle.Bold)
            };

            grpApprovedVariations.Controls.Add(lblApprovedAdditions);
            grpApprovedVariations.Controls.Add(lblApprovedCredits);
            grpApprovedVariations.Controls.Add(lblApprovedNetValue);

            this.Controls.Add(lblProjectInfo);
            this.Controls.Add(lblDate);
            this.Controls.Add(dgvVariations);
            this.Controls.Add(btnNewVariation);
            this.Controls.Add(btnEditVariation);
            this.Controls.Add(btnDeleteVariation);
            this.Controls.Add(btnRefresh);
            this.Controls.Add(grpAllVariations);
            this.Controls.Add(grpApprovedVariations);
        }

        private void LoadData()
        {
            var variations = _dbHelper.GetAllVariations();

            dgvVariations.Columns.Clear();
            dgvVariations.AutoGenerateColumns = false;

            // Add columns
            dgvVariations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VariationNumber",
                HeaderText = "Variation #",
                Width = 100
            });

            dgvVariations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VariationName",
                HeaderText = "Name",
                Width = 200
            });

            dgvVariations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VariationDate",
                HeaderText = "Date",
                Width = 100
            });

            dgvVariations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VariationType",
                HeaderText = "Type",
                Width = 100
            });

            dgvVariations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TotalValue",
                HeaderText = "Total Value",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }
            });

            dgvVariations.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsApproved",
                HeaderText = "Approved",
                Width = 80
            });

            dgvVariations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ApprovedBy",
                HeaderText = "Approved By",
                Width = 150
            });

            dgvVariations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ApprovedDate",
                HeaderText = "Approved Date",
                Width = 150
            });

            // Add approve button column
            var approveButtonColumn = new DataGridViewButtonColumn
            {
                HeaderText = "Action",
                Text = "Approve",
                UseColumnTextForButtonValue = false,
                Width = 100
            };
            dgvVariations.Columns.Add(approveButtonColumn);

            dgvVariations.DataSource = variations;

            // Update summary
            var summary = _dbHelper.GetVariationSummary();
            lblTotalAdditions.Text = $"Total Additions: {summary.TotalAdditions:C2}";
            lblTotalCredits.Text = $"Total Credits: {summary.TotalCredits:C2}";
            lblNetValue.Text = $"Net Value: {summary.NetValue:C2}";

            lblApprovedAdditions.Text = $"Approved Additions: {summary.ApprovedAdditions:C2}";
            lblApprovedCredits.Text = $"Approved Credits: {summary.ApprovedCredits:C2}";
            lblApprovedNetValue.Text = $"Approved Net Value: {summary.ApprovedNetValue:C2}";

            // Update button text based on approval status
            for (int i = 0; i < dgvVariations.Rows.Count; i++)
            {
                var variation = variations[i];
                dgvVariations.Rows[i].Cells[8].Value = variation.IsApproved ? "Unapprove" : "Approve";
            }
        }

        private void DgvVariations_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 8) return;

            var variations = _dbHelper.GetAllVariations();
            var variation = variations[e.RowIndex];

            if (variation.IsApproved)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to unapprove this variation?",
                    "Unapprove Variation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _dbHelper.UnapproveVariation(variation.Id);
                    LoadData();
                }
            }
            else
            {
                var approvalForm = new ApprovalForm();
                if (approvalForm.ShowDialog() == DialogResult.OK)
                {
                    _dbHelper.ApproveVariation(variation.Id, approvalForm.ApprovedBy);
                    LoadData();
                }
            }
        }

        private void DgvVariations_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex == 8) return;
            BtnEditVariation_Click(sender, e);
        }

        private void BtnNewVariation_Click(object? sender, EventArgs e)
        {
            var editorForm = new VariationEditorForm(_dbHelper);
            if (editorForm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnEditVariation_Click(object? sender, EventArgs e)
        {
            if (dgvVariations.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a variation to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var variations = _dbHelper.GetAllVariations();
            var selectedVariation = variations[dgvVariations.SelectedRows[0].Index];

            var editorForm = new VariationEditorForm(_dbHelper, selectedVariation.Id);
            if (editorForm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnDeleteVariation_Click(object? sender, EventArgs e)
        {
            if (dgvVariations.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a variation to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var variations = _dbHelper.GetAllVariations();
            var selectedVariation = variations[dgvVariations.SelectedRows[0].Index];

            var result = MessageBox.Show(
                $"Are you sure you want to delete variation '{selectedVariation.VariationNumber} - {selectedVariation.VariationName}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                _dbHelper.DeleteVariation(selectedVariation.Id);
                LoadData();
            }
        }
    }

    public class ApprovalForm : Form
    {
        private TextBox txtApprovedBy;
        private Button btnOK;
        private Button btnCancel;

        public string ApprovedBy { get; private set; } = "";

        public ApprovalForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Approve Variation";
            this.Size = new Size(400, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblApprovedBy = new Label
            {
                Text = "Approved By:",
                Location = new Point(20, 20),
                Size = new Size(100, 20)
            };

            txtApprovedBy = new TextBox
            {
                Location = new Point(130, 20),
                Size = new Size(230, 20)
            };

            btnOK = new Button
            {
                Text = "OK",
                Location = new Point(130, 60),
                Size = new Size(100, 30),
                DialogResult = DialogResult.OK
            };
            btnOK.Click += BtnOK_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(240, 60),
                Size = new Size(100, 30),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.Add(lblApprovedBy);
            this.Controls.Add(txtApprovedBy);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtApprovedBy.Text))
            {
                MessageBox.Show("Please enter your name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            ApprovedBy = txtApprovedBy.Text.Trim();
        }
    }
}
