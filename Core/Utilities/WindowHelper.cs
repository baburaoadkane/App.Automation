using OpenQA.Selenium;

namespace App.Automation.Core.Utilities;

public class WindowHelper
{
    private readonly IWebDriver _driver;
    private readonly WaitHelper _wait;
    private readonly ReportHelper _report;

    public WindowHelper(
        IWebDriver driver,
        WaitHelper wait,
        ReportHelper report)
    {
        _driver = driver;
        _wait = wait;
        _report = report;
    }

    // ================================================================
    // CAPTURE CURRENT WINDOWS
    // ================================================================

    public HashSet<string> GetCurrentWindows()
    {
        return _driver.WindowHandles.ToHashSet();
    }

    // ================================================================
    // SWITCH TO NEW WINDOW
    // ================================================================

    public void SwitchToNewWindow(
        HashSet<string> existingWindows,
        int timeoutSeconds = 10)
    {
        ArgumentNullException.ThrowIfNull(existingWindows);

        _report.Info("Waiting for new browser window/tab.");

        string? newWindow = null;

        var endTime =
            DateTime.Now.AddSeconds(timeoutSeconds);

        while (DateTime.Now < endTime)
        {
            var currentWindows =
                _driver.WindowHandles;

            newWindow =
                currentWindows.FirstOrDefault(
                    handle => !existingWindows.Contains(handle));

            if (!string.IsNullOrEmpty(newWindow))
                break;

            Thread.Sleep(300);
        }

        if (string.IsNullOrEmpty(newWindow))
        {
            throw new InvalidOperationException(
                "New browser window/tab was not opened.");
        }

        _driver.SwitchTo().Window(newWindow);

        _report.Info(
            $"Switched to new browser window/tab: {newWindow}");

        _wait.UntilPageLoaded();
    }

    // ================================================================
    // SWITCH TO WINDOW BY HANDLE
    // ================================================================

    public void SwitchToWindow(string windowHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(windowHandle);

        _driver.SwitchTo().Window(windowHandle);

        _report.Info(
            $"Switched to browser window: {windowHandle}");
    }

    // ================================================================
    // CURRENT WINDOW
    // ================================================================

    public string GetCurrentWindow()
    {
        return _driver.CurrentWindowHandle;
    }

    // ================================================================
    // CLOSE CURRENT WINDOW AND RETURN
    // ================================================================

    public void CloseCurrentAndSwitchTo(
        string windowHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(windowHandle);

        _driver.Close();

        _driver.SwitchTo().Window(windowHandle);

        _report.Info(
            "Closed current window and switched back.");
    }
}