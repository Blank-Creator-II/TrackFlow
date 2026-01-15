using System.Globalization;
using MaterialSkin;
using MaterialSkin.Controls;
using TrackFlow.Models;
using TrackFlow.Service;
using TrackFlow.Utils;

namespace TrackFlow.Forms;
public partial class ExpensesPage : UserControl
{
    private class AddExpense : MaterialForm
    {
        public TableLayoutPanel Table { get; private set; }

        // Inputs
        private MaterialTextBox txtAmount;
        private MaterialComboBox cmbMode;
        private MaterialTextBox txtReceiver;
        private MaterialComboBox cmbCategory;
        private MaterialComboBox cmbCurrency;

        // Coupon
        private MaterialTextBox txtCouponCode;
        private MaterialTextBox2 txtCouponDesc;
        private MaterialTextBox txtCouponStore;
        private TableLayoutPanel dtpCouponExp;
        private MaterialComboBox ExpMonth;
        private MaterialComboBox ExpDay;
        private MaterialComboBox ExpYear;

        // Bank
        private MaterialTextBox txtBankName;
        private MaterialComboBox cmbBankAccountType;
        private MaterialTextBox txtBankAccountId;
        private MaterialTextBox txtBankBalance;
        private TableLayoutPanel dtpBankLinkDate;
        private MaterialComboBox LinkMonth;
        private MaterialComboBox LinkDay;
        private MaterialComboBox LinkYear;

        private MaterialButton btnSave;

        public AddExpense()
        {
            // Setup MaterialSkin
            var skinManager = MaterialSkinManager.Instance;
            skinManager.AddFormToManage(this);

            Text = "Add Expense";
            Size = new Size(700, 670);
            MinimumSize = new Size(700, 670);
            StartPosition = FormStartPosition.CenterScreen;

            // Use a plain scrollable panel that fills the form
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(8),
            };
            this.Controls.Add(mainPanel);

            // Table setup - keep Dock=Top and we'll size Width to the panel client width
            Table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,       // top so it flows in the panel and keeps its width
                ColumnCount = 2,
                RowCount = 12,
                AutoSize = false,
                Padding = new Padding(10),
            };

