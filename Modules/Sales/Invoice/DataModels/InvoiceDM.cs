using App.Automation.Core.DataModels.Shared;

namespace App.Automation.Modules.Sales.Invoice.DataModels;

public class InvoiceDM : BaseDocumentDM
{
    public PreferenceDM? AppPreference { get; set; }
    public TxnParameterDM? TxnParameter { get; set; }
    public ApprovalDM? Approval { get; set; }
    public InvoiceHeaderDM Header { get; set; } = new();
    public InvoiceDiscountDM Discount { get; set; } = new();
    public List<InvoiceLineDM> Lines { get; set; } = new();
    public InvoiceChargesDM Charges { get; set; } = new();
    public InvoicePaymentsDM Payments { get; set; } = new();
    public InvoiceOthersDM Others { get; set; } = new();
}