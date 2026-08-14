using App.Automation.Core.Utilities;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Automation.Core.Base;

public class AppHeaderHandler : BaseHandler
{
    public AppHeaderHandler(
        IWebDriver driver,
        WaitHelper wait,
        ReportHelper report)
        : base(driver, wait, report)
    {
    }

    // ── Section-level locators ─────────────────────────────────────────────
    private static readonly By Notification = By.XPath("//div[@title='Notification']");
    private static readonly By Global = By.XPath("//div[@title='Global']");
    private static readonly By MyApprovals = By.XPath("//span[normalize-space()='My Approvals']");

    public void ClickOnNotification()
    {
        Click(Notification);
    }

    public void ClickOnGlobal()
    {
        Click(Global);
    }

    public void ClickOnMyApprovals()
    {
        Click(MyApprovals);
    }
}

