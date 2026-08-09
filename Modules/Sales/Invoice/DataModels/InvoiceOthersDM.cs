namespace App.Automation.Modules.Sales.Invoice.DataModels
{
    public class InvoiceOthersDM
    {
        /// <summary>Customer-facing remarks printed on the invoice.</summary>
        public string? Remarks { get; set; }
        public string? Description { get; set; }
        public string? ChequeNum { get; set; }
        public string? PaymentTerm { get; set; }
        public string? BillingAddress { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonMobile { get; set; }
        public string? ContactPersonEmail { get; set; }

        /// <summary>Internal notes visible only to staff.</summary>
        public string? InternalNotes { get; set; }

        /// <summary>Terms and conditions text for this invoice.</summary>
        public string? TermsAndConditions { get; set; }

        public bool HasData()
        {
            return

                !string.IsNullOrWhiteSpace(Remarks) ||
                !string.IsNullOrWhiteSpace(Description) ||
                !string.IsNullOrWhiteSpace(ChequeNum) ||
                !string.IsNullOrWhiteSpace(PaymentTerm) ||
                !string.IsNullOrWhiteSpace(BillingAddress) ||
                !string.IsNullOrWhiteSpace(ShippingAddress) ||
                !string.IsNullOrWhiteSpace(ContactPersonName) ||
                !string.IsNullOrWhiteSpace(ContactPersonMobile) ||
                !string.IsNullOrWhiteSpace(ContactPersonEmail);
        }
    }
}
