namespace App.Automation.Modules.Sales.Invoice.DataModels;

public class InvoiceDiscountDM
{
    /// <summary>Discount applied in percent.</summary>
    public decimal DiscountInPercent { get; set; }

    /// <summary>Discount applied in absolute value.</summary>
    public decimal DiscountValue { get; set; }

    public bool HasData()
    {
        return DiscountInPercent > 0 || DiscountValue > 0;
    }
}