            Table.ColumnStyles.Clear();
            Table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            Table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            // Explicit row heights
            Table.RowStyles.Clear();
            Table.RowStyles.Add(new RowStyle(SizeType.Absolute, 12f));  // 0: divider
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 11f));  // 1
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 11f));  // 2
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 11f));  // 3
            Table.RowStyles.Add(new RowStyle(SizeType.Absolute, 12f));  // 4
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 11f));  // 5
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 11f));  // 6
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 11f));  // 7
            Table.RowStyles.Add(new RowStyle(SizeType.Absolute, 12f));  // 8
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 11f));  // 9
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 11f));  // 10
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 12f));  // 11

            // Add table to the scroll panel
            mainPanel.Controls.Add(Table);

            // Important: size table to panel's client width (account for scrollbar width)
            void ResizeTableToPanel()
            {
                var scrollBarWidth = SystemInformation.VerticalScrollBarWidth;
                Table.Width = Math.Max(300, mainPanel.ClientSize.Width - scrollBarWidth - mainPanel.Padding.Horizontal);
            }

            // handle initial sizing and panel resize
            mainPanel.Resize += (s, e) => ResizeTableToPanel();
            // call once to set initial width
            ResizeTableToPanel();

            // ------- Controls population -------
            // Divider 1
            var Hdivider1 = new MaterialDivider
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                BackColor = MainForm.PrimaryDark
            };
            Table.Controls.Add(Hdivider1, 0, 0);
            Table.SetColumnSpan(Hdivider1, 2);

            // Amount (r:1 c:0)
            txtAmount = new MaterialTextBox
            {
                Hint = "Enter Amount",
                UseTallSize = true,
                Margin = new Padding(2),
                Dock = DockStyle.Fill
            };
            Table.Controls.Add(txtAmount, 0, 1);

            // Mode (r:1 c:1)
            cmbMode = new MaterialComboBox
            {
                Hint = "Chose Mode", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            cmbMode.Items.AddRange(new object[] { "Splitted", "Individual", "Discounted" });
            Table.Controls.Add(cmbMode, 1, 1);

            // Receiver (r:2 c:a)
            txtReceiver = new MaterialTextBox 
            { 
                Hint = "Enter Receiver", 
                UseTallSize = true, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            Table.Controls.Add(txtReceiver, 0, 2);
            Table.SetColumnSpan(txtReceiver, 2);

            // Category (r:3 c:0)
            cmbCategory = new MaterialComboBox 
            { 
                Hint = "Chose Category", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), Dock = DockStyle.Fill };
            cmbCategory.Items.AddRange(new object[] { "Travel", "Grocery", "Medicine", "Others" });
            Table.Controls.Add(cmbCategory, 0, 3);

            // Currency (r:3 c:1)
            cmbCurrency = new MaterialComboBox 
            { 
                Hint = "Chose Currency", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill };
            cmbCurrency.Items.AddRange(new object[] { "Dollar", "Yen", "Euro", "Birr" });
            Table.Controls.Add(cmbCurrency, 1, 3);

            // Divider 2
            var Hdivider2 = new MaterialDivider 
            { 
                Dock = DockStyle.Fill, 
                Margin = new Padding(2), 
                BackColor = MainForm.PrimaryDark 
            };
            Table.Controls.Add(Hdivider2, 0, 4);
            Table.SetColumnSpan(Hdivider2, 2);

            // Coupon Code (r:5 c:a)
            txtCouponCode = new MaterialTextBox 
            { 
                Hint = "Enter Coupon", 
                UseTallSize = true, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            Table.Controls.Add(txtCouponCode, 0, 5);
            Table.SetColumnSpan(txtCouponCode, 2);

            // Coupon Description (r:6 c:a)
            txtCouponDesc = new MaterialTextBox2 
            { 
                Hint = "Enter Coupon Description",  
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            Table.Controls.Add(txtCouponDesc, 0, 6);
            Table.SetColumnSpan(txtCouponDesc, 2);

            // Coupon Store (r:7 c:0)
            txtCouponStore = new MaterialTextBox 
            { 
                Hint = "Enter Coupon's Usage Store", 
                UseTallSize = true, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            Table.Controls.Add(txtCouponStore, 0, 7);

            // Coupon Expiration (r:7 c:1)
            dtpCouponExp = new TableLayoutPanel 
            {
                Margin = new Padding(2), 
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            Table.Controls.Add(dtpCouponExp, 1, 7);

            dtpCouponExp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            dtpCouponExp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            dtpCouponExp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            dtpCouponExp.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            ExpMonth = new MaterialComboBox 
            { 
                Hint = "MM", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            ExpMonth.Items.AddRange(new object[] { 1,2,3,4,5,6,7,8,9,10,11,12 });
            dtpCouponExp.Controls.Add(ExpMonth,0,0);

            ExpDay = new MaterialComboBox 
            { 
                Hint = "DD", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            ExpDay.Items.AddRange(new object[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,
                                        16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31 });
            dtpCouponExp.Controls.Add(ExpDay,1,0);

            ExpYear = new MaterialComboBox 
            { 
                Hint = "YY", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            ExpYear.Items.AddRange(new object[] { 90,91,92,93,94,95,96,97,98,99,20,21,22,23,24,25,
                                                   26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,
                                                   42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,67,
                                                   68,69,60,61,62,63,64,65,66,67,68,69,70,71,72,73,
                                                   74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89});
            dtpCouponExp.Controls.Add(ExpYear,2,0);

            // Divider 3
            var Hdivider3 = new MaterialDivider 
            { 
                Dock = DockStyle.Fill, 
                Margin = new Padding(2), 
                BackColor = MainForm.PrimaryDark };
            Table.Controls.Add(Hdivider3, 0, 8);
            Table.SetColumnSpan(Hdivider3, 2);

            // Bank Name (r:9 c:0)
            txtBankName = new MaterialTextBox 
            { 
                Hint = "Enter Bank's Name", 
                UseTallSize = true, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            Table.Controls.Add(txtBankName, 0, 9);

            // Account Type (r:9 c:1)
            cmbBankAccountType = new MaterialComboBox 
            { 
                Hint = "Chose Account Type", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            cmbBankAccountType.Items.AddRange(new object[] { "Checking", "Saving", "Shared", "Credit" });
            Table.Controls.Add(cmbBankAccountType, 1, 9);

            // Account ID (r:10 c:0)
            txtBankAccountId = new MaterialTextBox 
            { 
                Hint = "Enter Account ID", 
                UseTallSize = true, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            Table.Controls.Add(txtBankAccountId, 0, 10);

            // Bank Balance (r:10 c:1)
            txtBankBalance = new MaterialTextBox 
            { 
                Hint = "Enter Bank Balance", 
                UseTallSize = true, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill };
            Table.Controls.Add(txtBankBalance, 1, 10);

            // Bank Link Date (r:11 c:0)
            dtpBankLinkDate =  new TableLayoutPanel 
            {
                Margin = new Padding(2), 
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            Table.Controls.Add(dtpBankLinkDate, 0, 11);

            dtpBankLinkDate.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            dtpBankLinkDate.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            dtpBankLinkDate.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            dtpBankLinkDate.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            LinkMonth = new MaterialComboBox 
            { 
                Hint = "MM", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            LinkMonth.Items.AddRange(new object[] { 1,2,3,4,5,6,7,8,9,10,11,12 });
            dtpBankLinkDate.Controls.Add(LinkMonth,0,0);

            LinkDay = new MaterialComboBox 
            { 
                Hint = "DD", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            LinkDay.Items.AddRange(new object[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,
                                        16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31 });
            dtpBankLinkDate.Controls.Add(LinkDay,1,0);

            LinkYear = new MaterialComboBox 
            { 
                Hint = "YY", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            LinkYear.Items.AddRange(new object[] { 90,91,92,93,94,95,96,97,98,99,20,21,22,23,24,25,
                                                   26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,
                                                   42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,67,
                                                   68,69,60,61,62,63,64,65,66,67,68,69,70,71,72,73,
                                                   74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89});
            dtpBankLinkDate.Controls.Add(LinkYear,2,0);

            // Save button (r:11 c:1)
            btnSave = new MaterialButton 
            {
                Text = "Save Expense",
                Margin = new Padding(2,2,2,13),
                Dock = DockStyle.Fill,
            };
            btnSave.Click += (s, e) => BtnSave_Click();
            Table.Controls.Add(btnSave, 1, 11);
        }

        private bool TryParseDateFromCombos(MaterialComboBox monthCb, MaterialComboBox dayCb, MaterialComboBox yearCb, out DateTime result)
        {
            result = default;

            if (monthCb?.SelectedItem == null || dayCb?.SelectedItem == null || yearCb?.SelectedItem == null)
                return false;

            if (!int.TryParse(monthCb.SelectedItem.ToString(), out int month) ||
                !int.TryParse(dayCb.SelectedItem.ToString(), out int day) ||
                !int.TryParse(yearCb.SelectedItem.ToString(), out int year))
                return false;

            // Map two-digit years to a reasonable century:
            // 90-99 => 1990-1999, 0-89 => 2000-2089
            if (year >= 90 && year <= 99) year += 1900;
            else if (year >= 0 && year <= 89 && year < 100) year += 2000;

            try
            {
                result = new DateTime(year, month, day);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void BtnSave_Click()
        {
            try
            {
                // 1) Amount
                if (!double.TryParse(txtAmount.Text?.Trim(), NumberStyles.Float | NumberStyles.AllowThousands,
                                    CultureInfo.CurrentCulture, out double amount))
                {
                    MessageBox.Show("Invalid Amount value. Use numbers only (e.g. 1234.56).","Invalid",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                    txtAmount.Focus();
                    return;
                }

                // 2) Bank balance parse (we'll also validate required bank fields below)
                if (!double.TryParse(txtBankBalance.Text?.Trim(), NumberStyles.Float | NumberStyles.AllowThousands,
                                    CultureInfo.CurrentCulture, out double balance))
                {
                    MessageBox.Show("Invalid Bank Balance value. Use numbers only (e.g. 1000.00).","Invalid",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                    txtBankBalance.Focus();
                    return;
                }

                // 3) Validate required Bank fields (LinkedBank is required on Expense)
                var bankName = txtBankName.Text?.Trim() ?? string.Empty;
                var acctType = cmbBankAccountType.SelectedItem?.ToString() ?? string.Empty;
                var acctId = txtBankAccountId.Text?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(bankName))
                {
                    MessageBox.Show("Please enter the bank name.","Invalid",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                    txtBankName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(acctType))
                {
                    MessageBox.Show("Please choose an account type.","Invalid",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                    cmbBankAccountType.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(acctId))
                {
                    MessageBox.Show("Please enter the bank account ID.","Invalid",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                    txtBankAccountId.Focus();
                    return;
                }

                // 4) Coupon: only create if any coupon field or coupon date is present
                Expense.Coupon? coupon = null;
                bool couponHasText =
                    !string.IsNullOrWhiteSpace(txtCouponCode.Text) ||
                    !string.IsNullOrWhiteSpace(txtCouponDesc.Text) ||
                    !string.IsNullOrWhiteSpace(txtCouponStore.Text);

                bool couponHasDate = TryParseDateFromCombos(ExpMonth, ExpDay, ExpYear, out DateTime couponDate);

                if (couponHasText || couponHasDate)
                {
                    // If a date was provided but invalid, bail
                    if (!couponHasDate && (ExpMonth.SelectedItem != null || ExpDay.SelectedItem != null || ExpYear.SelectedItem != null))
                    {
                        MessageBox.Show("Coupon expiration date is invalid or incomplete.","Invalid",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                        ExpMonth.Focus();
                        return;
                    }

                    coupon = new Expense.Coupon
                    {
                        Code = string.IsNullOrWhiteSpace(txtCouponCode.Text) ? null : txtCouponCode.Text.Trim(),
                        Description = string.IsNullOrWhiteSpace(txtCouponDesc.Text) ? null : txtCouponDesc.Text.Trim(),
                        Store = string.IsNullOrWhiteSpace(txtCouponStore.Text) ? null : txtCouponStore.Text.Trim(),
                        ExpirationDate = couponHasDate ? (DateTime?)couponDate : null
                    };
                }

                // 5) Bank link date from combos (optional but prefer provided date if present)
                DateTime bankLinkDate;
                bool bankLinkHasDate = TryParseDateFromCombos(LinkMonth, LinkDay, LinkYear, out bankLinkDate);

                if (!bankLinkHasDate)
                {
                    // If user provided some year/month/day selections but it was invalid -> prompt
                    if (LinkMonth.SelectedItem != null || LinkDay.SelectedItem != null || LinkYear.SelectedItem != null)
                    {
                        MessageBox.Show("Bank link date is invalid or incomplete.","Invalid",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                        LinkMonth.Focus();
                        return;
                    }

                    // Fallback: use today's date if none provided
                    bankLinkDate = DateTime.Now.Date;
                }

                // 6) Build expense object
                var expense = new Expense
                {
                    Id = IDGenerator.GenID("Expense"),
                    Amount = amount,
                    Date = DateTime.Now, // main transaction date
                    Mode = cmbMode.SelectedItem?.ToString() ?? "Individual",
                    Receiver = txtReceiver.Text?.Trim() ?? string.Empty,
                    Category = cmbCategory.SelectedItem?.ToString() ?? "Others",
                    Currency = cmbCurrency.SelectedItem?.ToString() ?? "Dollar",
                    AppliedCoupon = coupon,
                    LinkedBank = new Expense.Bank
                    {
                        Name = bankName,
                        AccountType = acctType,
                        AccountId = acctId,
                        Balance = balance,
                        LinkDate = bankLinkDate
                    }
                };

                // 7) Save via your service
                (bool ok, ID savedId) = ExpenseService.SaveExpense(expense);
                if (ok)
                {
                    MessageBox.Show("Expense saved successfully!","Info",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    // close this form
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    return;
                }
                else
                {
                    MessageBox.Show("Failed to save expense. The service returned an error.","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating expense: {ex.Message}","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }
    }

    public class ViewExpense : MaterialForm
    {
        public ViewExpense(Expense expense)
        {
            var skinManager = MaterialSkinManager.Instance;
            skinManager.AddFormToManage(this);

            Text = "View Expense";
            Size = new Size(700, 670);
            MinimumSize = new Size(700, 670);
            StartPosition = FormStartPosition.CenterScreen;

            // container
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12,0,12,12),
            };
            this.Controls.Add(mainPanel);

            // Horizontal divder
            var Hdivder = new MaterialDivider
            {
                Dock = DockStyle.Top,
                Height = 6,
                Margin = new Padding(2,2,2,0),
            };
            this.Controls.Add(Hdivder);

            // header: Title + Close button
            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 3,
                RowCount = 1,
                AutoSize = true,
                Padding = new Padding(6)
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.Controls.Add(header);

            // Body: table of label / value rows
            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 3,
                Padding = new Padding(6,0,6,6),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160f)); // label column fixed width
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 6f)); // Vertical divder
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));   // value column flexible
            mainPanel.Controls.Add(body);

            // Helper to add rows
            void AddRow(string labelText, string valueText, bool multiline = false)
            {
                var lbl = new MaterialLabel
                {
                    Text = labelText,
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font(FontFamily.GenericSansSerif, 9f, FontStyle.Bold),
                    Margin = new Padding(3, 6, 3, 6)
                };

                var val = new MaterialLabel
                {
                    Text = string.IsNullOrEmpty(valueText) ? "—" : valueText,
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font(FontFamily.GenericSansSerif, 10f, FontStyle.Regular),
                    Margin = new Padding(3, 6, 3, 6)
                };

                // horizonatl Item divder
                var HIdivder = new MaterialDivider
                {
                    Dock = DockStyle.Fill,
                    Height = 3,
                    Margin = new Padding(2,0,2,0),
                };

                // vertical item divder
                var VIdivder = new MaterialDivider
                {
                    Dock = DockStyle.Left,
                    Width = 6,
                    Margin = new Padding(2,0,2,0),
                };

                if (multiline)
                {
                    // allow wrapping/word-break by giving a larger height when added; row auto-sizing will follow
                    val.MaximumSize = new Size(0, 0);
                    val.AutoEllipsis = true;
                }

                // Context menu + double-click to copy value
                var ctx = new ContextMenuStrip();
                var copy = new ToolStripMenuItem("Copy value");
                copy.Click += (s, e) =>
                {
                    try { Clipboard.SetText(val.Text); }
                    catch { /* ignore clipboard errors */ }
                };
                ctx.Items.Add(copy);
                val.ContextMenuStrip = ctx;
                val.DoubleClick += (s, e) =>
                {
                    try { Clipboard.SetText(val.Text); }
                    catch { /* ignore */ }
                };

                int rowIndex = body.RowCount;
                body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                body.Controls.Add(lbl, 0, rowIndex);
                body.Controls.Add(VIdivder, 1, rowIndex);
                body.Controls.Add(val, 2, rowIndex);
                body.Controls.Add(HIdivder, 0, ++body.RowCount);
                body.SetColumnSpan(HIdivder,3);
                body.RowCount++;
            }

            // Build display values with safe formatting
            string fmtAmount = expense.Amount.ToString("N2", CultureInfo.CurrentCulture);
            string fmtDate = expense.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture);
            AddRow("Amount", fmtAmount);
            AddRow("Date", fmtDate);
            AddRow("Mode", expense.Mode ?? "—");
            AddRow("Receiver", expense.Receiver ?? "—", multiline: true);
            AddRow("Category", expense.Category ?? "—");
            AddRow("Currency", expense.Currency ?? "—");

            // Coupon (optional)
            if (expense.AppliedCoupon is not null)
            {
                AddRow("Coupon Code", expense.AppliedCoupon.Code ?? "—");
                AddRow("Coupon Store", expense.AppliedCoupon.Store ?? "—");
                AddRow("Coupon Expiration", expense.AppliedCoupon.ExpirationDate?.ToString("yyyy-MM-dd") ?? "—");
            }

            // Bank (LinkedBank is required in your model, but we play safe)
            if (expense.LinkedBank is not null)
            {
                AddRow("Bank Name", expense.LinkedBank.Name ?? "—");
                AddRow("Account Type", expense.LinkedBank.AccountType ?? "—");
                AddRow("Account ID", expense.LinkedBank.AccountId ?? "—");
                AddRow("Bank Balance", expense.LinkedBank.Balance.ToString("N2", CultureInfo.CurrentCulture));
                AddRow("Bank Link Date", expense.LinkedBank.LinkDate.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture));
            }

            // Copy all as JSON / quick export (useful)
            var btnCopyAll = new MaterialButton
            {
                Text = "Copy",
                Anchor = AnchorStyles.Left,
                Icon = IconLibrary.GetBitmap(AppIcon.Planner, 24, MainForm.PrimaryLight),
                Type = MaterialButton.MaterialButtonType.Contained,
                Margin = new Padding(3)
            };
            btnCopyAll.Click += (s, e) =>
            {
                try
                {
                    // lightweight summary text (not heavy object serialization)
                    var lines = new List<string>
                    {
                        $"Amount: {fmtAmount}",
                        $"Date: {fmtDate}",
                        $"Mode: {expense.Mode ?? "—"}",
                        $"Receiver: {expense.Receiver ?? "—"}",
                        $"Category: {expense.Category ?? "—"}",
                        $"Currency: {expense.Currency ?? "—"}"
                    };
                    if (expense.AppliedCoupon is not null)
                    {
                        lines.Add($"Coupon: {expense.AppliedCoupon.Code ?? "—"} ({expense.AppliedCoupon.Description ?? "—"})");
                    }
                    if (expense.LinkedBank is not null)
                    {
                        lines.Add($"Bank: {expense.LinkedBank.Name} ({expense.LinkedBank.AccountType}) {expense.LinkedBank.AccountId} - {expense.LinkedBank.Balance.ToString("N2", CultureInfo.CurrentCulture)}");
                    }

                    Clipboard.SetText(string.Join(Environment.NewLine, lines));
                    MessageBox.Show("Copied summary to clipboard.","Info",MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
                catch
                {
                    MessageBox.Show("Failed to copy to clipboard.","Info",MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
            };
            header.Controls.Add(btnCopyAll,0,0);

            // Close already exists above; add a second button for Save/Export hook if needed
            var btnExport = new MaterialButton
            {
                Text = "Export",
                Anchor = AnchorStyles.Top,
                Icon = IconLibrary.GetBitmap(AppIcon.AddNote, 24, MainForm.PrimaryLight),
                Type = MaterialButton.MaterialButtonType.Contained,
                Margin = new Padding(3)
            };
            btnExport.Click += (s, e) =>
            {
                try
                {
                    using var sfd = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*", FileName = $"expense_{DateTime.Now:yyyyMMddHHmmss}.csv" };
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        var rows = new List<string[]>
                        {
                            new[] {"Field", "Value"},
                            new[] {"Amount", fmtAmount},
                            new[] {"Date", fmtDate},
                            new[] {"Mode", expense.Mode ?? "—"},
                            new[] {"Receiver", expense.Receiver ?? "—"},
                            new[] {"Category", expense.Category ?? "—"},
                            new[] {"Currency", expense.Currency ?? "—"}
                        };
                        if (expense.AppliedCoupon is not null)
                        {
                            rows.Add(new[] {"Coupon Code", expense.AppliedCoupon.Code ?? "—"});
                            rows.Add(new[] {"Coupon Desc", expense.AppliedCoupon.Description ?? "—"});
                            rows.Add(new[] {"Coupon Store", expense.AppliedCoupon.Store ?? "—"});
                            rows.Add(new[] {"Coupon Expiration", expense.AppliedCoupon.ExpirationDate?.ToString("yyyy-MM-dd") ?? "—"});
                        }
                        if (expense.LinkedBank is not null)
                        {
                            rows.Add(new[] {"Bank Name", expense.LinkedBank.Name ?? "—"});
                            rows.Add(new[] {"Bank Acc-Type", expense.LinkedBank.AccountType ?? "—"});
                            rows.Add(new[] {"Bank Acc-ID", expense.LinkedBank.AccountId ?? "—"});
                            rows.Add(new[] {"Bank Balance", expense.LinkedBank.Balance.ToString("N2", CultureInfo.CurrentCulture)});
                            rows.Add(new[] {"Bank LinkDate", expense.LinkedBank.LinkDate.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture)});
                        }

                        using var sw = new System.IO.StreamWriter(sfd.FileName);
                        foreach (var r in rows)
                        {
                            // simple CSV (no escaping since values should be simple
                            sw.WriteLine($"{r[0]},{r[1]}");
                        }

                        MessageBox.Show("Exported CSV.","Info",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                }
            };
            header.Controls.Add(btnExport,1,0);

            var line = new MaterialDivider
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
            };
            header.Controls.Add(line,2,0);
        }
    }
}