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
        private Button btnDuplicateVariation;
        private Button btnMoveUp;
        private Button btnMoveDown;
        private Button btnRefresh;
        private Button btnPrintSummary;
        private int _selectedRowIndex = -1;

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
            this.Size = new Size(1920, 1080);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(240, 240, 245);
            this.Shown += SummaryForm_Shown;
            this.Resize += SummaryForm_Resize;

            // Project Info
            lblProjectInfo = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(800, 25),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Text = $"Project: {_projectInfo.ProjectName} {_projectInfo.ProjectNumber} - Client: {_projectInfo.ClientName}",
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            lblDate = new Label
            {
                Location = new Point(850, 20),
                Size = new Size(300, 25),
                Font = new Font("Arial", 10),
                Text = $"Date: {DateTime.Now:dd-MM-yyyy}",
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            // DataGridView for variations
            dgvVariations = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(this.ClientSize.Width - 40, 350),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            dgvVariations.DefaultCellStyle.SelectionBackColor = dgvVariations.DefaultCellStyle.BackColor;
            dgvVariations.DefaultCellStyle.SelectionForeColor = dgvVariations.DefaultCellStyle.ForeColor;
            dgvVariations.CellContentClick += DgvVariations_CellContentClick;
            dgvVariations.CellDoubleClick += DgvVariations_CellDoubleClick;
            dgvVariations.CellEndEdit += DgvVariations_CellEndEdit;
            dgvVariations.RowPrePaint += DgvVariations_RowPrePaint;
            dgvVariations.RowPostPaint += DgvVariations_RowPostPaint;

            // Buttons
            btnNewVariation = new Button
            {
                Location = new Point(20, 420),
                Size = new Size(150, 35),
                Text = "New Variation",
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnNewVariation.Click += BtnNewVariation_Click;

            btnEditVariation = new Button
            {
                Location = new Point(180, 420),
                Size = new Size(150, 35),
                Text = "Edit Variation",
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnEditVariation.Click += BtnEditVariation_Click;

            btnDeleteVariation = new Button
            {
                Location = new Point(340, 420),
                Size = new Size(150, 35),
                Text = "Delete Variation",
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnDeleteVariation.Click += BtnDeleteVariation_Click;

            btnDuplicateVariation = new Button
            {
                Location = new Point(500, 420),
                Size = new Size(150, 35),
                Text = "Duplicate Variation",
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnDuplicateVariation.Click += BtnDuplicateVariation_Click;

            btnMoveUp = new Button
            {
                Location = new Point(660, 420),
                Size = new Size(100, 35),
                Text = "Move Up",
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnMoveUp.Click += BtnMoveUp_Click;

            btnMoveDown = new Button
            {
                Location = new Point(770, 420),
                Size = new Size(100, 35),
                Text = "Move Down",
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnMoveDown.Click += BtnMoveDown_Click;

            btnRefresh = new Button
            {
                Location = new Point(880, 420),
                Size = new Size(100, 35),
                Text = "Refresh",
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnRefresh.Click += (s, e) => LoadData();

            btnPrintSummary = new Button
            {
                Location = new Point(990, 420),
                Size = new Size(150, 35),
                Text = "📄 Print Summary",
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnPrintSummary.FlatAppearance.BorderSize = 0;
            btnPrintSummary.Click += BtnPrintSummary_Click;

            // Summary boxes
            grpAllVariations = new GroupBox
            {
                Location = new Point(20, 470),
                Size = new Size((this.ClientSize.Width - 60) / 2, 150),
                Text = "All Variations Summary",
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
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
                Location = new Point((this.ClientSize.Width / 2) + 10, 470),
                Size = new Size((this.ClientSize.Width - 60) / 2, 150),
                Text = "Approved Variations Summary",
                Anchor = AnchorStyles.Top | AnchorStyles.Right
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
            this.Controls.Add(btnDuplicateVariation);
            this.Controls.Add(btnMoveUp);
            this.Controls.Add(btnMoveDown);
            this.Controls.Add(btnRefresh);
            this.Controls.Add(btnPrintSummary);
            this.Controls.Add(grpAllVariations);
            this.Controls.Add(grpApprovedVariations);
        }

        private void SummaryForm_Shown(object? sender, EventArgs e)
        {
            // Reload data when form is shown to ensure button text is visible
            LoadData();
        }

        private void SummaryForm_Resize(object? sender, EventArgs e)
        {
            // Adjust lblDate position to stay on the right side
            if (lblDate != null)
            {
                lblDate.Left = this.ClientSize.Width - lblDate.Width - 20;
            }

            // Adjust group box widths
            if (grpAllVariations != null)
            {
                grpAllVariations.Width = (this.ClientSize.Width - 60) / 2;
            }

            if (grpApprovedVariations != null)
            {
                grpApprovedVariations.Width = (this.ClientSize.Width - 60) / 2;
                grpApprovedVariations.Left = (this.ClientSize.Width / 2) + 10;
            }
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
                Width = 81,
                ReadOnly = true
            });

            dgvVariations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VariationName",
                HeaderText = "Name",
                Width = 351,
                ReadOnly = true
            });

            dgvVariations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VariationDate",
                HeaderText = "Date",
                Width = 81,
                ReadOnly = true
            });

            dgvVariations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VariationType",
                HeaderText = "Type",
                Width = 72,
                ReadOnly = true
            });

            dgvVariations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TotalValue",
                HeaderText = "Total Value",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" },
                ReadOnly = true
            });

            dgvVariations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ApprovedBy",
                HeaderText = "Approved By",
                Width = 108,
                ReadOnly = false,
                Name = "ApprovedBy"
            });

            dgvVariations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PurchaseOrder",
                HeaderText = "Purchase Order",
                Width = 108,
                ReadOnly = false,
                Name = "PurchaseOrder"
            });

            dgvVariations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "JobNumber",
                HeaderText = "Job Number",
                Width = 108,
                ReadOnly = false,
                Name = "JobNumber"
            });

            dgvVariations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ApprovedDate",
                HeaderText = "Approved Date",
                Width = 117,
                ReadOnly = true,
                Name = "ApprovedDate"
            });

            // Add print button column
            var printButtonColumn = new DataGridViewButtonColumn
            {
                HeaderText = "Print",
                Text = "📄",
                UseColumnTextForButtonValue = true,
                Width = 54,
                Name = "PrintButton"
            };
            dgvVariations.Columns.Add(printButtonColumn);

            // Add approve button column
            var approveButtonColumn = new DataGridViewButtonColumn
            {
                HeaderText = "Action",
                Text = "Approve",
                UseColumnTextForButtonValue = false,
                Width = 90,
                Name = "ActionButton",
                FlatStyle = FlatStyle.Standard
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

            // Update button text and cell readonly status based on approval status
            for (int i = 0; i < dgvVariations.Rows.Count; i++)
            {
                var variation = variations[i];
                dgvVariations.Rows[i].Cells["ActionButton"].Value = variation.IsApproved ? "Unapprove" : "Approve";

                // Make ApprovedBy, PurchaseOrder, and JobNumber editable only if approved
                dgvVariations.Rows[i].Cells["ApprovedBy"].ReadOnly = !variation.IsApproved;
                dgvVariations.Rows[i].Cells["PurchaseOrder"].ReadOnly = !variation.IsApproved;
                dgvVariations.Rows[i].Cells["JobNumber"].ReadOnly = !variation.IsApproved;

                if (!variation.IsApproved)
                {
                    dgvVariations.Rows[i].Cells["ApprovedBy"].Style.BackColor = Color.LightGray;
                    dgvVariations.Rows[i].Cells["ApprovedBy"].Style.SelectionBackColor = Color.LightGray;
                    dgvVariations.Rows[i].Cells["PurchaseOrder"].Style.BackColor = Color.LightGray;
                    dgvVariations.Rows[i].Cells["PurchaseOrder"].Style.SelectionBackColor = Color.LightGray;
                    dgvVariations.Rows[i].Cells["JobNumber"].Style.BackColor = Color.LightGray;
                    dgvVariations.Rows[i].Cells["JobNumber"].Style.SelectionBackColor = Color.LightGray;
                }
                else
                {
                    dgvVariations.Rows[i].Cells["ApprovedBy"].Style.BackColor = Color.White;
                    dgvVariations.Rows[i].Cells["ApprovedBy"].Style.SelectionBackColor = Color.White;
                    dgvVariations.Rows[i].Cells["PurchaseOrder"].Style.BackColor = Color.White;
                    dgvVariations.Rows[i].Cells["PurchaseOrder"].Style.SelectionBackColor = Color.White;
                    dgvVariations.Rows[i].Cells["JobNumber"].Style.BackColor = Color.White;
                    dgvVariations.Rows[i].Cells["JobNumber"].Style.SelectionBackColor = Color.White;
                }
            }

            // Restore selection if we had one
            if (_selectedRowIndex >= 0 && _selectedRowIndex < dgvVariations.Rows.Count)
            {
                dgvVariations.ClearSelection();
                dgvVariations.Rows[_selectedRowIndex].Selected = true;
                dgvVariations.FirstDisplayedScrollingRowIndex = _selectedRowIndex;
            }
        }

        private void DgvVariations_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            var variations = _dbHelper.GetAllVariations();
            if (e.RowIndex >= 0 && e.RowIndex < variations.Count)
            {
                var variation = variations[e.RowIndex];
                if (variation.IsApproved)
                {
                    dgvVariations.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                    dgvVariations.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.LightGreen;
                }
                else
                {
                    dgvVariations.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                    dgvVariations.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.White;
                }
            }
        }

        private void DgvVariations_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
        {
            // Draw a border around the selected row
            if (dgvVariations.Rows[e.RowIndex].Selected)
            {
                using (Pen pen = new Pen(Color.DarkBlue, 2))
                {
                    Rectangle rect = e.RowBounds;
                    rect.Width -= 1;
                    rect.Height -= 1;
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }
        }

        private void DgvVariations_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var columnName = dgvVariations.Columns[e.ColumnIndex].Name;
            var variations = _dbHelper.GetAllVariations();
            var variation = variations[e.RowIndex];

            if (columnName == "PurchaseOrder")
            {
                var newPurchaseOrder = dgvVariations.Rows[e.RowIndex].Cells["PurchaseOrder"].Value?.ToString();
                _dbHelper.UpdatePurchaseOrder(variation.Id, newPurchaseOrder);
            }
            else if (columnName == "JobNumber")
            {
                var newJobNumber = dgvVariations.Rows[e.RowIndex].Cells["JobNumber"].Value?.ToString();
                _dbHelper.UpdateJobNumber(variation.Id, newJobNumber);
            }
            else if (columnName == "ApprovedBy")
            {
                var newApprovedBy = dgvVariations.Rows[e.RowIndex].Cells["ApprovedBy"].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(newApprovedBy))
                {
                    _dbHelper.UpdateApprovedBy(variation.Id, newApprovedBy);
                }
            }
        }

        private void DgvVariations_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var columnName = dgvVariations.Columns[e.ColumnIndex].Name;
            var variations = _dbHelper.GetAllVariations();
            var variation = variations[e.RowIndex];

            // Handle Print button click
            if (columnName == "PrintButton")
            {
                PrintVariation(variation.Id);
                return;
            }

            if (columnName != "ActionButton") return;

            _selectedRowIndex = e.RowIndex;

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
            if (e.RowIndex < 0) return;

            var columnName = dgvVariations.Columns[e.ColumnIndex].Name;
            // Don't open editor on double-click for these columns
            if (columnName == "ActionButton" || columnName == "ApprovedBy" || columnName == "PurchaseOrder") return;

            BtnEditVariation_Click(sender, e);
        }

        private void BtnNewVariation_Click(object? sender, EventArgs e)
        {
            var editorForm = new VariationEditorForm(_dbHelper);
            editorForm.ShowDialog();
            // Always refresh when editor closes, regardless of save status
            LoadData();
        }

        private void BtnEditVariation_Click(object? sender, EventArgs e)
        {
            if (dgvVariations.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a variation to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _selectedRowIndex = dgvVariations.SelectedRows[0].Index;
            var variations = _dbHelper.GetAllVariations();
            var selectedVariation = variations[_selectedRowIndex];

            var editorForm = new VariationEditorForm(_dbHelper, selectedVariation.Id);
            editorForm.ShowDialog();
            // Always refresh when editor closes, regardless of save status
            LoadData();
        }

        private void BtnDeleteVariation_Click(object? sender, EventArgs e)
        {
            if (dgvVariations.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a variation to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _selectedRowIndex = dgvVariations.SelectedRows[0].Index;
            var variations = _dbHelper.GetAllVariations();
            var selectedVariation = variations[_selectedRowIndex];

            var result = MessageBox.Show(
                $"Are you sure you want to delete variation '{selectedVariation.VariationNumber} - {selectedVariation.VariationName}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                _dbHelper.DeleteVariation(selectedVariation.Id);
                _selectedRowIndex = -1;
                LoadData();
            }
        }

        private void BtnDuplicateVariation_Click(object? sender, EventArgs e)
        {
            if (dgvVariations.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a variation to duplicate.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _selectedRowIndex = dgvVariations.SelectedRows[0].Index;
            var variations = _dbHelper.GetAllVariations();
            var selectedVariation = variations[_selectedRowIndex];

            try
            {
                int newVariationId = _dbHelper.DuplicateVariation(selectedVariation.Id);
                LoadData();

                // Select the newly duplicated variation
                for (int i = 0; i < dgvVariations.Rows.Count; i++)
                {
                    var vars = _dbHelper.GetAllVariations();
                    if (vars[i].Id == newVariationId)
                    {
                        _selectedRowIndex = i;
                        dgvVariations.ClearSelection();
                        dgvVariations.Rows[i].Selected = true;
                        dgvVariations.FirstDisplayedScrollingRowIndex = i;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error duplicating variation: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnMoveUp_Click(object? sender, EventArgs e)
        {
            if (dgvVariations.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a variation to move.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int currentIndex = dgvVariations.SelectedRows[0].Index;
            if (currentIndex == 0)
            {
                MessageBox.Show("This variation is already at the top.", "Cannot Move",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var variations = _dbHelper.GetAllVariations();

            // Ensure all display orders are set correctly
            for (int i = 0; i < variations.Count; i++)
            {
                if (variations[i].DisplayOrder != i)
                {
                    _dbHelper.UpdateDisplayOrder(variations[i].Id, i);
                    variations[i].DisplayOrder = i;
                }
            }

            var currentVariation = variations[currentIndex];
            var previousVariation = variations[currentIndex - 1];

            // Swap display orders
            _dbHelper.UpdateDisplayOrder(currentVariation.Id, currentIndex - 1);
            _dbHelper.UpdateDisplayOrder(previousVariation.Id, currentIndex);

            _selectedRowIndex = currentIndex - 1;
            LoadData();
        }

        private void BtnMoveDown_Click(object? sender, EventArgs e)
        {
            if (dgvVariations.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a variation to move.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var variations = _dbHelper.GetAllVariations();
            int currentIndex = dgvVariations.SelectedRows[0].Index;

            if (currentIndex >= variations.Count - 1)
            {
                MessageBox.Show("This variation is already at the bottom.", "Cannot Move",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Ensure all display orders are set correctly
            for (int i = 0; i < variations.Count; i++)
            {
                if (variations[i].DisplayOrder != i)
                {
                    _dbHelper.UpdateDisplayOrder(variations[i].Id, i);
                    variations[i].DisplayOrder = i;
                }
            }

            var currentVariation = variations[currentIndex];
            var nextVariation = variations[currentIndex + 1];

            // Swap display orders
            _dbHelper.UpdateDisplayOrder(currentVariation.Id, currentIndex + 1);
            _dbHelper.UpdateDisplayOrder(nextVariation.Id, currentIndex);

            _selectedRowIndex = currentIndex + 1;
            LoadData();
        }

        private void BtnPrintSummary_Click(object? sender, EventArgs e)
        {
            try
            {
                string outputFolder = System.IO.Directory.GetCurrentDirectory();
                var pdfGenerator = new PdfGenerator(_dbHelper, outputFolder);
                string filePath = pdfGenerator.GenerateSummaryPdf();

                MessageBox.Show($"Summary PDF generated successfully!\n\nSaved to: {filePath}", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Open the PDF
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintVariation(int variationId)
        {
            try
            {
                string outputFolder = System.IO.Directory.GetCurrentDirectory();
                var pdfGenerator = new PdfGenerator(_dbHelper, outputFolder);
                string filePath = pdfGenerator.GenerateVariationPdf(variationId);

                MessageBox.Show($"Variation PDF generated successfully!\n\nSaved to: {filePath}", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Open the PDF
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
