using App.Automation.Core.Base;
using App.Automation.Core.Utilities;
using App.Automation.Modules.Sales.Invoice.DataModels;
using OpenQA.Selenium;

namespace App.Automation.Modules.Global.Sections
{
    public class OthersHandler : BaseHandler
    {
        // ── Common ───────────────────────────────────────────────────────────
        private static readonly By LookupText = By.XPath("//div[contains(@class,'lookup-text')]");

        // ── Static Inputs ────────────────────────────────────────────────────
        private static readonly By ChequeNumInput = By.XPath("//input[contains(@id, '.ChequeNum_I')]");
        private static readonly By ContactPersonNameInput = By.XPath("//input[contains(@id, '.ContactPersonName_I')]");
        private static readonly By ContactPersonMobileInput = By.XPath("//input[contains(@id, '.ContactPersonMobile_I')]");
        private static readonly By ContactPersonEmailInput = By.XPath("//input[contains(@id, '.ContactPersonEmail_I')]");
        private static readonly By BillingAddressTextArea = By.XPath("//textarea[contains(@id, '.BillingAddress_I')]");
        private static readonly By ShippingAddressTextArea = By.XPath("//textarea[contains(@id, '.ShippingAddress_I')]");
        private static readonly By RemarksTextArea = By.XPath("//textarea[contains(@id, '.Description_I')]");

        public OthersHandler(IWebDriver driver, WaitHelper wait, ReportHelper report) : base(driver, wait, report) { }

        // ── Public Entry ─────────────────────────────────────────────────────
        public void Fill(InvoiceOthersDM other)
        {
            if (other == null) return;

            NavigateToOtherSection();

            Type(ChequeNumInput, other.ChequeNum);
            Type(ContactPersonNameInput, other.ContactPersonName);
            Type(ContactPersonMobileInput, other.ContactPersonMobile);
            Type(ContactPersonEmailInput, other.ContactPersonEmail);
            Type(BillingAddressTextArea, other.BillingAddress);
            Type(ShippingAddressTextArea, other.ShippingAddress);
            Type(RemarksTextArea, other.Remarks);

            Lookup("PaymentTermId", other.PaymentTerm);

            //WaitForLoader();
        }

        // ── 🔥 FULLY DYNAMIC LOOKUP (No FieldMap Needed) ─────────────────────
        private void Lookup(string fieldName, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            var (dropdown, input, nextPage) = BuildLookupLocators(fieldName);

            OpenDropdown(dropdown);

            //Type(input, value); 

            WaitForLoader();

            SelectOption(LookupText, nextPage, value);
        }

        // ── 🔥 SMART LOCATOR BUILDER ─────────────────────────────────────────
        private (By dropdown, By input, By nextPage) BuildLookupLocators(string fieldName)
        {
            // Example fieldName = "PaymentTermId"

            // Find exact input for this field (NOT generic)
            var inputElement = Wait.UntilVisible(
                By.XPath($"//input[contains(@id, '.{fieldName}Lookup_I')]")
            );

            string id = inputElement.GetAttribute("id");

            if (string.IsNullOrWhiteSpace(id) || !id.Contains('.'))
                throw new Exception($"Invalid ID format: {id}");

            // Example:
            // SalesInvoice.PaymentTermIdLookup_I

            string modulePrefix = id.Split('.')[0]; // SalesInvoice

            string baseId = $"{modulePrefix}.{fieldName}";

            var dropdown = By.Id($"{baseId}Lookup_B-1");
            var input = By.Id($"{baseId}Lookup_I");
            var nextPage = By.Id($"{baseId}_NextPage");

            return (dropdown, input, nextPage);
        }

        private void NavigateToOtherSection()
        {
            By otherTab = By.XPath("//td[contains(@id, 'General_HC')]");

            try
            {
                Wait.UntilClickable(otherTab, timeoutSeconds: 3).Click();
                WaitForLoader();
            }
            catch
            {
                // Other section already visible — no tab navigation needed
            }
        }
    }
}
