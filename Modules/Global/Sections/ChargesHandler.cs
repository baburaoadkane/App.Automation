using App.Automation.Core.Base;
using App.Automation.Core.Utilities;
using App.Automation.Modules.Sales.Invoice.DataModels;
using OpenQA.Selenium;

namespace App.Automation.Modules.Global.Sections
{
    public class ChargesHandler : BaseHandler
    {
        // ── Section-level locators ─────────────────────────────────────────────
        private static readonly By LookupText = By.XPath("//div[contains(@class,'lookup-text')]");
        private static readonly By AddChargeButton = By.Id("SalesInvoiceChargeNewButton");
        private static readonly By NextButton = By.XPath("//a[contains(@class,'dxp-button')]//img[@alt='Next']");
        // ── Constructor ────────────────────────────────────────────────────────
        public ChargesHandler(IWebDriver driver, WaitHelper wait, ReportHelper report) : base(driver, wait, report) { }

        // ── Field Mapping (🔥 Dictionary Driven) ──────────────────────────────
        private class FieldConfig
        {
            public By? Dropdown { get; set; }
            public int? ColumnIndex { get; set; }
        }

        private static readonly Dictionary<string, FieldConfig> FieldMap = new()
        {
            ["Charge"] = new()
            {
                Dropdown = By.XPath("//td[contains(@id, '_ChargeId_B-1')]"),
                ColumnIndex = 1
            },
            ["AccountType"] = new()
            {
                Dropdown = By.XPath("//td[contains(@id, '_AccountTypeId_B-1')]"),
                ColumnIndex = 2
            },
            ["Account"] = new()
            {
                Dropdown = By.XPath("//td[contains(@id, '_AccountId_B-1')]"),
                ColumnIndex = 3
            },
            ["Currency"] = new()
            {
                Dropdown = By.XPath("//td[contains(@id, '_CurrencyId_B-1')]"),
                ColumnIndex = 4
            },
            ["AmountFC"] = new()
            {
                ColumnIndex = 5
            },
            ["Remarks"] = new()
            {
                ColumnIndex = 7
            }
        };

        // ── Public entry ─────────────────────────────────────────────────
        public void Fill(InvoiceChargesDM charges)
        {
            if (charges?.Items == null || charges.Items.Count == 0) return;

            NavigateToChargesSection();

            foreach (var charge in charges.Items)
            {
                AddNewCharge();
                FillCharge(charge);
                //WaitForLoader();
            }
        }

        // ── 🔥 Core Row Fill ──────────────────────────────────────────────────
        private void FillCharge(ChargeDM charge)
        {
            Lookup("Charge", charge.ChargeType);
            LookupCell("Account", charge.Account);

            SetCell("AmountFC", charge.AmountFC);
            SetCell("Remarks", charge.Remarks);
        }

        // ── 🔥 Generic Lookup ────────────────────────────────────────────────
        private void Lookup(string field, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            var dropdown = GetDropdown(field);

            OpenDropdown(dropdown);
            WaitForLoader();

            SelectOption(LookupText, NextButton, value);
        }

        // ── 🔥 Lookup inside Grid Cell ─────────────────────────────────────────
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
        private void NavigateToChargesSection()
        {
            By chargesTab = By.XPath("//td[contains(@id, 'Charge_HC')]");

            try
            {
                Wait.UntilClickable(chargesTab, timeoutSeconds: 3).Click();
                WaitForLoader();
            }
            catch
            {
                // Charges section already visible — no tab navigation needed
            }
        }

        private void AddNewCharge()
        {
            Click(AddChargeButton);
            WaitForLoader();
        }

        // ── Set Cell Value ────────────────────────────────────────────────────
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
