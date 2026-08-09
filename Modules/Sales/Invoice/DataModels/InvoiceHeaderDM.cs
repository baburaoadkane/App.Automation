namespace App.Automation.Modules.Sales.Invoice.DataModels;

public class InvoiceHeaderDM
{
    // ── Date ──────────────────────────────────────────────────────────────

    /// <summary>Invoice date. Format: dd-MM-yyyy</summary>
    public string? InvoiceDate { get; set; }

    // ── CustomerInfo ───────────────────────────────────────────────────────────

    /// <summary>Customer name or code — typed into autocomplete field.</summary>
    public string? Customer { get; set; }

    /// <summary>Customer display name — typed into field.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Customer Mobile number — typed into field.</summary>
    public string? MobileNum { get; set; }

    // ── Financial ─────────────────────────────────────────────────────────

    /// <summary>Currency code. Example: USD, INR, AED</summary>
    public string? Currency { get; set; }

    /// <summary>Price list to apply. Example: "Standard Selling"</summary>
    public string? PriceList { get; set; }

    /// <summary>Payment terms. Example: "Net 30", "Immediate"</summary>
    public string? PaymentTerm { get; set; }

    /// <summary>Payment method. Example: "Credit", "Cash", "Bank"</summary>
    public string? PaymentMethod { get; set; }

    /// <summary>Financial dimension. Example: "Cost Center", "Department"</summary>
    public string? FinancialDimension { get; set; }

    // ── Warehouse ──────────────────────────────────────────────────────────

    /// <summary>Warehouse or location the invoice is raised from.</summary>
    public string? Warehouse { get; set; }

    // ── Sales Info ────────────────────────────────────────────────────────

    /// <summary>Sales person assigned to this invoice.</summary>
    public string? Salesman { get; set; }

    /// <summary>External reference number.</summary>
    public string? ReferenceNum { get; set; }

    /// <summary>Customer purchase order number.</summary>
    public string? CustomerPONum { get; set; }

    /// <summary>Discount applied in percent.</summary>
    public decimal DiscountInPercent { get; set; }

    /// <summary>Discount applied in absolute value.</summary>
    public decimal DiscountValue { get; set; }
    /// <summary>Gets or sets the gross total amount before any deductions or taxes are applied.</summary>
    public decimal GrossTotal { get; set; }
    /// <summary>Gets or sets the net total amount after all deductions and additions are applied.</summary>
    public decimal NetTotal { get; set; }
}
