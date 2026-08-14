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

    public void Approve()
    { 
        Report.Info("Approving invoice.");

        ClickOnButton("Approve");
        
        WaitForLoader();

        Report.Info("Invoice approved.");
    }

    // ================================================================
    // REJECT
    // ================================================================

    public void Reject(string? comments = null)
    {
        Report.Info("Rejecting invoice.");

        if (!string.IsNullOrWhiteSpace(comments))
        {
            Report.Info(
                $"Reject comment provided: {comments}");

            // TODO:
            // Enter rejection comment here when the
            // actual ERP comment field/locator is known.
        }

        ClickOnButton("Reject");

        WaitForLoader();

        Report.Info("Invoice rejected.");
    }

    // ================================================================
    // REVISE
    // ================================================================

    public void Revise(string? comments = null)
    {
        Report.Info("Requesting invoice revision.");

        if (!string.IsNullOrWhiteSpace(comments))
        {
            Report.Info(
                $"Revision comment provided: {comments}");

            // TODO:
            // Enter revision comment here when the
            // actual ERP comment field/locator is known.
        }

        ClickOnButton("Revise");

        WaitForLoader();

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
        By button = By.XPath(
            $"//span[contains(@class,'dx-vam') and " +
            $"normalize-space()='{buttonText}']" +
            $" | " +
            $"//button[normalize-space()='{buttonText}']"
        );

        Wait.UntilClickable(button).Click();
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