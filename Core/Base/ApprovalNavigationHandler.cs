using App.Automation.Core.Utilities;
using OpenQA.Selenium;

namespace App.Automation.Core.Base;

public class ApprovalNavigationHandler : BaseHandler
{
    public ApprovalNavigationHandler(
        IWebDriver driver,
        WaitHelper wait,
        ReportHelper report)
        : base(driver, wait, report)
    {
    }

    // ================================================================
    // SECTION LOCATORS
    // ================================================================

    private static readonly By Notification =
        By.XPath("//div[@title='Notification']");

    private static readonly By Global =
        By.XPath("//div[@title='Global']");

    private static readonly By MyApprovals =
        By.XPath("//span[normalize-space()='My Approvals']");


    // ================================================================
    // NAVIGATION
    // ================================================================

    public void ClickOnNotification()
    {
        Report.Info("Clicking Notification.");

        Wait.UntilClickable(Notification).Click();
    }

    public void ClickOnGlobal()
    {
        Report.Info("Clicking Global.");

        Wait.UntilClickable(Global).Click();
    }

    public void ClickOnMyApprovals()
    {
        Report.Info("Clicking My Approvals.");

        Wait.UntilClickable(MyApprovals).Click();
    }    

    public void FindAndOpenApprovalTransaction(
        string documentNo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentNo);           

        // Capture windows BEFORE clicking the card
        var existingWindows =
            Window.GetCurrentWindows();

        // Find and click the matching approval card
        Lookup.SelectCard(documentNo);

        // Switch to the newly opened tab/window
        Window.SwitchToNewWindow(existingWindows);

        Report.Info(
            $"Switched to approval transaction window: {documentNo}");
    }
}