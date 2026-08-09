using App.Automation.Core.Enums;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;

namespace App.Automation.Core.Utilities
{
    public static class DriverFactory
    {
        // ── Thread-local driver (supports parallel test execution) ─────────────
        [ThreadStatic]
        private static IWebDriver? _driver;

        private static readonly ConfigReader _config = ConfigReader.Instance;

        // ── Public API ─────────────────────────────────────────────────────────

        public static IWebDriver GetDriver()
        {
            if (_driver == null || IsDriverClosed(_driver))
                _driver = CreateDriver(_config.BrowserType);

            return _driver;
        }

        public static IWebDriver GetDriver(BrowserType browserType)
        {
            _driver = CreateDriver(browserType);
            return _driver;
        }

        /// <summary>
        /// Quits the WebDriver and clears the thread-local reference.
        /// Always call this in [TearDown] via BaseTest.
        /// </summary>
        public static void QuitDriver()
        {
            if (_driver != null && !IsDriverClosed(_driver))
            {
                try { _driver.Quit(); }
                catch { /* Driver already closed */ }
                finally { _driver = null; }
            }
        }

        // ── Private factory methods ────────────────────────────────────────────

        private static IWebDriver CreateDriver(BrowserType browserType)
        {
            IWebDriver driver = browserType switch
            {
                BrowserType.Chrome => CreateChromeDriver(),
                BrowserType.Firefox => CreateFirefoxDriver(),
                BrowserType.Edge => CreateEdgeDriver(),
                _ => throw new ArgumentOutOfRangeException(
                         nameof(browserType), $"Unsupported browser: {browserType}")
            };

            ConfigureDriver(driver);
            return driver;
        }

        private static IWebDriver CreateChromeDriver()
        {
            var options = new ChromeOptions();

            if (_config.Headless)
            {
                options.AddArgument("--headless=new");
                options.AddArgument("--disable-gpu");
            }

            options.AddArgument($"--window-size={_config.WindowSize}");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--ignore-certificate-errors");
            options.AddUserProfilePreference("profile.password_manager_enabled", false);

            // Suppress "Chrome is being controlled by automated software" bar
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddAdditionalOption("useAutomationExtension", false);

            return new ChromeDriver(options);
        }

        private static IWebDriver CreateFirefoxDriver()
        {
            var options = new FirefoxOptions();

            if (_config.Headless)
                options.AddArgument("-headless");

            options.AddArgument($"--width={_config.WindowSize.Split(',')[0]}");
            options.AddArgument($"--height={_config.WindowSize.Split(',')[1]}");

            return new FirefoxDriver(options);
        }

        private static IWebDriver CreateEdgeDriver()
        {
            var options = new EdgeOptions();

            if (_config.Headless)
            {
                options.AddArgument("--headless=new");
                options.AddArgument("--disable-gpu");
            }

            options.AddArgument($"--window-size={_config.WindowSize}");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            return new EdgeDriver(options);
        }

        private static void ConfigureDriver(IWebDriver driver)
        {
            // Page load timeout
            driver.Manage().Timeouts().PageLoad =
                TimeSpan.FromSeconds(_config.PageLoadTimeout);

            // Implicit wait — NOTE: we prefer explicit waits, keep this low
            driver.Manage().Timeouts().ImplicitWait =
                TimeSpan.FromSeconds(_config.ImplicitWait);

            // Maximize window (unless headless)
            if (!_config.Headless)
                driver.Manage().Window.Maximize();
        }

        private static bool IsDriverClosed(IWebDriver driver)
        {
            try { _ = driver.Title; return false; }
            catch { return true; }
        }
    }
}
