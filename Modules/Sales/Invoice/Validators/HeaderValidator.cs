using App.Automation.Core.Base;
using App.Automation.Core.DataModels.Shared;
using App.Automation.Core.Utilities;
using App.Automation.Modules.Global.Sections;
using OpenQA.Selenium;

namespace App.Automation.Modules.Sales.Invoice.Validators
{
    public class HeaderValidator : BaseValidator
    {
        private readonly ExpectationHandler _expectation;

        public HeaderValidator(
            IWebDriver driver,
            WaitHelper wait,
            ReportHelper report,
            ExpectationHandler expectation)
            : base(driver, wait, report)
        {
            _expectation = expectation;
        }

        // ── Public validation methods ──────────────────────────────────────────

        /// <summary>
        /// Assert multiple header field values match the data model.
        /// Reads each field from the UI and compares against the model values.
        /// Only validates fields that are non-null in the data model.
        /// </summary>
        public void ValidateHeaderFields(
            string? expectedCustomer,
            string? expectedDate,
            string? expectedCurrency)
        {
            // Customer
            if (!string.IsNullOrWhiteSpace(expectedCustomer))
            {
                string actual = GetText(By.CssSelector(
                    "#Header_CustomerName, [data-field='CustomerName'], .customer-display"
                ));
                LogAndAssert(expectedCustomer, actual, "Customer");
            }

            // Invoice Date
            if (!string.IsNullOrWhiteSpace(expectedDate))
            {
                string actual = GetValue(By.Id("Header_InvoiceDate"));
                LogAndAssert(expectedDate, actual, "Invoice Date");
            }

            // Currency
            if (!string.IsNullOrWhiteSpace(expectedCurrency))
            {
                string actual = GetText(By.CssSelector(
                    "#Header_CurrencyId option:checked, [data-field='Currency']"
                ));
                LogAndAssert(expectedCurrency, actual, "Currency");
            }
        }

        /// <summary>
        /// Assert that a document number was generated after save.
        /// Validates the document number is not empty.
        /// </summary>
        public void ValidateDocumentNumberGenerated()
        {
            string docNo = _expectation.ReadDocumentNumber();

            if (!string.IsNullOrWhiteSpace(docNo))
                Report.Pass($"✓ Number generated: '{docNo}'");
            else
            {
                Report.Fail("✗ Number is empty after Save.");
                NUnit.Framework.Assert.Fail("Number was not generated after Save.");
            }
        }

        /// <summary>
        /// Assert the document status matches the expected value from JSON.
        /// Example: "Draft", "Submitted", "Approved"
        /// </summary>
        public void ValidateDocumentPaymentStatus(ExpectedResultDM? expected)
        {
            if (expected?.PaymentStatus == null)
            {
                Report.Warning("Expected Payment Status not defined in JSON — skipping status validation.");
                return;
            }

            Report.Info($"Validating Document Payment Status: Expected = '{expected.PaymentStatus}'");

            string actualStatus = _expectation.ReadDocumentPaymentStatus();
            LogAndAssert(expected.PaymentStatus, actualStatus, "Document Payment Status");
        }

        /// <summary>
        /// Assert the document status matches the expected value from JSON.
        /// Example: "Draft", "Submitted", "Approved"
        /// </summary>
        public void ValidateDocumentStatus(ExpectedResultDM? expected)
        {
            if (expected?.Status == null)
            {
                Report.Warning("Expected Status not defined in JSON — skipping status validation.");
                return;
            }

            Report.Info($"Validating Status: Expected = '{expected.Status}'");

            string actualStatus = _expectation.ReadDocumentStatus();
            LogAndAssert(expected.Status, actualStatus, "Status");
        }

        // ── Private helper ─────────────────────────────────────────────────────

        private void LogAndAssert(string expected, string actual, string fieldName)
        {
            if (string.Equals(expected.Trim(), actual.Trim(), StringComparison.OrdinalIgnoreCase))
                Report.Pass($"✓ {fieldName}: Expected='{expected}' | Actual='{actual}'");
            else
            {
                Report.Fail($"✗ {fieldName}: Expected='{expected}' | Actual='{actual}'");
                NUnit.Framework.Assert.Fail(
                    $"[HeaderValidator] {fieldName} mismatch. Expected: '{expected}', Actual: '{actual}'");
            }
        }
    }
}
