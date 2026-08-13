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

    public void Submit()
    {
        Report.Info("Submitting invoice for approval.");

        ClickOnButton("Submit");

        WaitForLoader();

        Report.Info("Invoice submitted for approval.");
    }

    public void Approve()
    {
        Report.Info("Approving invoice.");

        ClickOnButton("Approve");

        WaitForLoader();

        Report.Info("Invoice approved.");
    }

    public void Reject()
    {
        Report.Info("Rejecting invoice.");

        ClickOnButton("Reject");

        WaitForLoader();

        Report.Info("Invoice rejected.");
    }

    public void Revise()
    {
        Report.Info("Requesting invoice revision.");

        ClickOnButton("Revise");

        WaitForLoader();

        Report.Info("Invoice sent for revision.");
    }

    public void Delegate()
    {
        Report.Info("Delegating invoice approval.");

        ClickOnButton("Delegate");

        WaitForLoader();

        Report.Info("Invoice approval delegated.");
    }

    private void ClickOnButton(string buttonText)
    {
        By button = By.XPath(
            $"//span[contains(@class,'dx-vam') and normalize-space()='{buttonText}']" +
            $" | //button[normalize-space()='{buttonText}']"
        );

        Wait.UntilClickable(button).Click();
    }

    private void WaitForLoader()
    {
        By loader = By.Id("LoadingPanel");

        try
        {
            Wait.UntilInvisible(loader, timeoutSeconds: 5);
        }
        catch
        {
            // Loader may not appear.
        }
    }
}