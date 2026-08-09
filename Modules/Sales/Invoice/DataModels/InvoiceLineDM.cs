namespace App.Automation.Modules.Sales.Invoice.DataModels;

public class InvoiceLineDM
{
    // ── Item ──────────────────────────────────────────────────────────────

    /// <summary>Item Barcode — scanned or typed into barcode field./// </summary>
    public string? Barcode { get; set; }
    /// <summary>Item code — typed into item search field.</summary>
    public string? Item { get; set; }

    /// <summary>Item name — used for verification / fallback search.</summary>
    public string? Description { get; set; }

    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Warehouse { get; set; }

    // ── Quantity ──────────────────────────────────────────────────────────

    /// <summary>Quantity to invoice.</summary>
    public decimal Quantity { get; set; }
    /// <summary>Unit price per UOM.</summary>
    public decimal UnitPrice { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal BonusQty { get; set; }

    /// <summary>Unit of measure. Example: Nos, Kg, Box, Ltr</summary>
    public string? UOM { get; set; }

    // ── Pricing ───────────────────────────────────────────────────────────

    /// <summary>Discount percentage on this line. Example: 10 for 10%</summary>
    public decimal DiscountInPercent { get; set; }

    public decimal DiscountValue { get; set; }
    public string? Remarks { get; set; }

    // ── Tax ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Tax type / tax code applied on this line.
    /// Supports multiple tax types — maps to the ERP's tax dropdown.
    /// Example: "GST 18%", "VAT 5%", "Tax Exempt"
    /// </summary>
    public string? TaxType { get; set; }

    /// <summary>Tax rate percentage. Example: 18 for 18%</summary>
    public decimal TaxPercent { get; set; }

    // ── Additional Fields ─────────────────────────────────────────────────


    // ── Computed (Expected) ───────────────────────────────────────────────

    /// <summary>
    /// Expected line total amount after discount and before tax.
    /// Used by LinesValidator to assert the computed value shown in ERP.
    /// Formula: (Quantity × UnitPrice) × (1 - DiscountPercent / 100)
    /// </summary>
    public decimal ExpectedLineTotal { get; set; }
}