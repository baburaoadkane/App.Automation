using App.Automation.Core.Base;
using App.Automation.Core.Utilities;
using OpenQA.Selenium;

namespace App.Automation.Modules.Sales.Invoice.Approval;

public class InvoiceApprovalHandler : BaseHandler
{
    private readonly AlertHelper _alertHelper;

    public InvoiceApprovalHandler(
        IWebDriver driver,
        WaitHelper wait,
        ReportHelper report)
        : base(driver, wait, report)
    {
        _alertHelper = new AlertHelper(driver, wait);
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

        _alertHelper.AcceptPrompt(comments?? "Approved");

        WaitForLoader();

        Report.Info(
            $"Approve comment provided: {comments}");

        Report.Info("Invoice approved.");
    }

    // ================================================================
    // REJECT
    // ================================================================

    public void Reject(string? comments = null)
    {
        Report.Info("Rejecting invoice.");

        OpenKebabMenu();

        ClickOnButton("Reject");

        // JavaScript prompt
        _alertHelper.AcceptPrompt(comments ?? "Rejected");

        WaitForLoader();

        Report.Info(
            $"Reject comment provided: {comments ?? "Rejected"}");

        Report.Info("Invoice rejected.");
    }

    // ================================================================
    // REVISE
    // ================================================================

    public void Revise(string? comments = null)
    {
        Report.Info("Requesting invoice revision.");

        OpenKebabMenu();

        ClickOnButton("Revise");

        // JavaScript prompt
        _alertHelper.AcceptPrompt(comments ?? "Revise");

        WaitForLoader();

        Report.Info(
            $"Revision comment provided: {comments ?? "Revise"}");

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
    }

    private void OpenKebabMenu()
    {
        By kebabMenu = By.XPath("//img[contains(@id, 'MainMenu_DXI') and @alt='...']");
        Wait.UntilClickable(kebabMenu).Click();
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