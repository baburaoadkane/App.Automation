namespace App.Automation.Modules.Sales.Invoice.DataModels
{
    public class TotalsResponseDM
    {
        public decimal GrossValue { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal DiscountValueLC { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal NetValue { get; set; }
        public decimal NetValueLC { get; set; }
        public decimal TaxValue { get; set; }
        public decimal TaxValueLC { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
