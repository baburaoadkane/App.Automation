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

    //public void ClickOnMatchDocument(string documentNo)
    //{
    //    var documentSubtitle = Wait.UntilVisible(
    //                By.XPath("//div[contains(@class,'pa-subtitle')]"));

    //    string documentInfo = documentSubtitle.Text;

    //    if (documentInfo.Contains(documentNo))
    //    {
    //        documentSubtitle.Click();
    //    }
    //}

    public void ClickOnMatchDocument(string documentNo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentNo);

        Report.Info(
            $"Opening approval transaction: {documentNo}");

        // Store the current window
        string originalWindow = Driver.CurrentWindowHandle;

        // Store existing windows before clicking
        var existingWindows = Driver.WindowHandles.ToHashSet();

        var documentSubtitle = Wait.UntilVisible(
                      By.XPath("//div[contains(@class,'pa-subtitle')]"));

        string documentInfo = documentSubtitle.Text;

        if (documentInfo.Contains(documentNo))
        {
            Report.Info(
            $"Approval transaction found: {documentNo}");

            documentSubtitle.Click();
            Wait.WaitForSeconds(2);
        }

        //documentSubtitle.Click();

        // ================================================================
        // WAIT FOR NEW TAB / WINDOW
        // ================================================================

        Wait.Until(_ =>
            Driver.WindowHandles.Count > existingWindows.Count);

        // Find the newly opened window
        string newWindow = Driver.WindowHandles
            .First(handle => !existingWindows.Contains(handle));

        // Switch Selenium to the new window
        Driver.SwitchTo().Window(newWindow);

        Wait.UntilPageLoaded();

        Report.Info(
            $"Switched to approval transaction window: {documentNo}");
    }

    // ================================================================
    // FIND + HOVER + ACTION
    // ================================================================

    public void FindAndOpenApprovalTransaction(
        string documentNo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentNo);

        Report.Info(
            $"Finding approval transaction: {documentNo}");

        ClickOnMatchDocument(documentNo);
    }
}