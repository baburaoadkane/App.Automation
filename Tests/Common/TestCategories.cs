namespace App.Automation.Tests.Common;

public static class TestCategories
{
    // Execution
    public const string Smoke = "Smoke";
    public const string Sanity = "Sanity";
    public const string Regression = "Regression";

    // Scenario
    public const string Create = "Create";
    public const string Edit = "Edit";
    public const string Delete = "Delete";
    public const string Direct_Approval = "Direct_Approval";
    public const string Submit = "Submit";
    public const string Approval = "Approval";
    public const string Validation = "Validation";
    public const string Negative = "Negative";
    public const string Positive = "Positive";

    // Modules
    public const string Sales = "Sales";
    public const string Purchase = "Purchase";
    public const string Inventory = "Inventory";
    public const string HRMS = "HRMS";

    // Transactions
    public const string Invoice = "Invoice";
    public const string Quotation = "Quotation";
    public const string Order = "Order";
    public const string DeliveryNote = "DeliveryNote";
    public const string Return = "Return";
}