using App.Automation.Core.Interfaces;
using App.Automation.Core.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using TextCopy;

namespace App.Automation.Core.Base
{
    public abstract class BaseHandler : IHandler
    {
        // ── Core dependencies ──────────────────────────────────────────────────
        protected readonly IWebDriver Driver;
        protected readonly WaitHelper Wait;
        protected readonly ConfigReader Config = ConfigReader.Instance;
        protected readonly ReportHelper Report;

        private readonly Actions _actions;

        // ── Constructor ────────────────────────────────────────────────────────
        protected BaseHandler(IWebDriver driver, WaitHelper wait, ReportHelper report)
        {
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
            Wait = wait ?? throw new ArgumentNullException(nameof(wait));
            Report = report ?? throw new ArgumentNullException(nameof(report));
            _actions = new Actions(driver);
        }

        // ── Click Actions ──────────────────────────────────────────────────────

        /// <summary>Wait for element to be clickable, then click.</summary>
        protected void Click(By locator)
        {
            var element = Wait.UntilClickable(locator);
            //ScrollIntoView(element);
            element.Click();
            Wait.WaitForSeconds(1);
        }

        /// <summary>Click directly on a located element.</summary>
        protected void Click(IWebElement element)
        {
            ScrollIntoView(element);
            Wait.UntilElementClickable(element);
            element.Click();
        }

        /// <summary>
        /// JavaScript click — use when standard click is intercepted.
        /// Useful for hidden elements or elements behind overlays.
        /// </summary>
        protected void JSClick(By locator)
        {
            var element = Wait.UntilVisible(locator);
            ScrollIntoView(element);

            ((IJavaScriptExecutor)Driver)
                .ExecuteScript("arguments[0].click();", element);
        }

        /// <summary>Double-click an element (e.g. to open inline edit in grids).</summary>
        protected void DoubleClick(By locator)
        {
            var element = Wait.UntilClickable(locator);
            ScrollIntoView(element);

            _actions.DoubleClick(element).Perform();
        }

        protected void PressEnter()
        {
            Driver.SwitchTo().ActiveElement().SendKeys(Keys.Enter);
        }

        // ── Input Actions ──────────────────────────────────────────────────────

        /// <summary>
        /// Clear existing text and type new value.
        /// Skips if value is null or empty.
        /// </summary>
        protected void Type(By locator, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            var element = Wait.UntilVisible(locator);
            ScrollIntoView(element);

            element.Clear();
            element.SendKeys(value);
            Wait.UntilValuePresent(locator, value);
        }

        protected void Type(By locator, decimal? value)
        {
            if (value <= 0) return;

            var element = Wait.UntilVisible(locator);
            ScrollIntoView(element);

            element.Clear();
            element.SendKeys(value.ToString() ?? string.Empty);
            PressEnter();
            Wait.WaitForSeconds(1);

            //Wait.UntilValuePresent(locator, value.ToString());
        }
        protected void ClearAndType(By locator, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            var element = Wait.UntilVisible(locator);
            ScrollIntoView(element);

            element.SendKeys(Keys.Control + "a");
            element.SendKeys(Keys.Delete);
            element.SendKeys(value);

            Wait.UntilValuePresent(locator, value);
        }
        /// <summary>
        /// Type and press Enter — used for search/autocomplete fields.
        /// Types the value then waits briefly for the dropdown to appear.
        /// </summary>
        protected void TypeAndSelect(By locator, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            Type(locator, value);
            Thread.Sleep(600);
            SendKey(locator, Keys.ArrowDown);
            SendKey(locator, Keys.Enter);
        }

        protected void TypeAndSelectChange(By locator, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            Type(locator, value);

            // Wait for lookup dropdown to appear
            Wait.Until(driver =>
                driver.FindElements(By.XPath("//div[contains(@class,'lookup')]")).Count > 0
            );

            SendKey(locator, Keys.ArrowDown);
            SendKey(locator, Keys.Enter);
        }
        protected void SetClipboardValue(By locator, string value)
        {
            var element = Wait.UntilVisible(locator);
            element.Click();
            Wait.WaitForSeconds(1);

            // Clear existing value
            _actions.KeyDown(Keys.Control)
                   .SendKeys("a")
                   .KeyUp(Keys.Control)
                   .SendKeys(Keys.Delete)
                   .Perform();

            // Set clipboard text 
            ClipboardService.SetText(value);
            string pasted = ClipboardService.GetText() ?? string.Empty;

            // Paste the value using Ctrl+V
            _actions.KeyDown(Keys.Control)
                    .SendKeys("v")
                    .KeyUp(Keys.Control)
                    .Perform();
        }
        /// <summary>Send a specific key to an element (Enter, Tab, Escape, etc.)</summary>
        protected void SendKey(By locator, string key)
        {
            var element = Wait.UntilVisible(locator);
            element.SendKeys(key);
        }

