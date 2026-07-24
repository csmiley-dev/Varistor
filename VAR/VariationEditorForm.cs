using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace VAR
{
    public class VariationEditorForm : Form
    {
        private DatabaseHelper _dbHelper;
        private int? _variationId;
        private Variation _variation;
        private List<LineItem> _lineItems;
        private List<HourlyRate> _hourlyRates;
        private bool _hasUnsavedChanges = false;

        private TextBox txtVariationNumber;
        private TextBox txtVariationName;
        private DateTimePicker dtpVariationDate;
        private ComboBox cboClientContact;
        private DataGridView dgvLineItems;
        private Label lblMaterialSubtotal;
        private Label lblLabourSubtotal;
        private Label lblGrandTotal;
        private Button btnSave;
        private Button btnClose;
        private Button btnAddRow;

        public VariationEditorForm(DatabaseHelper dbHelper, int? variationId = null)
        {
            _dbHelper = dbHelper;
            _variationId = variationId;
            _hourlyRates = _dbHelper.GetHourlyRates();

            if (_variationId.HasValue)
            {
                _variation = _dbHelper.GetVariation(_variationId.Value)!;
                _lineItems = _dbHelper.GetLineItems(_variationId.Value);
            }
            else
            {
                _variation = new Variation
                {
                    VariationNumber = _dbHelper.GetNextVariationNumber(),
                    VariationDate = DateTime.Now.ToString("yyyy-MM-dd")
                };
                _lineItems = new List<LineItem>();

                // Add 8 default rows
                for (int i = 1; i <= 8; i++)
                {
                    _lineItems.Add(new LineItem { ItemNumber = i, ItemType = "Cost" });
                }
            }

            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _variationId.HasValue ? "Edit Variation" : "New Variation";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += VariationEditorForm_FormClosing;
            this.KeyPreview = true;
            this.KeyDown += VariationEditorForm_KeyDown;

            int leftMargin = 20;
            int topMargin = 20;
            int labelWidth = 120;
            int controlWidth = 250;
            int rowHeight = 35;

            // Variation Number
            Label lblVariationNumber = new Label
            {
                Text = "Variation Number:",
                Location = new Point(leftMargin, topMargin),
                Size = new Size(labelWidth, 20)
            };
            txtVariationNumber = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, topMargin),
                Size = new Size(controlWidth, 20),
                Text = _variation.VariationNumber
            };
            txtVariationNumber.TextChanged += Control_Changed;

            // Variation Name
            Label lblVariationName = new Label
            {
                Text = "Variation Name:",
                Location = new Point(leftMargin, topMargin + rowHeight),
                Size = new Size(labelWidth, 20)
            };
            txtVariationName = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, topMargin + rowHeight),
                Size = new Size(controlWidth, 20),
                Text = _variation.VariationName
            };
            txtVariationName.TextChanged += Control_Changed;

            // Variation Date
            Label lblVariationDate = new Label
            {
                Text = "Date:",
                Location = new Point(leftMargin + 400, topMargin),
                Size = new Size(labelWidth, 20)
            };
            dtpVariationDate = new DateTimePicker
            {
                Location = new Point(leftMargin + 400 + labelWidth, topMargin),
                Size = new Size(controlWidth, 20),
                Format = DateTimePickerFormat.Short
            };
            DateTime.TryParse(_variation.VariationDate, out DateTime variationDate);
            dtpVariationDate.Value = variationDate == DateTime.MinValue ? DateTime.Now : variationDate;
            dtpVariationDate.ValueChanged += Control_Changed;

            // Client Contact
            Label lblClientContact = new Label
            {
                Text = "Client Contact:",
                Location = new Point(leftMargin + 400, topMargin + rowHeight),
                Size = new Size(labelWidth, 20)
            };
            cboClientContact = new ComboBox
            {
                Location = new Point(leftMargin + 400 + labelWidth, topMargin + rowHeight),
                Size = new Size(controlWidth, 20),
                DropDownStyle = ComboBoxStyle.DropDown
            };
            cboClientContact.SelectedIndexChanged += Control_Changed;
            cboClientContact.TextChanged += Control_Changed;

            // Line Items Grid
            dgvLineItems = new DataGridView
            {
                Location = new Point(leftMargin, topMargin + rowHeight * 3),
                Size = new Size(1350, 500),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
            };
            dgvLineItems.CellValueChanged += DgvLineItems_CellValueChanged;
            dgvLineItems.CurrentCellDirtyStateChanged += DgvLineItems_CurrentCellDirtyStateChanged;
            dgvLineItems.CellPainting += DgvLineItems_CellPainting;

            // Add Row Button
            btnAddRow = new Button
            {
                Text = "Add Row",
                Location = new Point(leftMargin, topMargin + rowHeight * 3 + 510),
                Size = new Size(100, 30)
            };
            btnAddRow.Click += BtnAddRow_Click;

            // Subtotals
            lblMaterialSubtotal = new Label
            {
                Location = new Point(leftMargin, topMargin + rowHeight * 3 + 550),
                Size = new Size(400, 20),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            lblLabourSubtotal = new Label
            {
                Location = new Point(leftMargin, topMargin + rowHeight * 3 + 575),
                Size = new Size(400, 20),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            lblGrandTotal = new Label
            {
                Location = new Point(leftMargin, topMargin + rowHeight * 3 + 600),
                Size = new Size(400, 20),
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            // Buttons
            btnSave = new Button
            {
                Text = "Save (Ctrl+S)",
                Location = new Point(leftMargin + 450, topMargin + rowHeight * 3 + 590),
                Size = new Size(120, 35)
            };
            btnSave.Click += BtnSave_Click;

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(leftMargin + 580, topMargin + rowHeight * 3 + 590),
                Size = new Size(120, 35)
            };
            btnClose.Click += BtnClose_Click;

            this.Controls.Add(lblVariationNumber);
            this.Controls.Add(txtVariationNumber);
            this.Controls.Add(lblVariationName);
            this.Controls.Add(txtVariationName);
            this.Controls.Add(lblVariationDate);
            this.Controls.Add(dtpVariationDate);
            this.Controls.Add(lblClientContact);
            this.Controls.Add(cboClientContact);
            this.Controls.Add(dgvLineItems);
            this.Controls.Add(btnAddRow);
            this.Controls.Add(lblMaterialSubtotal);
            this.Controls.Add(lblLabourSubtotal);
            this.Controls.Add(lblGrandTotal);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnClose);
        }

        private void LoadData()
        {
            // Load client contacts
            var contacts = _dbHelper.GetClientContacts();
            cboClientContact.Items.Clear();
            foreach (var contact in contacts)
            {
                cboClientContact.Items.Add(contact);
            }

            if (!string.IsNullOrEmpty(_variation.ClientContact))
            {
                cboClientContact.Text = _variation.ClientContact;
            }

            // Setup grid columns
            dgvLineItems.Columns.Clear();

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ItemNumber",
                HeaderText = "Item #",
                Width = 60
            });

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ItemDescription",
                HeaderText = "Description",
                Width = 250
            });

            var typeColumn = new DataGridViewComboBoxColumn
            {
                Name = "ItemType",
                HeaderText = "Type",
                Width = 80,
                DataSource = new[] { "Cost", "Refund" }
            };
            dgvLineItems.Columns.Add(typeColumn);

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MaterialQty",
                HeaderText = "Mat. Qty",
                Width = 80
            });

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MaterialCost",
                HeaderText = "Mat. Cost",
                Width = 90
            });

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MaterialTotal",
                HeaderText = "Mat. Total",
                Width = 100,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.LightGray, Format = "C2" }
            });

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "HourlyQty",
                HeaderText = "Hours",
                Width = 80
            });

            // Create hourly rate dropdown with rates and custom option
            var rateOptions = _hourlyRates.Select(r => $"{r.RateName} (${r.RateValue:F2})").ToList();
            rateOptions.Add("Custom");
            var hourlyRateColumn = new DataGridViewComboBoxColumn
            {
                Name = "HourlyRate",
                HeaderText = "Hourly Rate",
                Width = 150,
                DataSource = rateOptions.ToArray()
            };
            dgvLineItems.Columns.Add(hourlyRateColumn);

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CustomRate",
                HeaderText = "Custom Rate",
                Width = 100
            });

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LabourTotal",
                HeaderText = "Labour Total",
                Width = 110,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.LightGray, Format = "C2" }
            });

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LineTotal",
                HeaderText = "Line Total",
                Width = 110,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.LightGray, Format = "C2" }
            });

            // Load line items into grid
            foreach (var item in _lineItems)
            {
                int rowIndex = dgvLineItems.Rows.Add();
                var row = dgvLineItems.Rows[rowIndex];

                row.Cells["ItemNumber"].Value = item.ItemNumber;
                row.Cells["ItemDescription"].Value = item.ItemDescription;
                row.Cells["ItemType"].Value = item.ItemType;
                row.Cells["MaterialQty"].Value = item.MaterialQty == 0 ? "" : item.MaterialQty.ToString();
                row.Cells["MaterialCost"].Value = item.MaterialCost == 0 ? "" : item.MaterialCost.ToString();
                row.Cells["MaterialTotal"].Value = item.MaterialTotal;
                row.Cells["HourlyQty"].Value = item.HourlyQty == 0 ? "" : item.HourlyQty.ToString();

                // Set hourly rate
                if (item.HourlyRate > 0)
                {
                    var matchingRate = _hourlyRates.FirstOrDefault(r => r.RateValue == item.HourlyRate);
                    if (matchingRate != null)
                    {
                        row.Cells["HourlyRate"].Value = $"{matchingRate.RateName} (${matchingRate.RateValue:F2})";
                        row.Cells["CustomRate"].Value = "";
                    }
                    else
                    {
                        row.Cells["HourlyRate"].Value = "Custom";
                        row.Cells["CustomRate"].Value = item.HourlyRate.ToString();
                    }
                }

                row.Cells["LabourTotal"].Value = item.LabourTotal;
                row.Cells["LineTotal"].Value = item.LineTotal;
            }

            UpdateTotals();
        }

        private void DgvLineItems_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dgvLineItems.IsCurrentCellDirty)
            {
                dgvLineItems.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvLineItems_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvLineItems.Rows[e.RowIndex];

            // Parse numeric values
            decimal matQty = ParseDecimal(row.Cells["MaterialQty"].Value);
            decimal matCost = ParseDecimal(row.Cells["MaterialCost"].Value);
            decimal hourQty = ParseDecimal(row.Cells["HourlyQty"].Value);
            decimal hourRate = 0;

            // Get hourly rate
            string rateSelection = row.Cells["HourlyRate"].Value?.ToString() ?? "";
            if (rateSelection == "Custom")
            {
                hourRate = ParseDecimal(row.Cells["CustomRate"].Value);
            }
            else if (!string.IsNullOrEmpty(rateSelection))
            {
                var matchingRate = _hourlyRates.FirstOrDefault(r => rateSelection.StartsWith(r.RateName));
                if (matchingRate != null)
                {
                    hourRate = matchingRate.RateValue;
                    row.Cells["CustomRate"].Value = "";
                }
            }

            // Get item type
            string itemType = row.Cells["ItemType"].Value?.ToString() ?? "Cost";
            int multiplier = itemType == "Refund" ? -1 : 1;

            // Calculate totals
            decimal matTotal = matQty * matCost * multiplier;
            decimal labourTotal = hourQty * hourRate * multiplier;
            decimal lineTotal = matTotal + labourTotal;

            row.Cells["MaterialTotal"].Value = matTotal;
            row.Cells["LabourTotal"].Value = labourTotal;
            row.Cells["LineTotal"].Value = lineTotal;

            UpdateTotals();
            _hasUnsavedChanges = true;
        }

        private void DgvLineItems_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Highlight MaterialQty cell if empty
            if (e.ColumnIndex == dgvLineItems.Columns["MaterialQty"].Index)
            {
                var value = dgvLineItems.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    e.CellStyle.BackColor = Color.LightSalmon;
                }
            }
        }

        private decimal ParseDecimal(object? value)
        {
            if (value == null) return 0;
            string strValue = value.ToString()!;
            if (decimal.TryParse(strValue, out decimal result))
                return result;
            return 0;
        }

        private void UpdateTotals()
        {
            decimal materialSubtotal = 0;
            decimal labourSubtotal = 0;
            decimal grandTotal = 0;

            foreach (DataGridViewRow row in dgvLineItems.Rows)
            {
                materialSubtotal += ParseDecimal(row.Cells["MaterialTotal"].Value);
                labourSubtotal += ParseDecimal(row.Cells["LabourTotal"].Value);
                grandTotal += ParseDecimal(row.Cells["LineTotal"].Value);
            }

            lblMaterialSubtotal.Text = $"Material Subtotal: {materialSubtotal:C2}";
            lblLabourSubtotal.Text = $"Labour Subtotal: {labourSubtotal:C2}";
            lblGrandTotal.Text = $"Grand Total: {grandTotal:C2}";
        }

        private void BtnAddRow_Click(object? sender, EventArgs e)
        {
            int nextItemNumber = dgvLineItems.Rows.Count + 1;
            int rowIndex = dgvLineItems.Rows.Add();
            var row = dgvLineItems.Rows[rowIndex];

            row.Cells["ItemNumber"].Value = nextItemNumber;
            row.Cells["ItemType"].Value = "Cost";
            row.Cells["MaterialTotal"].Value = 0;
            row.Cells["LabourTotal"].Value = 0;
            row.Cells["LineTotal"].Value = 0;

            _hasUnsavedChanges = true;
        }

        private void Control_Changed(object? sender, EventArgs e)
        {
            _hasUnsavedChanges = true;
        }

        private void VariationEditorForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                BtnSave_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtVariationNumber.Text))
            {
                MessageBox.Show("Please enter a variation number.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtVariationName.Text))
            {
                MessageBox.Show("Please enter a variation name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check for duplicates
            if (_dbHelper.VariationNumberExists(txtVariationNumber.Text, _variationId))
            {
                MessageBox.Show("A variation with this number already exists.", "Duplicate Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_dbHelper.VariationNameExists(txtVariationName.Text, _variationId))
            {
                MessageBox.Show("A variation with this name already exists.", "Duplicate Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Update variation object
            _variation.VariationNumber = txtVariationNumber.Text.Trim();
            _variation.VariationName = txtVariationName.Text.Trim();
            _variation.VariationDate = dtpVariationDate.Value.ToString("yyyy-MM-dd");
            _variation.ClientContact = cboClientContact.Text.Trim();

            // Collect line items from grid
            var lineItems = new List<LineItem>();
            decimal totalValue = 0;

            foreach (DataGridViewRow row in dgvLineItems.Rows)
            {
                int itemNumber = (int)ParseDecimal(row.Cells["ItemNumber"].Value);
                if (itemNumber == 0) continue;

                decimal matQty = ParseDecimal(row.Cells["MaterialQty"].Value);
                decimal matCost = ParseDecimal(row.Cells["MaterialCost"].Value);
                decimal hourQty = ParseDecimal(row.Cells["HourlyQty"].Value);
                decimal hourRate = 0;

                string rateSelection = row.Cells["HourlyRate"].Value?.ToString() ?? "";
                if (rateSelection == "Custom")
                {
                    hourRate = ParseDecimal(row.Cells["CustomRate"].Value);
                }
                else if (!string.IsNullOrEmpty(rateSelection))
                {
                    var matchingRate = _hourlyRates.FirstOrDefault(r => rateSelection.StartsWith(r.RateName));
                    if (matchingRate != null)
                    {
                        hourRate = matchingRate.RateValue;
                    }
                }

                var lineItem = new LineItem
                {
                    ItemNumber = itemNumber,
                    ItemDescription = row.Cells["ItemDescription"].Value?.ToString() ?? "",
                    ItemType = row.Cells["ItemType"].Value?.ToString() ?? "Cost",
                    MaterialQty = matQty,
                    MaterialCost = matCost,
                    HourlyQty = hourQty,
                    HourlyRate = hourRate
                };

                lineItems.Add(lineItem);
                totalValue += lineItem.LineTotal;
            }

            _variation.TotalValue = totalValue;

            try
            {
                int savedId = _dbHelper.SaveVariation(_variation, lineItems);
                _variationId = savedId;
                _hasUnsavedChanges = false;

                MessageBox.Show("Variation saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving variation: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClose_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void VariationEditorForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Do you want to save before closing?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    BtnSave_Click(sender, e);
                    if (_hasUnsavedChanges) // Save failed
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        this.DialogResult = DialogResult.OK;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                else
                {
                    this.DialogResult = DialogResult.Cancel;
                }
            }
        }
    }
}
