using App.Automation.Core.Base;
using App.Automation.Core.DataModels.Shared;
using App.Automation.Core.Utilities;
using App.Automation.Modules.Global.Sections;
using App.Automation.Modules.Sales.Invoice.DataModels;
using OpenQA.Selenium;

namespace App.Automation.Modules.Global.Validators;

public class TotalsValidator : BaseValidator
{
    private readonly ExpectationHandler _expectation;

    public TotalsValidator(
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
    /// Validate only the GrandTotal — quick assertion for simple scenarios.
    /// </summary>
    public void ValidateGrandTotal(decimal expectedGrandTotal)
    {
        Report.Info($"Validating Grand Total: Expected = {expectedGrandTotal}");
        var actuals = _expectation.ReadTotals();

        if (actuals.TryGetValue("GrandTotal", out decimal actual))
            AssertAmountEqual(expectedGrandTotal, actual, "Grand Total");
        else
            Report.Warning("Could not read GrandTotal from UI.");
    }

    /// <summary>
    /// Validate only the BalanceDue — useful after partial payment scenarios.
    /// </summary>
    public void ValidateBalanceDue(decimal expectedBalance)
    {
        Report.Info($"Validating Balance Due: Expected = {expectedBalance}");
        var actuals = _expectation.ReadTotals();

        if (actuals.TryGetValue("BalanceDue", out decimal actual))
            AssertAmountEqual(expectedBalance, actual, "Balance Due");
        else
            Report.Warning("Could not read BalanceDue from UI.");
    }

    // ── Private helper ─────────────────────────────────────────────────────

    /// <summary>
    /// Validate a single total field by key name.
    /// Reads from the actuals dictionary and compares with expected value.
    /// </summary>
    private void ValidateSingleTotal(
        Dictionary<string, decimal> actuals,
        string key,
        decimal expected)
    {
        if (!actuals.TryGetValue(key, out decimal actual))
        {
            Report.Warning($"⚠ {key}: Could not read from UI — locator may need updating.");
            return;
        }

        AssertAmountEqual(expected, actual, key);
    }

    private void Compare(string field, decimal expected, decimal actual)
    {
        if (Math.Abs(expected - actual) > 0.01m)
            throw new Exception($"{field} mismatch → Expected: {expected}, Actual: {actual}");

        Report.Pass($"{field} matched → {actual}");
    }

    public void ValidateTotalsFromApi(ExpectedResultDM expected, TotalsResponseDM actual)
    {
        if (expected?.Totals == null)
        {
            Report.Warning("Expected.Totals not defined — skipping API validation.");
            return;
        }

        Report.Info("── Validating Financial Totals (API) ──");

        // Mapping JSON → API
        AssertAmountEqual(expected.Totals.GrossValue, actual.GrossValue, "GrossValue");
        AssertAmountEqual(expected.Totals.DiscountValue, actual.DiscountValue, "DiscountValue");
        AssertAmountEqual(expected.Totals.NetValue, actual.NetValue, "NetValue");
    }
}