        /// <summary>Clear an input field completely.</summary>
        protected void Clear(By locator)
        {
            Wait.UntilVisible(locator).Clear();
        }

        // ── Date Actions ───────────────────────────────────────────────────────

        /// <summary>
        /// Set a date field value using JavaScript to bypass datepicker popups.
        /// Format: "dd-MM-yyyy" or whatever your ERP expects.
        /// </summary>
        protected void SetDate(By locator, string date)
        {
            if (string.IsNullOrWhiteSpace(date)) return;

            var element = Wait.UntilVisible(locator);
            ScrollIntoView(element);

            ((IJavaScriptExecutor)Driver)
                .ExecuteScript("arguments[0].value = arguments[1];", element, date);

            // Trigger change event so ERP form reacts
            ((IJavaScriptExecutor)Driver)
                .ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", element);
        }

        // ── Checkbox / Radio Actions ───────────────────────────────────────────

        /// <summary>Check a checkbox if it is not already checked.</summary>
        protected void Check(By locator)
        {
            var element = Wait.UntilVisible(locator);
            if (!element.Selected) element.Click();
        }

        /// <summary>Uncheck a checkbox if it is currently checked.</summary>
        protected void Uncheck(By locator)
        {
            var element = Wait.UntilVisible(locator);
            if (element.Selected) element.Click();
        }

        // ── Read / Get Actions ─────────────────────────────────────────────────

        /// <summary>Get the visible text of an element.</summary>
        protected string GetText(By locator)
        {
            return Wait.UntilVisible(locator).Text.Trim();
        }

        /// <summary>Get the value attribute of an input field.</summary>
        protected string GetValue(By locator)
        {
            return Wait.UntilVisible(locator).GetAttribute("value")?.Trim() ?? string.Empty;
        }

        /// <summary>Get any HTML attribute of an element.</summary>
        protected string GetAttribute(By locator, string attribute)
        {
            return Wait.UntilVisible(locator)
                      .GetAttribute(attribute)?.Trim() ?? string.Empty;
        }

