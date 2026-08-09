using App.Automation.Core.Enums;
using App.Automation.Core.Interfaces;
using App.Automation.Core.Utilities;
using App.Automation.Modules.Global.Sections;
using App.Automation.Modules.Sales.Invoice.HeaderHandlers;
using App.Automation.Modules.Sales.Invoice.LineHandlers;
using OpenQA.Selenium;

namespace App.Automation.Core.Factories;

public static class HandlerFactory
{
    public static IHandler Create(
        ModuleType module,
        TransactionType transaction,
        string section,
        IWebDriver driver,
        WaitHelper wait,
        ReportHelper report)
    {
        return section.ToUpperInvariant() switch
        {
            "HEADER" =>
                CreateHeaderHandler(
                    module,
                    transaction,
                    driver,
                    wait,
                    report),

            "LINES" =>
                CreateLineHandler(
                    module,
                    transaction,
                    driver,
                    wait,
                    report),

            "CHARGES" =>
                new ChargesHandler(driver, wait, report),

            "DISCOUNT" =>
                new DiscountHandler(driver, wait, report),

            "PAYMENTS" =>
                new PaymentsHandler(driver, wait, report),

            "OTHERS" =>
                new OthersHandler(driver, wait, report),

            "ENTITY" =>
                new EntityHandler(driver, wait, report),

            _ => throw new NotSupportedException(
                $"Handler not configured for " +
                $"Module='{module}', " +
                $"Transaction='{transaction}', " +
                $"Section='{section}'.")
        };
    }

    private static IHandler CreateHeaderHandler(
        ModuleType module,
        TransactionType transaction,
        IWebDriver driver,
        WaitHelper wait,
        ReportHelper report)
    {
        return (module, transaction) switch
        {
            (ModuleType.Sales, TransactionType.Invoice) =>
                new InvoiceHeaderHandler(driver, wait, report),

            _ => throw new NotSupportedException(
                $"HeaderHandler not configured for " +
                $"Module='{module}', Transaction='{transaction}'.")
        };
    }

    private static IHandler CreateLineHandler(
        ModuleType module,
        TransactionType transaction,
        IWebDriver driver,
        WaitHelper wait,
        ReportHelper report)
    {
        return (module, transaction) switch
        {
            (ModuleType.Sales, TransactionType.Invoice) =>
                new InvoiceLineHandler(driver, wait, report),

            _ => throw new NotSupportedException(
                $"LineHandler not configured for " +
                $"Module='{module}', Transaction='{transaction}'.")
        };
    }
}