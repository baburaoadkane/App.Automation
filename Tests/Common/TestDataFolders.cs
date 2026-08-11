namespace App.Automation.Tests.Common
{
    public static class TestDataFolders
    {
        public static class Sales
        {
            public const string Quotation = "Modules/Sales/Quotation/TestData";
            public const string Order = "Modules/Sales/Order/TestData";
            public const string DeliveryNote = "Modules/Sales/DeliveryNote/TestData";
            public const string Invoice = "Modules/Sales/Invoice/TestData";
            public const string Return = "Modules/Sales/Return/TestData";
        }

        public static class Purchase
        {
            public const string Order = "Modules/Purchase/Order/TestData";
            public const string Invoice = "Modules/Purchase/Invoice/TestData";
            public const string Return = "Modules/Purchase/Return/TestData";
        }

        public static class Inventory
        {
            public const string Adjustment = "Modules/Inventory/Adjustment/TestData";
        }

        public static string Create(string root) => $"{root}/Create";
        public static string Approval(string root) => $"{root}/Approve";
        public static string Validation(string root) => $"{root}/Validation";
        public static string Edit(string root) => $"{root}/Edit";
        public static string Negative(string root) => $"{root}/Negative";
    }
}