        /// <summary>Check if an element is currently visible on the page.</summary>
        protected bool IsVisible(By locator)
        {
            try
            {
                return Wait.UntilVisible(locator, 2) != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Check if an element is enabled (not disabled/readonly).</summary>
        protected bool IsEnabled(By locator)
        {
            try
            {
                return Wait.UntilClickable(locator, 2) != null;
            }
            catch
            {
                return false;
            }
        }

        // ── Scroll Actions ─────────────────────────────────────────────────────

        /// <summary>Scroll element into the visible viewport.</summary>
        protected void ScrollIntoView(IWebElement element)
        {
            ((IJavaScriptExecutor)Driver)
                .ExecuteScript("arguments[0].scrollIntoView({block:'center'});", element);
        }

        /// <summary>Scroll to top of page.</summary>
        protected void ScrollToTop()
        {
            ((IJavaScriptExecutor)Driver)
                .ExecuteScript("window.scrollTo(0, 0);");
        }

        /// <summary>Scroll to bottom of page.</summary>
        protected void ScrollToBottom()
        {
            ((IJavaScriptExecutor)Driver)
                .ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
        }

        // ── Wait + Retry helpers ───────────────────────────────────────────────

        /// <summary>
        /// Retry an action up to N times with a delay between attempts.
        /// Swallows exceptions until the last attempt.
        /// </summary>
        protected void RetryAction(Action action, int maxAttempts = 0, int delayMs = 0)
        {
            int attempts = maxAttempts > 0 ? maxAttempts : Config.MaxRetryAttempts;
            int delay = delayMs > 0 ? delayMs : Config.RetryDelayMs;

            for (int i = 1; i <= attempts; i++)
            {
                try
                {
                    action();
                    return; // Success — exit
                }
                catch when (i < attempts)
                {
                    Wait.WaitForSeconds(delay / 1000);
                }
            }
        }

        // ── Loader ────────────────────────────────────────────────────────────
        /// <summary>
        /// Wait for a loading spinner / overlay to disappear.
        /// Override the locator in derived classes for your ERP's specific spinner.
        /// </summary>
        protected virtual void WaitForLoader(By? loaderLocator = null)
        {
            loaderLocator ??= By.Id("LoadingPanel");
            //By.CssSelector(".loading-overlay, .spinner, [data-loading='true']");
            Wait.UntilInvisible(loaderLocator, 3);
        }

        // ── Dropdown/Lookup Actions ───────────────────────────────────────────────────

        protected void OpenDropdown(By locator)
        {
            Click(locator);
            Wait.WaitForSeconds(1);
        }
        protected void SelectOption(By lookupText, By nextPage, string? optionText)
        {
            if (string.IsNullOrWhiteSpace(optionText)) return;

            while (true)
            {
                var options = Wait.UntilAllVisible(lookupText);
                Wait.WaitForSeconds(1);

                foreach (var option in options)
                {
                    string actualValue = option.Text.Trim();
                    if (actualValue.Contains(optionText, StringComparison.OrdinalIgnoreCase))
                    {
                        option.Click();
                        Wait.WaitForSeconds(1);
                        return;
                    }
                }
                //var nextButton = Driver.FindElement(nextPage);
                var nextButton = Wait.UntilVisible(nextPage);
                if (IsDisabled(nextButton))
                {
                    throw new NoSuchElementException(
                        $"[Lookup] Option '{optionText}' not found.");
                }
                Click(nextButton);
                Wait.WaitForSeconds(1);
            }
        }
        /// <summary>Select from a standard HTML &lt;select&gt; by visible text.</summary>
        protected void SelectByText(By locator, string text)
        {
            var element = Wait.UntilVisible(locator);
            new SelectElement(element).SelectByText(text);
        }

        /// <summary>Select from a standard HTML &lt;select&gt; by value attribute.</summary>
        protected void SelectByValue(By locator, string value)
        {
            var element = Wait.UntilVisible(locator);
            new SelectElement(element).SelectByValue(value);
        }

        /// <summary>
        /// Select from a custom ERP dropdown (non-native select).
        /// Clicks the dropdown trigger, waits for options, then clicks the matching option.
        /// </summary>
        protected void SelectFromCustomDropdown(By triggerLocator, By optionLocator, string? optionText)
        {
            //Click(triggerLocator);

            var options = Wait.UntilAllVisible(optionLocator);
            var match = options.FirstOrDefault(o =>
                o.Text.Trim().Equals(optionText, StringComparison.OrdinalIgnoreCase));

            if (match == null)
                throw new NoSuchElementException(
                    $"[Dropdown] Option '{optionText}' not found in dropdown.");

            Click(match);
        }

        // ── Utility ───────────────────────────────────────────────────────────
        protected IReadOnlyList<IWebElement> GetElements(By locator)
        {
            return Driver.FindElements(locator);
        }
        protected bool IsDisabled(IWebElement element)
        {
            return element.GetAttribute("class")
                .Contains("dx-state-disabled", StringComparison.OrdinalIgnoreCase);
        }

        public void SwitchToInterface(string target)
        {
            try
            {
                var switchLocator = By.XPath("//div[contains(@id,'MainMenu_DXI25_T')]//span[contains(@class,'dx-vam')]");

                if (!IsVisible(switchLocator))
                {
                    Report.Warning("Switch interface option not visible.");
                    return;
                }

                var currentText = Wait.UntilVisible(switchLocator).Text.Trim();

                bool shouldSwitch =
                    target.Equals("OLD", StringComparison.OrdinalIgnoreCase)
                    && currentText.Contains("old interface", StringComparison.OrdinalIgnoreCase)

                    ||

                    target.Equals("NEW", StringComparison.OrdinalIgnoreCase)
                    && currentText.Contains("new interface", StringComparison.OrdinalIgnoreCase);

                if (shouldSwitch)
                {
                    Report.Info($"Switching to {target} interface...");

                    Click(switchLocator);
                    WaitForLoader();

                    Report.Info($"Switched to {target} interface successfully.");
                }
                else
                {
                    Report.Info($"Already in {target} interface.");
                }
            }
            catch (Exception ex)
            {
                Report.Fail($"Failed to switch interface: {ex.Message}");
                throw;
            }
        }
    }
}
