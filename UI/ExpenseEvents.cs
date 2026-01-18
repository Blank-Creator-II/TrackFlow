using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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
            cmbCategory.Items.AddRange(new object[] { "Entertainment", "Grocery", "Medicine", "Other", "Shopping", "Travel", "Utilities" });
            Table.Controls.Add(cmbCategory, 0, 3);

            // Currency (r:3 c:1)
            cmbCurrency = new MaterialComboBox 
            { 
                Hint = "Chose Currency", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill };
            cmbCurrency.Items.AddRange(new object[] {
                "Birr", "Dollar", "Euro", "Pound", "Yen", "Yuan", "Won", "Rupee",
                "Rand", "Naira", "Peso", "Franc", "Dinar", "Dirham", "Shekel",
                "Ruble", "Real", "Zloty", "Krona", "Krone", "Baht", "Dong",
                "Ringgit", "Rupiah", "Taka", "Hryvnia", "Forint", "Cedi",
                "Shilling", "Pula", "Kwacha", "Metical", "Leu", "Lev",
                "Kip", "Tugrik", "Manat", "Som", "Lira", "Bolivar",
                "Sol", "Guarani", "Lempira", "Quetzal", "Balboa",
                "Tenge", "Dram", "Rial", "Pataca", "Ngultrum"
                });
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

                if (string.IsNullOrWhiteSpace(txtReceiver.Text?.Trim()))
                {
                    MessageBox.Show("Please enter the receiver.","Invalid",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                    txtReceiver.Focus();
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
                    Receiver = txtReceiver.Text?.Trim()!,
                    Category = cmbCategory.SelectedItem?.ToString() ?? "Other",
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
                Icon = IconLibrary.GetBitmap(AppIcon.Copy, 24, MainForm.PrimaryLight),
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
                Icon = IconLibrary.GetBitmap(AppIcon.Export, 24, MainForm.PrimaryLight),
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

    private FlowLayoutPanel BuildRecommendationsPanel(Dictionary<string, double> totals, int maxRecommendations = 3)
    {
        // Safety
        if (maxRecommendations <= 0) maxRecommendations = 3;
        if (totals == null || totals.Count <= 3) // I am prety sure there is a better method to this but this is not a corpo project so meh
        {
            totals = new Dictionary<string, double>();
            var _panel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(6),
                Dock = DockStyle.Fill
            };

            var fallback = new MaterialLabel
            {
                Text = "No Specific Recommendations\nTry Adding More Expense",
                FontType = MaterialSkinManager.fontType.H5,
                AutoSize = false,
                Width = (int)Math.Round(2.7 * _panel.ClientSize.Width),
                Height = 0,
                Margin = new Padding(3, 4, 3, 4),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _panel.Controls.Add(fallback);
            _panel.SizeChanged += (s, e) => {fallback.Width = _panel.ClientSize.Width; fallback.Height = _panel.ClientSize.Height;};

            return _panel;
        }

        // canonical category list
        var categories = new[] { "Entertainment", "Grocery", "Medicine", "Other", "Shopping", "Travel", "Utilities" };

        // Ensure all categories present with zero default
        var values = categories.ToDictionary(c => c, c => totals.ContainsKey(c) ? Math.Max(0.0, totals[c]) : 0.0);

        double total = values.Values.Sum();
        // percent map (0..1)
        var pct = values.ToDictionary(kv => kv.Key, kv => total > 0 ? kv.Value / total : 0.0);

        // Sort categories by amount desc
        var byAmountDesc = values.OrderByDescending(kv => kv.Value).ToList();
        string topCat = byAmountDesc.First().Key;
        double topAmt = byAmountDesc.First().Value;
        double secondAmt = byAmountDesc.Skip(1).FirstOrDefault().Value;

        // store candidates (message, score)
        var candidates = new List<(string msg, double score)>();

        string FormatPct(double x) => (x * 100).ToString("0.#", CultureInfo.CurrentCulture) + "%";

        // 1) General global signals
        if (total <= 0)
        {
            candidates.Add(("No spending recorded. Try adding transactions to see meaningful recommendations.", 5));
        }
        else
        {
            // overall spending intensity
            if (total >= 5000) candidates.Add(( $"Total spending is high ({total:N0}). Consider an audit or monthly budget.", 40 ));
            if (total >= 2000 && total < 5000) candidates.Add(( $"Overall monthly spending is moderate-high ({total:N0}). A quick audit could reveal savings.", 25 ));
            if (total < 200) candidates.Add(( $"Low activity detected ({total:N0}). If this is unexpected check that transactions are being tracked.", 8 ));
        }

        // 2) Top category rules
        candidates.Add(($"Top spending category: {topCat} — {FormatPct(pct[topCat])} of total.", 100 * pct[topCat] + 10));
        if (topAmt >= 2 * secondAmt && total > 0)
        {
            candidates.Add(($"{topCat} dominates spending (≥2× the next category). Consider setting a hard budget for it.", 80));
        }
        if (pct[topCat] >= 0.30) candidates.Add(($"{topCat} is a large share ({FormatPct(pct[topCat])}) — investigate recurring charges or subscriptions.", 70));
        if (pct[topCat] >= 0.20 && pct[topCat] < 0.30) candidates.Add(($"{topCat} takes a notable share ({FormatPct(pct[topCat])}). A small cap could improve savings.", 40));

        // 3) Per-category threshold recommendations
        foreach (var c in categories)
        {
            double p = pct[c];
            double amt = values[c];

            // general advices by category
            if (c == "Grocery")
            {
                if (p >= 0.20) candidates.Add(($"Grocery is {FormatPct(p)} of spending — try meal planning, bulk buys or price-tracking apps.", 60));
                if (p >= 0.12 && p < 0.20) candidates.Add(($"Grocery share ({FormatPct(p)}) is moderate — consider comparing store prices or using weekly lists.", 25));
            }
            else if (c == "Entertainment")
            {
                if (p >= 0.15) candidates.Add(($"Entertainment is {FormatPct(p)} of spending — cap impulse buys or subscribe to a cheaper plan.", 50));
                if (p >= 0.05 && p < 0.15) candidates.Add(($"Entertainment spending ({FormatPct(p)}) is reasonable — keep an eye on microtransactions.", 12));
            }
            else if (c == "Shopping")
            {
                if (p >= 0.15) candidates.Add(($"Shopping is {FormatPct(p)} — consider wishlist delays and watch for seasonal sales instead of impulse buys.", 45));
            }
            else if (c == "Travel")
            {
                if (p >= 0.15) candidates.Add(($"Travel is {FormatPct(p)} — consider planning trips in advance or using travel deals.", 40));
            }
            else if (c == "Utilities")
            {
                if (p >= 0.12) candidates.Add(($"Utilities are {FormatPct(p)} — check tariffs and energy-saving changes to cut costs.", 35));
            }
            else if (c == "Medicine")
            {
                if (p >= 0.10) candidates.Add(($"Medicine costs are {FormatPct(p)} — check for generic options or subscription savings.", 30));
            }
            else if (c == "Other")
            {
                if (p >= 0.18) candidates.Add(($"Large 'Other' category ({FormatPct(p)}) suggests uncategorized spending. Re-categorize to understand costs.", 65));
                if (p >= 0.08 && p < 0.18) candidates.Add(($"'Other' is notable ({FormatPct(p)}). Split recurring items into explicit categories.", 25));
            }

            // low spend suggestions
            if (p > 0 && p < 0.03) candidates.Add(($"{c} is very low ({FormatPct(p)}). If this is a priority, schedule or automate it; otherwise deprioritize.", 12));
            if (amt == 0) candidates.Add(($"{c} has no recorded spending. If you expect activity, check transaction sources.", 8));
        }

        // 4) Pairwise comparisons (directional) - lots of conditions created programmatically
        for (int i = 0; i < categories.Length; i++)
        {
            for (int j = 0; j < categories.Length; j++)
            {
                if (i == j) continue;
                var a = categories[i];
                var b = categories[j];

                double diff = values[a] - values[b];            // absolute difference
                double diffPct = total > 0 ? diff / total : 0; // difference as share of total

                // Significant directional difference
                if (diffPct >= 0.10)
                {
                    // "You spend significantly more on A than B (X% of total)"
                    candidates.Add(($"You spend significantly more on {a} than {b} ({FormatPct(Math.Abs(diffPct))} of total difference). Consider reallocating from {a} to {b} if appropriate.", 20 + 60 * Math.Abs(diffPct)));
                }

                // small/close competitors
                if (Math.Abs(pct[a] - pct[b]) <= 0.03 && values[a] > 0 && values[b] > 0)
                {
                    candidates.Add(($"{a} and {b} are close in spend ({FormatPct(pct[a])} vs {FormatPct(pct[b])}). Consider combining budgets or prioritizing between them.", 10));
                }

                // If B is nearly zero and A > small threshold
                if (values[b] < 1 && values[a] > 100 && total > 0)
                {
                    candidates.Add(($"{a} has non-trivial spending while {b} has almost none — check if {b} should be a tracked category or intentionally ignored.", 12));
                }
            }
        }

        // 5) Micro-spend explosion: many small categories each with small % -> suggest micro-tracking
        int smallCount = values.Values.Count(v => total > 0 ? v / total <= 0.05 && v > 0 : false);
        if (smallCount >= 3)
        {
            candidates.Add(("Multiple small spending buckets detected. Micro-spend tracking or an 'everyday purchases' category could reduce noise.", 30));
        }

        // 6) Red flags / opportunities
        // big one-time vs recurring heuristic: if one category is >40% then big red flag
        if (pct[topCat] >= 0.40) candidates.Add(($"{topCat} is massive ({FormatPct(pct[topCat])}). This looks like either a big one-off or a recurring leak — investigate.", 90));
        // if many categories have nonzero small spends, propose subscription audit
        int recurringLikely = values.Values.Count(v => v > 50);
        if (recurringLikely >= 4) candidates.Add(("Multiple mid-size spends found — run a subscription/bundles audit to spot recurring charges.", 30));

        // 7) Unusual combos: e.g., Entertainment + Shopping combine high -> suggest entertainment-shopping split
        if (pct["Entertainment"] + pct["Shopping"] > 0.30) candidates.Add(("Entertainment + Shopping together are a big chunk. Consider consolidating impulse buys into a single monthly allowance.", 38));

        // 8) Final guard: prefer actionable suggestions
        // (no-op here; we already prioritized items by score)

        // Now pick top scoring unique messages
        var ordered = candidates
            .Where(c => !string.IsNullOrWhiteSpace(c.msg))
            .OrderByDescending(c => c.score)
            .Select(c => (c.msg, score: c.score))
            .ToList();

        // dedupe but preserve order by score
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var top = new List<string>();
        foreach (var (msg, score) in ordered)
        {
            if (top.Count >= maxRecommendations) break;
            var trimmed = msg.Trim();
            if (seen.Add(trimmed))
            {
                top.Add(trimmed);
            }
        }

        // Build FlowLayoutPanel with Material labels
        var panel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(6),
            Dock = DockStyle.Fill
        };

        foreach (var recommendation in top)
        {
            var lbl = new MaterialLabel
            {
                Text = "• " + recommendation,
                AutoSize = false,
                Width = (int)Math.Round(2.7 * panel.ClientSize.Width), // sensible label width — the container will adjust anyway
                Height = 0,
                Margin = new Padding(3, 4, 3, 4),
                //TextAlign = ContentAlignment.MiddleLeft
            };

            // Let the label auto-size vertically to content using MeasureString
            using (var g = lbl.CreateGraphics())
            {
                var sz = g.MeasureString(lbl.Text, lbl.Font, lbl.Width);
                lbl.Height = (int)Math.Ceiling(sz.Height) + 20;
            }

            // copy-on-click (handy)
            lbl.Click += (s, e) =>
            {
                try { Clipboard.SetText(recommendation); }
                catch { /* ignore clipboard issues */ }
            };

            panel.Controls.Add(lbl);
            panel.SizeChanged += (s, e) => {lbl.Width = panel.ClientSize.Width;};
        }

        // If no recommendations found, add a gentle fallback
        if (top.Count == 0)
        {
            var lbl = new MaterialLabel
            {
                Text = "No specific recommendations — spending looks balanced or there's not enough data.",
                AutoSize = false,
                Width = 320,
                Height = 40,
                Margin = new Padding(3, 4, 3, 4),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };
            panel.Controls.Add(lbl);
        }

        return panel;
    }
}
