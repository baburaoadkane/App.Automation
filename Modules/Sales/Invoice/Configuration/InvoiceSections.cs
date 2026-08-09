using App.Automation.Core.Engine;
using App.Automation.Modules.Global.Sections;
using App.Automation.Modules.Sales.Invoice.DataModels;
using App.Automation.Modules.Sales.Invoice.HeaderHandlers;
using App.Automation.Modules.Sales.Invoice.LineHandlers;

namespace App.Automation.Modules.Sales.Invoice.Configuration;

public static class InvoiceSections
{
    public static List<SectionDefinition<InvoiceDM>> Create(
        InvoiceHeaderHandler headerHandler,
        InvoiceLineHandler lineHandler,
        ChargesHandler chargesHandler,
        DiscountHandler discountHandler,
        PaymentsHandler paymentsHandler,
        OthersHandler othersHandler)
    {
        return new List<SectionDefinition<InvoiceDM>>
        {
            // ─────────────────────────────────────────────
            // HEADER
            // ─────────────────────────────────────────────
            new()
            {
                Name = "Header",
                //Order = 10,

                ShouldRun = d => d.Header != null,

                Action = d =>
                {
                    headerHandler.Fill(d.Header);
                }
            },

            // ─────────────────────────────────────────────
            // DISCOUNT
            // ─────────────────────────────────────────────
            new()
            {
                Name = "Discount",
                //Order = 20,

                ShouldRun = d =>
                    d.Discount != null &&
                    d.Discount.HasData(),

                Action = d =>
                {
                    discountHandler.Fill(d.Discount);
                }
            },

            // ─────────────────────────────────────────────
            // LINES
            // ─────────────────────────────────────────────
            new()
            {
                Name = "Lines",
                //Order = 30,

                ShouldRun = d =>
                    d.Lines?.Any() == true,

                Action = d =>
                {
                    lineHandler.Fill(d.Lines);
                }
            },

            // ─────────────────────────────────────────────
            // CHARGES
            // ─────────────────────────────────────────────
            new()
            {
                Name = "Charges",
                //Order = 40,

                ShouldRun = d =>
                    d.Charges?.Items?.Any() == true &&
                    d.AppPreference.IsChargesEnabled,

                Action = d =>
                {
                    chargesHandler.Fill(d.Charges);
                }
            },

            // ─────────────────────────────────────────────
            // PAYMENTS
            // ─────────────────────────────────────────────
            new()
            {
                Name = "Payments",
                //Order = 50,

                ShouldRun = d =>
                    d.Payments?.Entries?.Any() == true,

                Action = d =>
                {
                    paymentsHandler.Fill(d.Payments);
                }
            },

            // ─────────────────────────────────────────────
            // OTHERS
            // ─────────────────────────────────────────────
            new()
            {
                Name = "Others",
                //Order = 60,

                ShouldRun = d =>
                    d.Others?.HasData() == true,

                Action = d =>
                {
                    othersHandler.Fill(d.Others);
                }
            }
        };
    }
}