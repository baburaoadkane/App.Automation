namespace App.Automation.Modules.Sales.Invoice.DataModels
{
    public class InvoicePaymentsDM
    {
        /// <summary>List of payment entries to apply.</summary>
        public List<PaymentEntryDM> Entries { get; set; } = new();
    }

    public class PaymentEntryDM
    {
        /// <summary>Payment mode. Example: "Cash", "Bank Transfer", "Cheque"</summary>
        public string? PaymentMode { get; set; }
        public string? Currency { get; set; }
        public string? CardNumber { get; set; }

        /// <summary>Payment amount to apply against this invoice.</summary>
        public decimal AmountFC { get; set; }
        public decimal AmountLC { get; set; }

        /// <summary>Payment reference number or transaction ID.</summary>
        public string? ReferenceNo { get; set; }

        /// <summary>Payment date. Format: dd-MM-yyyy</summary>
        public string? PaymentDate { get; set; }

        /// <summary>Bank account or cash account used for payment.</summary>
        public string? Account { get; set; }
        public string? Remarks { get; set; }
    }
}
