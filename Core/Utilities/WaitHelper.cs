using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace App.Automation.Core.Utilities
{
    public class WaitHelper
    {
        private readonly IWebDriver _driver;
        private readonly int _defaultTimeout;

        public WaitHelper(IWebDriver driver, int defaultTimeoutSeconds)
        {
            _driver = driver;
            _defaultTimeout = defaultTimeoutSeconds;
        }

        public void WaitForSeconds(int seconds)
        {
            Thread.Sleep(seconds * 1000);
        }

        // ── Element presence ───────────────────────────────────────────────────

        /// <summary>Wait until element is visible in the DOM and on screen.</summary>
        public IWebElement UntilVisible(By locator, int? timeoutSeconds = null)
        {
            return GetWait(timeoutSeconds).Until(driver =>
            {
                try
                {
                    var el = driver.FindElement(locator);
                    return el.Displayed ? el : null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });
        }

        /// <summary>Wait until element exists in DOM (may not be visible).</summary>
        public IWebElement UntilPresent(By locator, int? timeoutSeconds = null)
        {
            return GetWait(timeoutSeconds).Until(driver =>
            {
                try
                {
                    return driver.FindElement(locator);
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });
        }

        /// <summary>Wait until element is invisible or removed from DOM.</summary>
        public void UntilInvisible(By locator, int? timeoutSeconds = null)
        {
            GetWait(timeoutSeconds).Until(driver =>
            {
                try
                {
                    var elements = driver.FindElements(locator);
                    if (elements.Count == 0) return true;
                    foreach (var el in elements)
                    {
                        if (el.Displayed) return false;
                    }
                    return true;
                }
                catch (StaleElementReferenceException)
                {
                    return true;
                }
            });
        }

        // ── Clickability ───────────────────────────────────────────────────────

        /// <summary>Wait until element is visible AND enabled (ready to click).</summary>
        public IWebElement UntilClickable(By locator, int? timeoutSeconds = null)
        {
            return GetWait(timeoutSeconds).Until(driver =>
            {
                try
                {
                    var el = driver.FindElement(locator);
                    return (el.Displayed && el.Enabled) ? el : null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });
        }

        /// <summary>Wait until a located element is clickable.</summary>
        public IWebElement UntilElementClickable(IWebElement element, int? timeoutSeconds = null)
        {
            return GetWait(timeoutSeconds).Until(driver =>
            {
                try
                {
                    return (element.Displayed && element.Enabled && element.GetCssValue("pointer-events") != "none")
                    ? element
                    : null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });
        }

        // ── Text conditions ────────────────────────────────────────────────────

        /// <summary>Wait until element contains specific text.</summary>
        public void UntilTextPresent(By locator, string text, int? timeoutSeconds = null)
        {
            GetWait(timeoutSeconds).Until(driver =>
            {
                try
                {
                    var el = driver.FindElement(locator);
                    return !string.IsNullOrEmpty(el.Text) && el.Text.Contains(text, StringComparison.OrdinalIgnoreCase);
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        /// <summary>Wait until an input field has a non-empty value.</summary>
        public void UntilValuePresent(By locator, string value, int? timeoutSeconds = null)
        {
            GetWait(timeoutSeconds).Until(driver =>
            {
                try
                {
                    var el = driver.FindElement(locator);
                    var val = el.GetAttribute("value");
                    return !string.IsNullOrEmpty(val) && val.Contains(value, StringComparison.OrdinalIgnoreCase);
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        public void UntilNotPresent(By locator, int? timeoutSeconds = null)
        {
            GetWait(timeoutSeconds).Until(driver =>
                driver.FindElements(locator).Count == 0
            );
        }

        // ── URL conditions ─────────────────────────────────────────────────────

        /// <summary>Wait until URL contains a specific substring.</summary>
        public void UntilUrlContains(string urlFragment, int? timeoutSeconds = null)
        {
            GetWait(timeoutSeconds).Until(driver => driver.Url != null &&
                                                    driver.Url.Contains(urlFragment, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Wait until URL matches exactly.</summary>
        public void UntilUrlIs(string url, int? timeoutSeconds = null)
        {
            GetWait(timeoutSeconds).Until(driver => driver.Url != null &&
                                                    driver.Url.StartsWith(url, StringComparison.OrdinalIgnoreCase));
        }

        // ── Page load ──────────────────────────────────────────────────────────

        /// <summary>Wait until document.readyState === 'complete'.</summary>
        public void UntilPageLoaded(int? timeoutSeconds = null)
        {
            GetWait(timeoutSeconds).Until(driver =>
            {
                var readyState = ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState")
                    ?.ToString();

                return readyState == "complete";
            });
        }
        // ── Multiple elements ──────────────────────────────────────────────────

        /// <summary>Wait until at least one element matching locator is present.</summary>
        public IReadOnlyList<IWebElement> UntilAllVisible(By locator, int minCount = 1, int? timeoutSeconds = null)
        {
            return GetWait(timeoutSeconds).Until(driver =>
            {
                try
                {
                    var elements = driver.FindElements(locator);
                    var visible = elements.Where(e => e.Displayed).ToList();
                    WaitForSeconds(2);

                    return visible.Count >= minCount ? visible.AsReadOnly() : null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });
        }

        // ── Custom condition ───────────────────────────────────────────────────

        /// <summary>
        /// Wait for any custom condition expressed as a Func.
        /// Example: wait.Until(driver => driver.FindElements(locator).Count > 3)
        /// </summary>
        public TResult Until<TResult>(Func<IWebDriver, TResult> condition, int? timeoutSeconds = null)
        {
            return GetWait(timeoutSeconds).Until(condition);
        }

        // ── Private helpers ────────────────────────────────────────────────────
        private WebDriverWait GetWait(int? timeoutSeconds)
        {
            int timeout = timeoutSeconds ?? _defaultTimeout;

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout))
            {
                PollingInterval = TimeSpan.FromMilliseconds(300),
                Message = $"Element not found within {timeout} seconds."
            };

            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException)
            );

            return wait;
        }
        public void UntilTextNotPresent(By locator, string text, int? timeoutSeconds = null)
        {
            GetWait(timeoutSeconds).Until(driver =>
            {
                try
                {
                    var el = driver.FindElement(locator);
                    return string.IsNullOrEmpty(el.Text) ||
                           !el.Text.Contains(text, StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return true;
                }
            });
        }
        public void UntilLoaderInvisible(By loaderLocator, int? timeoutSeconds = null)
        {
            GetWait(timeoutSeconds).Until(driver =>
            {
                var loaders = driver.FindElements(loaderLocator);
                return loaders.All(l => !l.Displayed);
            });
        }
    }
}
