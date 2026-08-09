namespace App.Automation.Core.DataModels.Shared
{
    public class ExpectedTotalsDM
    {
        /// <summary>Sum of all line amounts before any charges or tax.</summary>
        public decimal GrossValue { get; set; }

        /// <summary>Total discount amount applied across all lines.</summary>
        public decimal DiscountValue { get; set; }

        /// <summary>Total tax amount computed across all lines.</summary>
        public decimal TaxValue { get; set; }

        /// <summary>Total additional charges (freight, packing, etc.)</summary>
        public decimal TotalCharges { get; set; }

        /// <summary>Final payable amount: SubTotal - Discount + Tax + Charges.</summary>
        public decimal NetValue { get; set; }

        /// <summary>Amount already paid (from Payments section).</summary>
        public decimal AmountPaid { get; set; }

        /// <summary>Balance due: NetValue - AmountPaid.</summary>
        public decimal BalanceDue { get; set; }
    }
}
