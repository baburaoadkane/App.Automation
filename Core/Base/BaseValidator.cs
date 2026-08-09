using App.Automation.Core.Interfaces;
using App.Automation.Core.Utilities;
using OpenQA.Selenium;

namespace App.Automation.Core.Base;

public abstract class BaseValidator : IValidator
{
    // ── Core dependencies ──────────────────────────────────────────────────
    protected readonly IWebDriver Driver;
    protected readonly WaitHelper Wait;
    protected readonly ReportHelper Report;

    // ── Constructor ────────────────────────────────────────────────────────
    protected BaseValidator(IWebDriver driver, WaitHelper wait, ReportHelper report)
    {
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        Wait = wait ?? throw new ArgumentNullException(nameof(wait));
        Report = report ?? throw new ArgumentNullException(nameof(report));
    }

    // ── Text / String Assertions ───────────────────────────────────────────

    /// <summary>
    /// Assert that the UI element's text equals the expected value.
    /// Logs PASS or FAIL to the report automatically.
    /// </summary>
    protected void AssertText(By locator, string expected, string fieldName)
    {
        string actual = GetText(locator);
        LogAndAssertEqual(expected, actual, fieldName);
    }

    /// <summary>Assert an input field's value attribute equals expected.</summary>
    protected void AssertValue(By locator, string expected, string fieldName)
    {
        string actual = GetValue(locator);
        LogAndAssertEqual(expected, actual, fieldName);
    }

    /// <summary>Assert that element text contains the expected substring.</summary>
    protected void AssertContains(By locator, string expectedSubstring, string fieldName)
    {
        string actual = GetText(locator);

        if (actual.Contains(expectedSubstring, StringComparison.OrdinalIgnoreCase))
        {
            Report.Pass($"✓ {fieldName}: Contains '{expectedSubstring}' | Actual: '{actual}'");
        }
        else
        {
            Report.Fail($"✗ {fieldName}: Expected to contain '{expectedSubstring}' | Actual: '{actual}'");
            Assert.Fail($"[{fieldName}] Expected to contain '{expectedSubstring}' but got '{actual}'");
        }
    }

    // ── Numeric / Amount Assertions ────────────────────────────────────────

    /// <summary>
    /// Assert a numeric field equals expected value.
    /// Tolerance: ±0.01 by default (handles rounding differences).
    /// </summary>
    protected void AssertAmount(By locator, decimal expected, string fieldName, decimal tolerance = 0.01m)
    {
        string rawActual = GetText(locator).Replace(",", "").Replace("$", "").Trim();

        if (!decimal.TryParse(rawActual, out decimal actual))
        {
            Report.Fail($"✗ {fieldName}: Could not parse amount '{rawActual}' as decimal.");
            Assert.Fail($"[{fieldName}] Could not parse '{rawActual}' as a decimal amount.");
            return;
        }

        decimal diff = Math.Abs(actual - expected);

        if (diff <= tolerance)
        {
            Report.Pass($"✓ {fieldName}: Expected={expected} | Actual={actual} | Diff={diff}");
        }
        else
        {
            Report.Fail($"✗ {fieldName}: Expected={expected} | Actual={actual} | Diff={diff} (tolerance={tolerance})");
            Assert.Fail($"[{fieldName}] Amount mismatch. Expected: {expected}, Actual: {actual}");
        }
    }

    /// <summary>Assert a numeric value passed directly (not from UI locator).</summary>
    protected void AssertAmountEqual(decimal expected, decimal actual, string fieldName, decimal tolerance = 0.01m)
    {
        decimal diff = Math.Abs(actual - expected);

        if (diff <= tolerance)
            Report.Pass($"✓ {fieldName}: Expected={expected} | Actual={actual}");
        else
        {
            Report.Fail($"✗ {fieldName}: Expected={expected} | Actual={actual} | Diff={diff}");
            Assert.Fail($"[{fieldName}] Amount mismatch. Expected: {expected}, Actual: {actual}");
        }
    }

    // ── Visibility / State Assertions ──────────────────────────────────────

    /// <summary>Assert that an element is visible on the page.</summary>
    protected void AssertVisible(By locator, string elementName)
    {
        bool visible = IsVisible(locator);

        if (visible)
            Report.Pass($"✓ {elementName}: Is visible on page.");
        else
        {
            Report.Fail($"✗ {elementName}: Expected to be visible but was NOT found.");
            Assert.Fail($"[{elementName}] Element is not visible on page.");
        }
    }

    /// <summary>Assert that an element is NOT visible on the page.</summary>
    protected void AssertNotVisible(By locator, string elementName)
    {
        bool visible = IsVisible(locator);

        if (!visible)
            Report.Pass($"✓ {elementName}: Correctly not visible.");
        else
        {
            Report.Fail($"✗ {elementName}: Expected to be hidden but IS visible.");
            Assert.Fail($"[{elementName}] Element should not be visible but is present.");
        }
    }

    /// <summary>Assert an element is enabled (not disabled/readonly).</summary>
    protected void AssertEnabled(By locator, string elementName)
    {
        bool enabled = Driver.FindElement(locator).Enabled;

        if (enabled)
            Report.Pass($"✓ {elementName}: Is enabled.");
        else
        {
            Report.Fail($"✗ {elementName}: Expected to be enabled but is disabled.");
            Assert.Fail($"[{elementName}] Element is disabled.");
        }
    }

    // ── Document Status Assertion ──────────────────────────────────────────

    /// <summary>
    /// Assert the document status badge/label matches expected.
    /// Override the locator for your ERP's status element.
    /// </summary>
    protected void AssertDocumentStatus(string expectedStatus, By? statusLocator = null)
    {
        statusLocator ??= By.CssSelector(
            ".document-status, .status-badge, [data-field='status'], .doc-status"
        );

        AssertText(statusLocator, expectedStatus, "Document Status");
    }

    // ── Message / Toast Assertions ─────────────────────────────────────────

    /// <summary>Assert that a success toast/notification appears.</summary>
    protected void AssertSuccessMessage(string? expectedText = null)
    {
        By toast = By.CssSelector(".toast-success, .alert-success, [class*='success']");
        AssertVisible(toast, "Success Notification");

        if (!string.IsNullOrEmpty(expectedText))
            AssertContains(toast, expectedText, "Success Message Text");
    }

    /// <summary>Assert that an error/validation message appears.</summary>
    protected void AssertErrorMessage(string expectedText)
    {
        By error = By.CssSelector(
            ".toast-error, .alert-danger, .validation-error, [class*='error']"
        );
        AssertContains(error, expectedText, "Error Message");
    }

    // ── Protected Read Helpers (for use in child validators) ───────────────

    protected string GetText(By locator)
    {
        try
        {
            IWebElement element = Wait.UntilVisible(locator);
            return element.Text.Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    protected string GetValue(By locator)
    {
        try
        {
            IWebElement element = Wait.UntilVisible(locator);
            return element.GetAttribute("value")?.Trim() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    protected bool IsVisible(By locator)
    {
        try { return Driver.FindElement(locator).Displayed; }
        catch { return false; }
    }

    // ── Private helpers ────────────────────────────────────────────────────

    private void LogAndAssertEqual(string expected, string actual, string fieldName)
    {
        if (string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
        {
            Report.Pass($"✓ {fieldName}: Expected='{expected}' | Actual='{actual}'");
        }
        else
        {
            Report.Fail($"✗ {fieldName}: Expected='{expected}' | Actual='{actual}'");
            Assert.Fail($"[{fieldName}] Mismatch. Expected: '{expected}', Actual: '{actual}'");
        }
    }
}
