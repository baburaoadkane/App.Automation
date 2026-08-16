using OpenQA.Selenium;

namespace App.Automation.Core.Utilities;

public class AlertHelper
{
    private readonly IWebDriver _driver;
    private readonly WaitHelper _wait;

    public AlertHelper(IWebDriver driver, WaitHelper wait)
    {
        _driver = driver;
        _wait = wait;
    }

    public void AcceptPrompt(string text)
    {
        IAlert alert = _wait.UntilAlertPresent();

        alert.SendKeys(text);
        alert.Accept();
    }

    public void CancelPrompt()
    {
        IAlert alert = _wait.UntilAlertPresent();

        alert.Dismiss();
    }

    public string GetAlertText()
    {
        IAlert alert = _wait.UntilAlertPresent();

        return alert.Text ?? "No Text Found";
    }
}