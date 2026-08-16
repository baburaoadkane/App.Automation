using App.Automation.Core.Base;
using App.Automation.Core.Utilities;
using OpenQA.Selenium;

namespace App.Automation.Modules.Sales.Invoice.Approval;

public class InvoiceApprovalHandler : BaseHandler
{
    public InvoiceApprovalHandler(
        IWebDriver driver,
        WaitHelper wait,
        ReportHelper report)
        : base(driver, wait, report)
    {
    }

    // ================================================================
    // SUBMIT FOR APPROVAL
    // ================================================================

    public void Submit()
    {
        Report.Info("Submitting invoice for approval.");

        ClickOnButton("Submit");

        WaitForLoader();

        Report.Info("Invoice submitted for approval.");
    }

    // ================================================================
    // APPROVE
    // ================================================================

    public void Approve(string? comments = null)
    {
        Report.Info("Approving invoice.");

        ClickOnButton("Approve");

        WaitForLoader();

        if (!string.IsNullOrWhiteSpace(comments))
        {
            Report.Info(
                $"Approve comment provided: {comments}");

            IAlert alert = Wait.UntilAlertPresent();

            alert.SendKeys(comments);
            alert.Accept();
        }

        Report.Info("Invoice approved.");
    }

    // ================================================================
    // REJECT
    // ================================================================

    public void Reject(string? comments = null)
    {
        Report.Info("Rejecting invoice.");

        ClickOnButton("Reject");

        WaitForLoader();

        if (!string.IsNullOrWhiteSpace(comments))
        {
            Report.Info(
                $"Reject comment provided: {comments}");

            IAlert alert = Wait.UntilAlertPresent();

            alert.SendKeys(comments);
            alert.Accept();
        }        

        Report.Info("Invoice rejected.");
    }

    // ================================================================
    // REVISE
    // ================================================================

    public void Revise(string? comments = null)
    {
        Report.Info("Requesting invoice revision.");        

        ClickOnButton("Revise");

        WaitForLoader();

        if (!string.IsNullOrWhiteSpace(comments))
        {
            Report.Info(
                $"Revision comment provided: {comments}");

            IAlert alert = Wait.UntilAlertPresent();

            alert.SendKeys(comments);
            alert.Accept();
        }

        Report.Info("Invoice sent for revision.");
    }

    // ================================================================
    // DELEGATE
    //
    // Kept as a separate capability.
    // It is NOT part of the normal approval workflow.
    // ================================================================

    public void Delegate()
    {
        Report.Info("Delegating invoice approval.");

        ClickOnButton("Delegate");

        WaitForLoader();

        Report.Info("Invoice approval delegated.");
    }

    // ================================================================
    // BUTTON
    // ================================================================

    private void ClickOnButton(string buttonText)
    {
        By button = By.XPath($"//span[contains(@class, 'dx-vam') and text()='{buttonText}']");
        Wait.UntilClickable(button).Click();
        // By.XPath($"//span[normalize-space()='{buttonText}']");
    }

    // ================================================================
    // LOADER
    // ================================================================

    private void WaitForLoader()
    {
        By loader = By.Id("LoadingPanel");

        try
        {
            Wait.UntilInvisible(
                loader,
                timeoutSeconds: 5);
        }
        catch
        {
            // Loader may not appear.
        }
    }
}