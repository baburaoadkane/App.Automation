using App.Automation.Core.Enums;
using App.Automation.Core.Interfaces;
using App.Automation.Core.Utilities;
using App.Automation.Modules.Sales.Invoice.Executors;
using OpenQA.Selenium;

namespace App.Automation.Core.Factories;

public static class ExecutorFactory
{
    public static IExecutor<TDocument> Create<TDocument>(
        ModuleType module,
        TransactionType transaction,
        IWebDriver driver,
        WaitHelper wait,
        ReportHelper report)
    {
        return (module, transaction) switch
        {
            (ModuleType.Sales, TransactionType.Invoice) =>
                (IExecutor<TDocument>)(object)new InvoiceExecutor(
                    driver,
                    wait,
                    report),

            _ => throw new NotSupportedException(
                $"Executor not configured for " +
                $"Module='{module}', " +
                $"Transaction='{transaction}'.")
        };
    }
}
