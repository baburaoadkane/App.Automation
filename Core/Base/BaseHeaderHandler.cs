using App.Automation.Core.Interfaces;
using App.Automation.Core.Utilities;
using OpenQA.Selenium;

namespace App.Automation.Core.Base;

public abstract class BaseHeaderHandler<THeader> : BaseHandler, IHeaderHandler<THeader>
{
    protected BaseHeaderHandler(IWebDriver driver, WaitHelper wait, ReportHelper report)
        : base(driver, wait, report)
    {
    }

    /// <summary>
    /// Fill the transaction header.
    /// Each transaction implements its own field mapping
    /// and transaction-specific fields.
    /// </summary>
    public abstract void Fill(THeader header);
}