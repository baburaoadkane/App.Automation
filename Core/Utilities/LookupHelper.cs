using OpenQA.Selenium;

namespace App.Automation.Core.Utilities;

public class LookupHelper
{
    private readonly IWebDriver _driver;
    private readonly WaitHelper _wait;
    private readonly ReportHelper _report;

    public LookupHelper(
        IWebDriver driver,
        WaitHelper wait,
        ReportHelper report)
    {
        _driver = driver;
        _wait = wait;
        _report = report;
    }

    // ================================================================
    // SELECT FROM CARD LOOKUP
    // ================================================================

    /// <summary>
    /// Finds a card/list item containing the specified value
    /// and clicks the matching card.
    /// </summary>
    public void SelectCard(
        string value,
        string cardSelector = ".dx-list-item .pa-list-item")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        _report.Info(
            $"Searching card contains: {value}");

        _wait.WaitForSeconds(1);

        var cards = _driver.FindElements(
            By.CssSelector(cardSelector));

        foreach (var card in cards)
        {
            string actualValue = card.Text.Trim();

            _report.Info(
                $"Checking card: {actualValue}");

            if (!actualValue.Contains(
                    value,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            _report.Info(
                $"Matching card found: {value}");

            ScrollIntoView(card);

            card.Click();

            _wait.WaitForSeconds(1);

            return;
        }

        throw new NoSuchElementException(
            $"Card containing '{value}' was not found.");
    }

    // ================================================================
    // SELECT FROM CARD LOOKUP - EXACT MATCH
    // ================================================================

    /// <summary>
    /// Finds a card whose complete text exactly matches
    /// the specified value.
    /// </summary>
    public void SelectCardExact(
        string value,
        string cardSelector = ".dx-list-item .pa-list-item")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        _report.Info(
            $"Searching for exact card: {value}");

        _wait.WaitForSeconds(1);

        var cards = _driver.FindElements(
            By.CssSelector(cardSelector));

        foreach (var card in cards)
        {
            string actualValue = card.Text.Trim();

            if (!string.Equals(
                    actualValue,
                    value,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            _report.Info(
                $"Exact matching card found: {value}");

            ScrollIntoView(card);

            card.Click();

            _wait.WaitForSeconds(1);

            return;
        }

        throw new NoSuchElementException(
            $"Exact card '{value}' was not found.");
    }

    // ================================================================
    // SELECT CARD BY SUBTEXT
    // ================================================================

    /// <summary>
    /// Finds a card containing the specified value
    /// in its visible text.
    /// Useful for approval cards such as:
    /// Goods Receipt
    /// (GR-20020, Company: EBS)
    /// </summary>
    public void SelectCardByText(
        string value,
        string cardSelector = ".dx-list-item .pa-list-item")
    {
        SelectCard(value, cardSelector);
    }

    // ================================================================
    // SCROLL
    // ================================================================

    private void ScrollIntoView(IWebElement element)
    {
        ((IJavaScriptExecutor)_driver)
            .ExecuteScript(
                "arguments[0].scrollIntoView({block:'center'});",
                element);
    }
}