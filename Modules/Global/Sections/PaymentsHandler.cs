using App.Automation.Core.Base;
using App.Automation.Core.Utilities;
using App.Automation.Modules.Sales.Invoice.DataModels;
using OpenQA.Selenium;

namespace App.Automation.Modules.Global.Sections
{
    public class PaymentsHandler : BaseHandler
    {
        // ── Section-level locators ─────────────────────────────────────────────
        private static readonly By LookupText = By.XPath("//div[contains(@class,'lookup-text')]");
        private static readonly By AddPaymentButton = By.Id("SalesInvoicePaymentNewButton");
        private static readonly By NextButton = By.XPath("//a[contains(@class,'dxp-button')]//img[@alt='Next']");
        // ── Constructor ────────────────────────────────────────────────────────
        public PaymentsHandler(IWebDriver driver, WaitHelper wait, ReportHelper report) : base(driver, wait, report) { }

        private class FieldConfig
        {
            public By? Dropdown { get; set; }
            public int? ColumnIndex { get; set; }
        }

        private static readonly Dictionary<string, FieldConfig> FieldMap = new()
        {
            ["PaymentMethod"] = new()
            {
                Dropdown = By.XPath("//td[contains(@id, '_PaymentMethodId_B-1')]"),
                ColumnIndex = 1
            },
            ["Currency"] = new()
            {
                Dropdown = By.XPath("//td[contains(@id, 'Payment_CurrencyId_B-1')]"),
                ColumnIndex = 2
            },
            ["CardNum"] = new()
            {
                ColumnIndex = 3
            },
            ["AmountFC"] = new()
            {
                ColumnIndex = 4
            },
            ["Remarks"] = new()
            {
                ColumnIndex = 5
            }
        };

        // ── Public entry point ─────────────────────────────────────────────────
        public void Fill(InvoicePaymentsDM payments)
        {
            if (payments?.Entries == null || payments.Entries.Count == 0) return;

            NavigateToPaymentSection();

            foreach (var payment in payments.Entries)
            {
                AddNewPayment();
                FillPayment(payment);
                //WaitForLoader();
            }
        }

        /// <summary>Fill all fields for a single payment row.</summary>
        private void FillPayment(PaymentEntryDM payment)
        {
            Lookup("PaymentMethod", payment.PaymentMode);
            LookupCell("Currency", payment.Currency);

            SetCell("CardNum", payment.CardNumber);
            SetCell("AmountFC", payment.AmountFC);
            SetCell("Remarks", payment.Remarks);
        }

        // ── 🔥 Lookup inside Grid Cell ────────────────────────────────────────
        private void Lookup(string field, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            var dropdown = GetDropdown(field);

            OpenDropdown(dropdown);
            WaitForLoader();

            SelectOption(LookupText, NextButton, value);
        }

        // ── 🔥 Lookup inside Grid Cell ────────────────────────────────────────
        private void LookupCell(string field, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            var cell = GetCell(field);
            Click(cell);

            var dropdown = GetDropdown(field);

            OpenDropdown(dropdown);
            WaitForLoader();

            SelectOption(LookupText, NextButton, value);
        }

        // ── Mapping Helpers ───────────────────────────────────────────────────
        private By GetDropdown(string field)
        {
            if (!FieldMap.ContainsKey(field) || FieldMap[field].Dropdown == null)
                throw new Exception($"Dropdown not defined for field: {field}");

            return FieldMap[field].Dropdown!;
        }

        private int GetColIndex(string field)
        {
            if (!FieldMap.ContainsKey(field) || FieldMap[field].ColumnIndex == null)
                throw new Exception($"Column index not defined for field: {field}");

            return FieldMap[field].ColumnIndex!.Value;
        }

        // ── 🔥 Cell Locator ───────────────────────────────────────────────────
        private By GetCell(string field)
        {
            int colIndex = GetColIndex(field);
            return By.XPath($"(//div[@class='dxgBCTC dx-ellipsis'])[{colIndex}]");
        }

        // ── Navigation ────────────────────────────────────────────────────────
        private void NavigateToPaymentSection()
        {
            By paymentsTab = By.XPath("//td[contains(@id, 'PaymentMethods_HC')]");

            try
            {
                Wait.UntilClickable(paymentsTab, timeoutSeconds: 3).Click();
                WaitForLoader();
            }
            catch
            {
                // Payments section already visible — no tab navigation needed
            }
        }

        /// <summary>Click Add Payment button to create a new payment row.</summary>
        private void AddNewPayment()
        {
            Click(AddPaymentButton);
            WaitForLoader();
        }

        // ── Cell Value Setter────────────────────────────────────────────────────
        private void SetCell(string field, object? value)
        {
            if (value == null || !IsValidValue(value)) return;

            string finalValue = value switch
            {
                decimal d => d.ToString("G29"),
                double d => d.ToString("G29"),
                float f => f.ToString("G29"),
                int i => i.ToString("G29"),
                long l => l.ToString("G29"),
                _ => value.ToString() ?? string.Empty
            };

            var cell = GetCell(field);
            SetClipboardValue(cell, finalValue);
        }

        // ── Validation ────────────────────────────────────────────────────────
        private bool IsValidValue(object value)
        {
            return value switch
            {
                string s => !string.IsNullOrWhiteSpace(s),
                decimal d => d > 0,
                int i => i > 0,
                double d => d > 0,
                float f => f > 0,
                long l => l > 0,
                _ => true
            };
        }
    }
}
