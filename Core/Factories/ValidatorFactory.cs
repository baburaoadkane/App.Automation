using App.Automation.Core.Enums;
using App.Automation.Core.Interfaces;
using App.Automation.Core.Utilities;
using App.Automation.Modules.Global.Sections;
using App.Automation.Modules.Global.Validators;
using App.Automation.Modules.Sales.Invoice.Validators;
using OpenQA.Selenium;

namespace App.Automation.Core.Factories;

public static class ValidatorFactory
{
    public static IValidator Create(
        ModuleType module,
        TransactionType transaction,
        string validationType,
        IWebDriver driver,
        WaitHelper wait,
        ReportHelper report,
        ExpectationHandler expectationHandler)
    {
        return validationType.ToUpperInvariant() switch
        {
            "HEADER" =>
                CreateHeaderValidator(
                    module,
                    transaction,
                    driver,
                    wait,
                    report,
                    expectationHandler),

            "LINES" =>
                CreateLinesValidator(
                    module,
                    transaction,
                    driver,
                    wait,
                    report,
                    expectationHandler),

            "TOTALS" =>
                new TotalsValidator(
                    driver,
                    wait,
                    report,
                    expectationHandler),

            "MESSAGE" =>
                new MessageValidator(
                    driver,
                    wait,
                    report,
                    expectationHandler),

            _ => throw new NotSupportedException(
                $"Validator not configured for " +
                $"Module='{module}', " +
                $"Transaction='{transaction}', " +
                $"Type='{validationType}'.")
        };
    }

    private static IValidator CreateHeaderValidator(
        ModuleType module,
        TransactionType transaction,
        IWebDriver driver,
        WaitHelper wait,
        ReportHelper report,
        ExpectationHandler expectationHandler)
    {
        return (module, transaction) switch
        {
            (ModuleType.Sales, TransactionType.Invoice) =>
                new HeaderValidator(
                    driver,
                    wait,
                    report,
                    expectationHandler),

            _ => throw new NotSupportedException(
                $"HeaderValidator not configured for " +
                $"Module='{module}', Transaction='{transaction}'.")
        };
    }

    private static IValidator CreateLinesValidator(
        ModuleType module,
        TransactionType transaction,
        IWebDriver driver,
        WaitHelper wait,
        ReportHelper report,
        ExpectationHandler expectationHandler)
    {
        return (module, transaction) switch
        {
            (ModuleType.Sales, TransactionType.Invoice) =>
                new LinesValidator(
                    driver,
                    wait,
                    report,
                    expectationHandler),

            _ => throw new NotSupportedException(
                $"LinesValidator not configured for " +
                $"Module='{module}', Transaction='{transaction}'.")
        };
    }
}
