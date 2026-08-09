namespace App.Automation.Core.DataModels.Shared
{
    public class TxnParameterDM
    {
        public bool ShowBarcodeOnLines { get; set; }
        public bool AllowModifyItemDescription { get; set; }
        public bool ShowWarehouseOnLines { get; set; }
        public bool AllowItemUOMModifyInTxn { get; set; }
        public bool ShowDiscountOnLines { get; set; }
        public bool AllowBonusQty { get; set; }
        public bool AllowNonInventoryItems { get; set; }
        public bool AllowPartialPayment { get; set; }
        public bool RequirePaymentMethod { get; set; }
        public bool UseMultiplePaymentMethod { get; set; }
        public bool PromptOnStockBelowMinimumLevel { get; set; }
        public bool ShowAdditionalItemInfo { get; set; }
        public bool ShowLastPurchasePrice { get; set; }
        public bool NegativeStockCheck { get; set; }
        public bool ReserveQty { get; set; }
        public bool EnableDentedStock { get; set; }
        public bool AvoidCostingOnApproval { get; set; }
        public bool InvoiceAsPerPickingOrder { get; set; }
        public bool StoreTransferAsPerPickingOrder { get; set; }
        public bool EnableApprovalWorkflow { get; set; }
        public bool SendEmailOnApproval { get; set; }
        public bool SendPaymentLinkViaEmail { get; set; }
    }
}
