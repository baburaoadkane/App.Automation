using App.Automation.Core.Utilities;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;

namespace App.Automation.Core.Base;

[TestFixture]
public abstract class BaseTest
{
    // ── Protected driver — accessible by all test classes ──────────────────
    protected IWebDriver Driver { get; private set; } = null!;

    // ── Shared helpers — available to every test class ─────────────────────
    protected ConfigReader Config => ConfigReader.Instance;
    protected WaitHelper Wait { get; private set; } = null!;
    protected ReportHelper Report { get; private set; } = null!;
    protected LoginHelper LoginHelper { get; private set; } = null!;

    // ── One-time setup: runs once before ALL tests in the suite ────────────
    [OneTimeSetUp]
    public virtual void OneTimeSetUp()
    {
        ReportHelper.InitializeReport(
            outputPath: Config.ReportsOutput,
            reportTitle: Config.Get("Application:Name"),
            environment: Config.Get("Application:Environment"),
            browser: Config.BrowserType.ToString()
        );
    }

    // ── Per-test setup: runs before EACH test ──────────────────────────────
    [SetUp]
    public virtual void SetUp()
    {
        // 1. Create driver
        Driver = DriverFactory.GetDriver();

        // 2. Initialize helpers with this driver
        Wait = new WaitHelper(Driver, Config.ExplicitWait);
        Report = new ReportHelper(TestContext.CurrentContext.Test.Name);
        LoginHelper = new LoginHelper(Driver, Wait);

        // 3. Navigate to application
        Driver.Navigate().GoToUrl(Config.BaseUrl);

        // 4. Log test start
        Report.Info($"Test Started: {TestContext.CurrentContext.Test.Name}");
        Report.Info($"URL: {Config.BaseUrl}");
        Report.Info($"Browser: {Config.BrowserType}");

        // 5. Login to ERP
        LoginHelper.Login(Config.AdminUsername, Config.AdminPassword);
        Report.Info($"Logged in as: {Config.AdminUsername}");
    }

    // ── Per-test teardown: runs after EACH test ────────────────────────────
    [TearDown]
    public virtual void TearDown()
    {
        var outcome = TestContext.CurrentContext.Result.Outcome.Status;

        // Capture screenshot on failure
        if (outcome == TestStatus.Failed)
        {
            string screenshotPath = CaptureScreenshot();

            Report.Fail(
                $"Test Failed: {TestContext.CurrentContext.Result.Message}",
                screenshotPath
            );
        }
        else if (outcome == TestStatus.Passed)
        {
            Report.Pass("Test Passed Successfully.");

            if (Config.AttachScreenshotOnPass)
            {
                string screenshotPath = CaptureScreenshot();
                Report.AttachScreenshot(screenshotPath);
            }
        }
        else
        {
            Report.Skip($"Test Skipped: {TestContext.CurrentContext.Result.Message}");
        }

        // Dispose the driver instance assigned to this property (satisfies NUnit1032)
        try
        {
            Driver?.Dispose();
        }
        catch
        {
            // swallow any disposal exceptions — factory cleanup will also run
        }
        finally
        {
            // Ensure factory cleanup still runs if present
            DriverFactory.QuitDriver();

            // Clear the property reference so it's not considered alive
            Driver = null!;
        }
    }

    // ── One-time teardown: runs once after ALL tests in suite ──────────────
    [OneTimeTearDown]
    public virtual void OneTimeTearDown()
    {
        ReportHelper.FlushReport();
    }

    // ── Protected helpers available to child test classes ──────────────────

    /// <summary>
    /// Capture a screenshot and save it to the configured screenshots folder.
    /// Returns the full file path of the saved screenshot.
    /// </summary>
    protected string CaptureScreenshot()
    {
        return ScreenshotHelper.Capture(
            driver: Driver,
            testName: TestContext.CurrentContext.Test.Name,
            outputPath: Config.ScreenshotsOutput
        );
    }
}
