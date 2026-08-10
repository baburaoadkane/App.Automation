using App.Automation.Core.Engine;
using App.Automation.Modules.Global.Sections;
using App.Automation.Modules.Sales.Invoice.DataModels;
using App.Automation.Modules.Sales.Invoice.LineHandlers;

namespace App.Automation.Modules.Sales.Invoice.Configuration;

public static class InvoiceSections
{
    /// <summary>
    /// Creates the sections that are executed after the invoice header
    /// has been filled and saved.
    ///
    /// Execution order:
    ///     Lines
    ///     Discount
    ///     Charges
    ///     Payments
    ///     Others
    /// </summary>
    public static List<SectionDefinition<InvoiceDM>> Create(
        InvoiceLineHandler lineHandler,
        DiscountHandler discountHandler,
        ChargesHandler chargesHandler,
        PaymentsHandler paymentsHandler,
        OthersHandler othersHandler)
    {
        return new List<SectionDefinition<InvoiceDM>>
        {
            // =============================================================
            // LINES
            // =============================================================
            new()
            {
                Name = "Lines",

                ShouldRun = d =>
                    d.Lines?.Any() == true,

                Action = d =>
                {
                    lineHandler.Fill(d.Lines!);
                },

                RequiresSave = true
            },

            // =============================================================
            // DISCOUNT
            // =============================================================
            new()
            {
                Name = "Discount",

                ShouldRun = d =>
                    d.Discount != null &&
                    d.Discount.HasData(),

                Action = d =>
                {
                    discountHandler.Fill(d.Discount!);
                },

                RequiresSave = true
            },

            // =============================================================
            // CHARGES
            // =============================================================
            new()
            {
                Name = "Charges",

                ShouldRun = d =>
                    d.AppPreference?.IsChargesEnabled == true &&
                    d.Charges?.Items?.Any() == true,

                Action = d =>
                {
                    chargesHandler.Fill(d.Charges!);
                },

                RequiresSave = true
            },

            // =============================================================
            // PAYMENTS
            // =============================================================
            new()
            {
                Name = "Payments",

                ShouldRun = d =>
                    d.TxnParameter?.UseMultiplePaymentMethod == true &&
                    d.Payments?.Entries?.Any() == true,

                Action = d =>
                {
                    paymentsHandler.Fill(d.Payments!);
                },

                RequiresSave = true
            },

            // =============================================================
            // OTHERS
            // =============================================================
            new()
            {
                Name = "Others",

                ShouldRun = d =>
                    d.Others?.HasData() == true,

                Action = d =>
                {
                    othersHandler.Fill(d.Others!);
                },

                RequiresSave = true
            }
        };
    }
}