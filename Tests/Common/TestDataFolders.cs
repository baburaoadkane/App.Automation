namespace App.Automation.Tests.Common
{
    public static class TestDataFolders
    {
        public static class Sales
        {
            public const string Quotation = "Modules/Sales/Data/Quotation";
            public const string Order = "Modules/Sales/Data/Order";
            public const string DeliveryNote = "Modules/Sales/Data/DeliveryNote";
            public const string Invoice = "Modules/Sales/Data/Invoice";
            public const string Return = "Modules/Sales/Data/Return";
        }

        public static class Purchase
        {
            public const string Order = "Modules/Purchase/Data/Order";
            public const string Invoice = "Modules/Purchase/Data/Invoice";
            public const string Return = "Modules/Purchase/Data/Return";
        }

        public static class Inventory
        {
            public const string Adjustment = "Modules/Inventory/Data/Adjustment";
        }

        public static string Create(string root) => $"{root}/Create";
        public static string Approval(string root) => $"{root}/Approve";
        public static string Validation(string root) => $"{root}/Validation";
        public static string Edit(string root) => $"{root}/Edit";
        public static string Negative(string root) => $"{root}/Negative";
    }
}
