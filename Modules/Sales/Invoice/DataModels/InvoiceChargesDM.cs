namespace App.Automation.Modules.Sales.Invoice.DataModels;

public class InvoiceChargesDM
{
    /// <summary>List of individual charge entries.</summary>
    public List<ChargeDM> Items { get; set; } = new();
}

public class ChargeDM
{
    /// <summary>Charge type / description. Example: "Freight", "Packing Charges"</summary>
    public string? ChargeType { get; set; }
    public string? AccountType { get; set; }
    public string? Account { get; set; }
    public string? Currency { get; set; }

    /// <summary>Charge amount.</summary>
    public decimal AmountFC { get; set; }
    public decimal AmountLC { get; set; }
    public string? Remarks { get; set; }

    /// <summary>Tax type applied on this charge. Example: "GST 18%"</summary>
    public string? TaxType { get; set; }

    /// <summary>Whether this charge is taxable.</summary>
    public bool IsTaxable { get; set; }
}
