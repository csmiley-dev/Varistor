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
        private List<string> _staffNames;
        private bool _hasUnsavedChanges = false;

        private TextBox txtVariationNumber;
        private TextBox txtVariationName;
        private DateTimePicker dtpVariationDate;
        private ComboBox cboClientContact;
        private ComboBox cboCreatedBy;
        private Label lblScopeOfWorks;
        private TextBox txtScopeOfWorks;
        private Label lblExclusions;
        private TextBox txtExclusions;
        private DataGridView dgvLineItems;
        private Label lblMaterialSubtotal;
        private Label lblLabourSubtotal;
        private Label lblGrandTotal;
        private Button btnSave;
        private Button btnClose;
        private Button btnPrint;
        private Button btnAddRow;
        private Button btnDeleteRow;
        private Button btnMoveUp;
        private Button btnMoveDown;
        private Label lblSaveStatus;
        private Timer saveStatusTimer;
        private string _originalDataHash = "";

        // Vertical offsets of the controls below the grid, measured from the grid's bottom edge.
        // Used to reflow them when the grid grows/shrinks on window resize.
        private const int GapGridToNotes = 10;
        private const int NotesHeight = 60;
        private const int NotesColumnGap = 15;
        private const int GridBottomOffsetMoveButtons = 10;
        private const int GridBottomOffsetMaterialSubtotal = 50;
        private const int GridBottomOffsetLabourSubtotal = 75;
        private const int GridBottomOffsetGrandTotal = 100;
        private const int GridBottomOffsetSaveStatus = 60;
        private const int GridBottomOffsetActionButtons = 90;
        private const int ReservedHeightBelowGrid = 145 + GapGridToNotes + NotesHeight;
        private const int MinGridHeight = 200;

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
                    VariationDate = DateTime.Now.ToString("dd-MM-yyyy")
                };
                _lineItems = new List<LineItem>();

                // Add 8 default rows
                for (int i = 1; i <= 8; i++)
                {
                    _lineItems.Add(new LineItem { ItemNumber = i, ItemType = "Cost" });
                }

                // Pre-fill the first row as an Administration line, if that rate still exists
                var adminRate = _hourlyRates.FirstOrDefault(r => string.Equals(r.RateName, "Administration", StringComparison.OrdinalIgnoreCase));
                if (adminRate != null)
                {
                    _lineItems[0].ItemDescription = "Administration";
                    _lineItems[0].HourlyQty = 1;
                    _lineItems[0].HourlyRate = adminRate.RateValue;
                }
            }

            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _variationId.HasValue ? "Edit Variation" : "New Variation";
            this.Size = new Size(1540, 880);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += VariationEditorForm_FormClosing;
            this.KeyPreview = true;
            this.KeyDown += VariationEditorForm_KeyDown;
            this.Resize += (s, e) => ApplyGridSizeAndLayout();
            // Re-run the wrap-based row sizing once the form is actually shown: the initial
            // call in LoadData() happens from the constructor, before the grid's Fill-mode
            // columns have undergone a real layout pass, so it can compute wrap height using
            // a not-yet-final column width - producing a too-short row that then looked
            // "collapsed" for any variation loaded with an existing multi-line description.
            this.Shown += (s, e) => dgvLineItems.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);
            // Clicking another control already ends the grid's edit mode via the normal
            // WinForms focus-change handling. Clicking the form's own blank background
            // doesn't - there's no control there to receive focus, so the grid never gets a
            // signal to commit. Handle it explicitly.
            this.MouseDown += (s, e) =>
            {
                if (dgvLineItems.IsCurrentCellInEditMode)
                {
                    dgvLineItems.EndEdit();
                }
            };

            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { /* fall back to the default WinForms icon if extraction fails */ }
            this.BackColor = Color.FromArgb(240, 240, 245);

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
                Size = new Size(labelWidth, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            txtVariationNumber = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, topMargin),
                Size = new Size(controlWidth, 20),
                Text = _variation.VariationNumber,
                Font = new Font("Arial", 10)
            };
            txtVariationNumber.TextChanged += Control_Changed;

            // Variation Name
            Label lblVariationName = new Label
            {
                Text = "Variation Name:",
                Location = new Point(leftMargin, topMargin + rowHeight),
                Size = new Size(labelWidth, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            txtVariationName = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, topMargin + rowHeight),
                Size = new Size(controlWidth, 20),
                Text = _variation.VariationName,
                Font = new Font("Arial", 10)
            };
            txtVariationName.TextChanged += Control_Changed;

            // Client Contact (moved to top row, right side)
            Label lblClientContact = new Label
            {
                Text = "Client Contact:",
                Location = new Point(leftMargin + 400, topMargin),
                Size = new Size(labelWidth, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            cboClientContact = new ComboBox
            {
                Location = new Point(leftMargin + 400 + labelWidth, topMargin),
                Size = new Size(controlWidth, 20),
                DropDownStyle = ComboBoxStyle.DropDown,
                Font = new Font("Arial", 10)
            };
            cboClientContact.SelectedIndexChanged += Control_Changed;
            cboClientContact.TextChanged += Control_Changed;

            // Variation Date (moved to second row, right side)
            Label lblVariationDate = new Label
            {
                Text = "Date:",
                Location = new Point(leftMargin + 400, topMargin + rowHeight),
                Size = new Size(labelWidth, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            dtpVariationDate = new DateTimePicker
            {
                Location = new Point(leftMargin + 400 + labelWidth, topMargin + rowHeight),
                Size = new Size(controlWidth, 20),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd-MM-yyyy",
                Font = new Font("Arial", 10)
            };
            DateTime.TryParse(_variation.VariationDate, out DateTime variationDate);
            dtpVariationDate.Value = variationDate == DateTime.MinValue ? DateTime.Now : variationDate;
            dtpVariationDate.ValueChanged += Control_Changed;

            // Created By (third row, left side - was previously empty space above the grid)
            Label lblCreatedBy = new Label
            {
                Text = "Created By:",
                Location = new Point(leftMargin, topMargin + rowHeight * 2),
                Size = new Size(labelWidth, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            cboCreatedBy = new ComboBox
            {
                Location = new Point(leftMargin + labelWidth, topMargin + rowHeight * 2),
                Size = new Size(controlWidth, 20),
                DropDownStyle = ComboBoxStyle.DropDown,
                Font = new Font("Arial", 10)
            };
            cboCreatedBy.SelectedIndexChanged += Control_Changed;
            cboCreatedBy.TextChanged += Control_Changed;

            // Line Items Grid
            dgvLineItems = new DataGridView
            {
                Location = new Point(leftMargin, topMargin + rowHeight * 3),
                Size = new Size(1350, 500),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                // AllCells here (the "ambient" mode) means the grid continuously re-measures
                // EVERY cell in a row - not just the one being edited - to determine the row's
                // height, on every single edit anywhere in that row. Since ItemDescription
                // still needs WrapMode=True, that column's wrap-measurement kept getting
                // re-triggered while typing in a completely different cell in the same row,
                // which is what caused the random-feeling black rendering across different
                // fields. Row growth for long descriptions is instead handled by explicit,
                // targeted AutoResizeRow calls (see DescriptionTextBox_TextChanged and
                // DgvLineItems_CellValueChanged) only when Description's own content changes.
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.False },
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            dgvLineItems.CellValueChanged += DgvLineItems_CellValueChanged;
            dgvLineItems.CurrentCellDirtyStateChanged += DgvLineItems_CurrentCellDirtyStateChanged;
            dgvLineItems.CellPainting += DgvLineItems_CellPainting;
            dgvLineItems.CellClick += DgvLineItems_CellClick;
            dgvLineItems.EditingControlShowing += DgvLineItems_EditingControlShowing;
            dgvLineItems.CellFormatting += DgvLineItems_CellFormatting;
            dgvLineItems.KeyDown += DgvLineItems_KeyDown;
            EnableDoubleBuffering(dgvLineItems);

            // Scope of Works and Exclusions, side by side (between the grid and the Add/Move
            // buttons). Left/Width are recomputed in ApplyGridSizeAndLayout, since a fixed
            // 50/50 split needs to be re-derived whenever the grid's width changes.
            lblScopeOfWorks = new Label
            {
                Text = "Scope of Works:",
                Location = new Point(leftMargin, topMargin + rowHeight * 3 + 510),
                Size = new Size(labelWidth, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            txtScopeOfWorks = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, topMargin + rowHeight * 3 + 510),
                Size = new Size(400, NotesHeight),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Arial", 9),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            txtScopeOfWorks.TextChanged += Control_Changed;

            lblExclusions = new Label
            {
                Text = "Exclusions:",
                Location = new Point(leftMargin + 700, topMargin + rowHeight * 3 + 510),
                Size = new Size(labelWidth, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            txtExclusions = new TextBox
            {
                Location = new Point(leftMargin + 700 + labelWidth, topMargin + rowHeight * 3 + 510),
                Size = new Size(400, NotesHeight),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Arial", 9),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            txtExclusions.TextChanged += Control_Changed;

            // Add Row Button
            btnAddRow = new Button
            {
                Text = "Add Row",
                Location = new Point(leftMargin, topMargin + rowHeight * 3 + 510),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(60, 179, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnAddRow.FlatAppearance.BorderSize = 0;
            btnAddRow.Click += BtnAddRow_Click;

            // Delete Row Button
            btnDeleteRow = new Button
            {
                Text = "Delete Row",
                Location = new Point(leftMargin + 360, topMargin + rowHeight * 3 + 510),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(178, 34, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnDeleteRow.FlatAppearance.BorderSize = 0;
            btnDeleteRow.Click += BtnDeleteRow_Click;

            // Move Up Button
            btnMoveUp = new Button
            {
                Text = "Move Up ↑",
                Location = new Point(leftMargin + 110, topMargin + rowHeight * 3 + 510),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(100, 149, 237),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnMoveUp.FlatAppearance.BorderSize = 0;
            btnMoveUp.Click += BtnMoveUp_Click;

            // Move Down Button
            btnMoveDown = new Button
            {
                Text = "Move Down ↓",
                Location = new Point(leftMargin + 220, topMargin + rowHeight * 3 + 510),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(100, 149, 237),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnMoveDown.FlatAppearance.BorderSize = 0;
            btnMoveDown.Click += BtnMoveDown_Click;

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
                Text = "💾 Save (Ctrl+S)",
                Location = new Point(leftMargin + 450, topMargin + rowHeight * 3 + 590),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(leftMargin + 580, topMargin + rowHeight * 3 + 590),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(128, 128, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += BtnClose_Click;

            btnPrint = new Button
            {
                Text = "📄 Print PDF",
                Location = new Point(leftMargin + 710, topMargin + rowHeight * 3 + 590),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += BtnPrint_Click;

            // Save status label
            lblSaveStatus = new Label
            {
                Location = new Point(leftMargin + 450, topMargin + rowHeight * 3 + 560),
                Size = new Size(150, 25),
                Font = new Font("Arial", 11, FontStyle.Bold),
                ForeColor = Color.Green,
                Text = "",
                Visible = false,
                BackColor = Color.Transparent
            };

            // Timer for hiding save status
            saveStatusTimer = new Timer { Interval = 2000 };
            saveStatusTimer.Tick += (s, e) =>
            {
                lblSaveStatus.Visible = false;
                saveStatusTimer.Stop();
            };

            this.Controls.Add(lblVariationNumber);
            this.Controls.Add(txtVariationNumber);
            this.Controls.Add(lblVariationName);
            this.Controls.Add(txtVariationName);
            this.Controls.Add(lblVariationDate);
            this.Controls.Add(dtpVariationDate);
            this.Controls.Add(lblClientContact);
            this.Controls.Add(cboClientContact);
            this.Controls.Add(lblCreatedBy);
            this.Controls.Add(cboCreatedBy);
            this.Controls.Add(dgvLineItems);
            this.Controls.Add(lblScopeOfWorks);
            this.Controls.Add(txtScopeOfWorks);
            this.Controls.Add(lblExclusions);
            this.Controls.Add(txtExclusions);
            this.Controls.Add(btnAddRow);
            this.Controls.Add(btnDeleteRow);
            this.Controls.Add(btnMoveUp);
            this.Controls.Add(btnMoveDown);
            this.Controls.Add(lblMaterialSubtotal);
            this.Controls.Add(lblLabourSubtotal);
            this.Controls.Add(lblGrandTotal);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnClose);
            this.Controls.Add(btnPrint);
            this.Controls.Add(lblSaveStatus);

            ApplyGridSizeAndLayout();
        }

        private void ApplyGridSizeAndLayout()
        {
            int availableHeight = this.ClientSize.Height - dgvLineItems.Top - ReservedHeightBelowGrid;
            dgvLineItems.Height = Math.Max(MinGridHeight, availableHeight);

            int gridBottom = dgvLineItems.Top + dgvLineItems.Height;

            // Scope of Works (left half) and Exclusions (right half), split evenly across
            // the grid's current width with a gap between them.
            int rowTop = gridBottom + GapGridToNotes;
            int gridLeft = dgvLineItems.Left;
            int labelWidth = lblScopeOfWorks.Width;
            int halfWidth = (dgvLineItems.Width - NotesColumnGap) / 2;

            lblScopeOfWorks.Top = rowTop;
            txtScopeOfWorks.Top = rowTop;
            lblScopeOfWorks.Left = gridLeft;
            txtScopeOfWorks.Left = gridLeft + labelWidth;
            txtScopeOfWorks.Width = halfWidth - labelWidth;

            int rightHalfLeft = gridLeft + halfWidth + NotesColumnGap;
            lblExclusions.Top = rowTop;
            txtExclusions.Top = rowTop;
            lblExclusions.Left = rightHalfLeft;
            txtExclusions.Left = rightHalfLeft + labelWidth;
            txtExclusions.Width = halfWidth - labelWidth;

            int notesBottom = rowTop + NotesHeight;

            btnAddRow.Top = notesBottom + GridBottomOffsetMoveButtons;
            btnDeleteRow.Top = notesBottom + GridBottomOffsetMoveButtons;
            btnMoveUp.Top = notesBottom + GridBottomOffsetMoveButtons;
            btnMoveDown.Top = notesBottom + GridBottomOffsetMoveButtons;

            lblMaterialSubtotal.Top = notesBottom + GridBottomOffsetMaterialSubtotal;
            lblLabourSubtotal.Top = notesBottom + GridBottomOffsetLabourSubtotal;
            lblGrandTotal.Top = notesBottom + GridBottomOffsetGrandTotal;

            lblSaveStatus.Top = notesBottom + GridBottomOffsetSaveStatus;
            btnSave.Top = notesBottom + GridBottomOffsetActionButtons;
            btnClose.Top = notesBottom + GridBottomOffsetActionButtons;
            btnPrint.Top = notesBottom + GridBottomOffsetActionButtons;
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

            // Load staff names for Created By
            _staffNames = _dbHelper.GetStaffNames();
            cboCreatedBy.Items.Clear();
            foreach (var name in _staffNames)
            {
                cboCreatedBy.Items.Add(name);
            }

            if (!string.IsNullOrEmpty(_variation.CreatedBy))
            {
                cboCreatedBy.Text = _variation.CreatedBy;
            }

            txtScopeOfWorks.Text = _variation.Notes ?? "";
            txtExclusions.Text = _variation.Exclusions ?? "";

            // Setup grid columns
            dgvLineItems.Columns.Clear();

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ItemNumber",
                HeaderText = "Item #",
                FillWeight = 60,
                ReadOnly = true
            });

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ItemDescription",
                HeaderText = "Description",
                FillWeight = 350,
                DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True },
                CellTemplate = new TextEditCell()
            });

            var typeColumn = new DataGridViewComboBoxColumn
            {
                Name = "ItemType",
                HeaderText = "Type",
                FillWeight = 90,
                DataSource = new[] { "Cost", "Refund" },
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(230, 240, 255) }
            };
            dgvLineItems.Columns.Add(typeColumn);

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MaterialQty",
                HeaderText = "Mat. Qty",
                FillWeight = 80,
                CellTemplate = new TextEditCell()
            });

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MaterialCost",
                HeaderText = "Mat. Cost",
                FillWeight = 100,
                CellTemplate = new TextEditCell()
            });

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MaterialTotal",
                HeaderText = "Mat. Total",
                FillWeight = 110,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 255, 240), Format = "C2" }
            });

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "HourlyQty",
                HeaderText = "Hours",
                FillWeight = 80,
                CellTemplate = new TextEditCell()
            });

            // Create hourly rate dropdown with rates and custom option
            var rateOptions = _hourlyRates.Select(r => $"{r.RateName} (${r.RateValue:F2})").ToList();
            rateOptions.Add("Custom");
            var hourlyRateColumn = new DataGridViewComboBoxColumn
            {
                Name = "HourlyRate",
                HeaderText = "Hourly Rate",
                FillWeight = 158,
                DataSource = rateOptions.ToArray(),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(255, 250, 230),
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    // Grid-level WrapMode=True (needed for Description) would otherwise be
                    // inherited here, growing the row to fit the raw untruncated text - the
                    // CellFormatting ellipsis logic below doesn't factor into row auto-sizing.
                    WrapMode = DataGridViewTriState.False
                }
            };
            dgvLineItems.Columns.Add(hourlyRateColumn);

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CustomRate",
                HeaderText = "Custom Rate",
                FillWeight = 110,
                CellTemplate = new TextEditCell()
            });

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LabourTotal",
                HeaderText = "Labour Total",
                FillWeight = 120,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 248, 255), Format = "C2" }
            });

            dgvLineItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LineTotal",
                HeaderText = "Line Total",
                FillWeight = 120,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(255, 245, 230), Format = "C2" }
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

            dgvLineItems.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);
            UpdateTotals();
            ComputeDataHash();
        }

        private void ComputeDataHash()
        {
            // Create a simple hash of all data to detect real changes
            var hashData = $"{txtVariationNumber.Text}|{txtVariationName.Text}|{dtpVariationDate.Value}|{cboClientContact.Text}|{cboCreatedBy.Text}|{txtScopeOfWorks.Text}|{txtExclusions.Text}|";
            foreach (DataGridViewRow row in dgvLineItems.Rows)
            {
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    hashData += row.Cells[i].Value?.ToString() ?? "";
                    hashData += "|";
                }
            }
            _originalDataHash = hashData.GetHashCode().ToString();
            _hasUnsavedChanges = false;
        }

        private bool HasActualChanges()
        {
            var currentHash = $"{txtVariationNumber.Text}|{txtVariationName.Text}|{dtpVariationDate.Value}|{cboClientContact.Text}|{cboCreatedBy.Text}|{txtScopeOfWorks.Text}|{txtExclusions.Text}|";
            foreach (DataGridViewRow row in dgvLineItems.Rows)
            {
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    currentHash += row.Cells[i].Value?.ToString() ?? "";
                    currentHash += "|";
                }
            }
            return currentHash.GetHashCode().ToString() != _originalDataHash;
        }

        private static void EnableDoubleBuffering(DataGridView grid)
        {
            // DataGridView's own double buffering doesn't fully cover rapid repaints
            // triggered while a cell is actively being edited (row auto-sizing, cell
            // formatting, etc.), which is what caused cells to render solid black mid-edit
            // until a different cell was clicked. DoubleBuffered is a protected property on
            // Control, so it has to be set via reflection.
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null,
                grid,
                new object[] { true });
        }

        private void DgvLineItems_KeyDown(object? sender, KeyEventArgs e)
        {
            // Delete clears a selected text cell's content without needing to enter edit
            // mode first (matches common spreadsheet behavior).
            if (e.KeyCode != Keys.Delete || dgvLineItems.IsCurrentCellInEditMode) return;

            foreach (DataGridViewCell cell in dgvLineItems.SelectedCells)
            {
                if (!cell.ReadOnly && cell is DataGridViewTextBoxCell)
                {
                    cell.Value = "";
                }
            }
            e.Handled = true;
        }

        private void DgvLineItems_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            // Only combo box columns (ItemType, HourlyRate) need the value committed
            // immediately, so selecting a new item updates dependent calculations right away.
            // Forcing this on every keystroke for plain text columns too (MaterialQty,
            // MaterialCost, etc.) triggered a commit + full row recalculation on every
            // character typed while the cell was still mid-edit, which corrupted the cell's
            // rendering (it would paint solid black until a different cell was clicked).
            if (dgvLineItems.IsCurrentCellDirty && dgvLineItems.CurrentCell?.OwningColumn is DataGridViewComboBoxColumn)
            {
                dgvLineItems.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvLineItems_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvLineItems.Rows[e.RowIndex];

            // Re-wrap and resize the row for the committed description text (covers paste,
            // programmatic edits, etc. in addition to the live-typing handler)
            if (e.ColumnIndex == dgvLineItems.Columns["ItemDescription"].Index)
            {
                dgvLineItems.AutoResizeRow(e.RowIndex, DataGridViewAutoSizeRowMode.AllCells);
            }

            // Format MaterialCost with dollar sign
            if (e.ColumnIndex == dgvLineItems.Columns["MaterialCost"].Index)
            {
                FormatCurrencyInput(row.Cells["MaterialCost"]);
            }

            // Format CustomRate with dollar sign
            if (e.ColumnIndex == dgvLineItems.Columns["CustomRate"].Index)
            {
                FormatCurrencyInput(row.Cells["CustomRate"]);
            }

            // Update custom rate state when hourly rate changes
            if (e.ColumnIndex == dgvLineItems.Columns["HourlyRate"].Index)
            {
                UpdateCustomRateState(e.RowIndex);
            }

            // Parse numeric values
            decimal matQty = ParseDecimal(row.Cells["MaterialQty"].Value);
            decimal matCost = ParseDecimalFromCurrency(row.Cells["MaterialCost"].Value);
            decimal hourQty = ParseDecimal(row.Cells["HourlyQty"].Value);
            decimal hourRate = 0;

            // Get hourly rate
            string rateSelection = row.Cells["HourlyRate"].Value?.ToString() ?? "";
            if (rateSelection == "Custom")
            {
                hourRate = ParseDecimalFromCurrency(row.Cells["CustomRate"].Value);
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

        private void FormatCurrencyInput(DataGridViewCell cell)
        {
            if (cell.Value == null) return;
            string strValue = cell.Value.ToString()!;

            // Remove existing $ signs and parse
            strValue = strValue.Replace("$", "").Trim();
            if (decimal.TryParse(strValue, out decimal value) && value != 0)
            {
                // Add $ sign but don't show decimals unless entered
                if (strValue.Contains("."))
                {
                    cell.Value = "$" + value.ToString("0.##");
                }
                else
                {
                    cell.Value = "$" + value.ToString("0");
                }
            }
        }

        private decimal ParseDecimalFromCurrency(object? value)
        {
            if (value == null) return 0;
            string strValue = value.ToString()!.Replace("$", "").Trim();
            if (decimal.TryParse(strValue, out decimal result))
                return result;
            return 0;
        }

        private void DgvLineItems_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Highlight MaterialQty cell if empty (lighter red)
            if (e.ColumnIndex == dgvLineItems.Columns["MaterialQty"].Index)
            {
                var value = dgvLineItems.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    e.CellStyle.BackColor = Color.FromArgb(255, 200, 200);
                }
            }
        }

        private void DgvLineItems_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvLineItems.Columns[e.ColumnIndex].Name != "HourlyRate") return;
            if (e.Value == null) return;

            string text = e.Value.ToString() ?? "";
            int markerIndex = text.IndexOf("($", StringComparison.Ordinal);
            if (markerIndex <= 0) return;

            string suffix = text.Substring(markerIndex); // e.g. "($161.00)"
            string prefix = text.Substring(0, markerIndex);
            Font font = e.CellStyle?.Font ?? dgvLineItems.Font;

            // Reserve room for the combo box's dropdown arrow button plus cell padding,
            // not just the text itself, or borderline-length values overflow past the arrow.
            // Kept tight since the goal is fitting as much of the prefix as possible.
            const int comboButtonWidth = 18;
            const int cellPadding = 4;
            int availableWidth = dgvLineItems.Columns[e.ColumnIndex].Width - comboButtonWidth - cellPadding;

            if (MeasureTextWidth(text, font) <= availableWidth)
                return;

            const string ellipsis = "..";
            int budget = availableWidth - MeasureTextWidth(suffix, font) - MeasureTextWidth(ellipsis, font);

            if (budget <= 0)
            {
                e.Value = ellipsis + suffix;
                e.FormattingApplied = true;
                return;
            }

            string truncatedPrefix = prefix;
            while (truncatedPrefix.Length > 0 && MeasureTextWidth(truncatedPrefix, font) > budget)
            {
                truncatedPrefix = truncatedPrefix.Substring(0, truncatedPrefix.Length - 1);
            }

            e.Value = truncatedPrefix.TrimEnd() + ellipsis + suffix;
            e.FormattingApplied = true;
        }

        private static int MeasureTextWidth(string text, Font font)
        {
            // TextRenderer.MeasureText's default overload adds internal padding, which left
            // a visible gap once the text is right-aligned. NoPadding gives a tighter,
            // more accurate width so more of the prefix text can be kept.
            // Size.Empty (not a huge proposed size like int.MaxValue) is the documented way
            // to ask for the text's natural unwrapped width - a large proposed size here was
            // producing wrong (too-small) measurements for longer rate names, letting them
            // slip past the "does it fit" check and wrap into a second line instead of
            // getting truncated.
            return TextRenderer.MeasureText(text, font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width;
        }

        private void DgvLineItems_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            // Immediately open dropdown for combo box columns. Opening it is handled by
            // EditingControlShowing (deferred there to avoid a positioning race), so this
            // just needs to start the edit.
            if (dgvLineItems.Columns[e.ColumnIndex] is DataGridViewComboBoxColumn)
            {
                dgvLineItems.BeginEdit(true);
            }

            // Handle Custom Rate enable/disable
            UpdateCustomRateState(e.RowIndex);
        }

        private void DgvLineItems_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // Force the editing control's colors to match the cell it's editing. Without
            // this, Windows can apply OS-level dark-mode theming to the native edit control
            // (black background, white text) independently of the DataGridView's own
            // light-themed cells drawn around it - inconsistently, which is why it looked
            // random. Explicit colors here take precedence over that theming.
            var cellStyle = dgvLineItems.CurrentCell?.InheritedStyle;
            if (e.Control is Control editingControl)
            {
                editingControl.BackColor = cellStyle?.BackColor ?? Color.White;
                editingControl.ForeColor = Color.Black;
            }

            // Auto-open dropdown when editing combobox. The shared editing control isn't
            // repositioned to the new cell until after this event returns, so opening it
            // immediately drops it down at the previous cell's stale location. Defer until
            // the control has been moved and painted at the correct spot.
            if (e.Control is ComboBox combo)
            {
                combo.BeginInvoke(new Action(() =>
                {
                    if (!combo.IsDisposed)
                    {
                        combo.DroppedDown = true;
                    }
                }));
            }

            // Grow the row in real time as wrapped description text is typed. The editing
            // control is reused across cells, so unsubscribe first to avoid stacking handlers.
            if (e.Control is TextBox textBox && dgvLineItems.CurrentCell?.OwningColumn?.Name == "ItemDescription")
            {
                textBox.TextChanged -= DescriptionTextBox_TextChanged;
                textBox.TextChanged += DescriptionTextBox_TextChanged;
            }
        }

        private void DescriptionTextBox_TextChanged(object? sender, EventArgs e)
        {
            int rowIndex = dgvLineItems.CurrentCell?.RowIndex ?? -1;
            if (rowIndex < 0) return;

            dgvLineItems.NotifyCurrentCellDirty(true);
            dgvLineItems.AutoResizeRow(rowIndex, DataGridViewAutoSizeRowMode.AllCells);
        }

        private void UpdateCustomRateState(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvLineItems.Rows.Count) return;

            var row = dgvLineItems.Rows[rowIndex];
            string rateSelection = row.Cells["HourlyRate"].Value?.ToString() ?? "";
            bool isCustom = rateSelection == "Custom";

            // Enable/disable custom rate cell
            row.Cells["CustomRate"].ReadOnly = !isCustom;
            row.Cells["CustomRate"].Style.BackColor = isCustom ? Color.White : Color.FromArgb(240, 240, 240);
            row.Cells["CustomRate"].Style.ForeColor = isCustom ? Color.Black : Color.Gray;
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

        private void BtnDeleteRow_Click(object? sender, EventArgs e)
        {
            if (dgvLineItems.CurrentRow == null)
            {
                MessageBox.Show("Please select a line item to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to delete this line item?",
                "Delete Line Item",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            dgvLineItems.Rows.RemoveAt(dgvLineItems.CurrentRow.Index);

            RenumberItems();
            UpdateTotals();
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

            if (string.IsNullOrWhiteSpace(cboClientContact.Text))
            {
                MessageBox.Show("Please enter a client contact.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(cboCreatedBy.Text))
            {
                MessageBox.Show("Please enter who is creating this variation.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check for duplicate variation number (only numbers need to be unique, not names)
            if (_dbHelper.VariationNumberExists(txtVariationNumber.Text, _variationId))
            {
                MessageBox.Show("A variation with this number already exists.", "Duplicate Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Update variation object
            _variation.VariationNumber = txtVariationNumber.Text.Trim();
            _variation.VariationName = txtVariationName.Text.Trim();
            _variation.VariationDate = dtpVariationDate.Value.ToString("dd-MM-yyyy");
            _variation.ClientContact = cboClientContact.Text.Trim();
            _variation.CreatedBy = cboCreatedBy.Text.Trim();
            _variation.Notes = txtScopeOfWorks.Text.Trim();
            _variation.Exclusions = txtExclusions.Text.Trim();

            // Collect line items from grid
            var lineItems = new List<LineItem>();
            decimal totalValue = 0;

            foreach (DataGridViewRow row in dgvLineItems.Rows)
            {
                int itemNumber = (int)ParseDecimal(row.Cells["ItemNumber"].Value);
                if (itemNumber == 0) continue;

                decimal matQty = ParseDecimal(row.Cells["MaterialQty"].Value);
                decimal matCost = ParseDecimalFromCurrency(row.Cells["MaterialCost"].Value);
                decimal hourQty = ParseDecimal(row.Cells["HourlyQty"].Value);
                decimal hourRate = 0;

                string rateSelection = row.Cells["HourlyRate"].Value?.ToString() ?? "";
                if (rateSelection == "Custom")
                {
                    hourRate = ParseDecimalFromCurrency(row.Cells["CustomRate"].Value);
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
                _variation.Id = savedId;  // Important: Update the variation object's ID
                _hasUnsavedChanges = false;
                ComputeDataHash();

                string outputFolder = System.IO.Directory.GetCurrentDirectory();
                new PdfGenerator(_dbHelper, outputFolder).EnsureVariationFolder(_variation);

                // Show temporary "Saved" message
                lblSaveStatus.Text = "Saved ✓";
                lblSaveStatus.Visible = true;
                saveStatusTimer.Stop();
                saveStatusTimer.Start();
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

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_variationId == null)
                {
                    MessageBox.Show("Please save the variation first before printing.", "Cannot Print",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string outputFolder = System.IO.Directory.GetCurrentDirectory();
                var pdfGenerator = new PdfGenerator(_dbHelper, outputFolder);
                string filePath = pdfGenerator.GenerateVariationPdf(_variationId.Value);

                MessageBox.Show($"Variation PDF generated successfully!\n\nSaved to: {filePath}", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

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

        private void VariationEditorForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Check if there are actual changes, not just the flag
            if (HasActualChanges())
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Do you want to save before closing?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    BtnSave_Click(sender, e);
                    if (HasActualChanges()) // Save failed
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

        private void RenumberItems()
        {
            for (int i = 0; i < dgvLineItems.Rows.Count; i++)
            {
                dgvLineItems.Rows[i].Cells["ItemNumber"].Value = i + 1;
            }
        }

        private void BtnMoveUp_Click(object? sender, EventArgs e)
        {
            if (dgvLineItems.CurrentRow == null || dgvLineItems.CurrentRow.Index == 0)
                return;

            int currentIndex = dgvLineItems.CurrentRow.Index;

            // Swap rows
            var row1 = dgvLineItems.Rows[currentIndex];
            var row2 = dgvLineItems.Rows[currentIndex - 1];
            SwapRows(row1, row2);

            // Move selection
            dgvLineItems.CurrentCell = dgvLineItems.Rows[currentIndex - 1].Cells[1];

            // Renumber all items
            RenumberItems();
            UpdateTotals();
            _hasUnsavedChanges = true;
        }

        private void BtnMoveDown_Click(object? sender, EventArgs e)
        {
            if (dgvLineItems.CurrentRow == null || dgvLineItems.CurrentRow.Index >= dgvLineItems.Rows.Count - 1)
                return;

            int currentIndex = dgvLineItems.CurrentRow.Index;

            // Swap rows
            var row1 = dgvLineItems.Rows[currentIndex];
            var row2 = dgvLineItems.Rows[currentIndex + 1];
            SwapRows(row1, row2);

            // Move selection
            dgvLineItems.CurrentCell = dgvLineItems.Rows[currentIndex + 1].Cells[1];

            // Renumber all items
            RenumberItems();
            UpdateTotals();
            _hasUnsavedChanges = true;
        }

        private void SwapRows(DataGridViewRow row1, DataGridViewRow row2)
        {
            for (int i = 0; i < row1.Cells.Count; i++)
            {
                if (dgvLineItems.Columns[i].Name != "ItemNumber")
                {
                    object? temp = row1.Cells[i].Value;
                    row1.Cells[i].Value = row2.Cells[i].Value;
                    row2.Cells[i].Value = temp;
                }
            }
        }
    }

    // By default, DataGridView intercepts arrow/navigation keys for cell-to-cell movement
    // even while a cell is being edited, rather than passing them to the text box for normal
    // cursor movement/selection. The correct extension point to change that is
    // EditingControlWantsInputKey (checked by the grid before its own key handling) - a
    // generic Control.PreviewKeyDown handler runs too late to affect this.
    internal class TextEditingControl : DataGridViewTextBoxEditingControl
    {
        public override bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Home:
                case Keys.End:
                case Keys.PageUp:
                case Keys.PageDown:
                    return true;
                default:
                    return base.EditingControlWantsInputKey(keyData, dataGridViewWantsInputKey);
            }
        }
    }

    internal class TextEditCell : DataGridViewTextBoxCell
    {
        public override Type EditType => typeof(TextEditingControl);
    }
}
