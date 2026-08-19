using App.Automation.Core.Enums;
using App.Automation.Modules.Sales.Invoice.Builders;
using App.Automation.Modules.Sales.Invoice.DataModels;
using App.Automation.Tests.Common;

namespace App.Automation.Modules.Sales.Invoice.Tests;

[TestFixture]
[Category(TestCategories.Sales)]
[Category(TestCategories.Invoice)]
public class InvoiceTests : BaseTransactionTest<InvoiceDM>
{
    protected override ModuleType Module =>
    ModuleType.Sales;

    protected override TransactionType Transaction =>
    TransactionType.Invoice;

    // ── JSON folder paths ──────────────────────────────────────────────────
    private const string FolderPath = TestDataFolders.Sales.Invoice;    

    #region Validation Scenarios

    //[Test]
    //[Category(TestCategories.Validation)]
    //[Category(TestCategories.Smoke)]
    public void Invoice_Validate_Smoke_CustomerRequired()
    {
        var data = InvoiceBuilder
            .New()
            .AsScenario("Validation")
            .Build();

        data.Expected = new Core.DataModels.Shared.ExpectedResultDM
        {
            ValidationMessage = "Currency is required."
        };

        Executor.Execute(data);
    }

    //[Test]
    //[Category(TestCategories.Validation)]
    //[Category(TestCategories.Smoke)]
    public void Invoice_Validate_Smoke_WarehouseRequired()
    {
        var data = InvoiceBuilder
            .New()
            .WithCustomer("C0002 | Minnah Elamin")
            .AsScenario("Validation")
            .Build();

        data.Expected = new Core.DataModels.Shared.ExpectedResultDM
        {
            ValidationMessage = "Warehouse is required."
        };

        Executor.Execute(data);
    }    

    //[Test]
    //[TestCaseSource(nameof(ValidationScenarios))]
    //[Category(TestCategories.Validation)]
    public void Base_Invoice_Validate_Json_ValidateMessage(string jsonPath)
    {
        var data = InvoiceBuilder
            .FromJson(jsonPath)
            .AsScenario("Validation")
            .Build();

        Report.Info($"Scenario: {data.TestDescription}");
        Report.Info($"Expected Error: {data.Expected?.ValidationMessage}");

        Executor.Execute(data);
    }

    #endregion    

    #region Create Scenarios

    //[Test]
    [Category(TestCategories.Create)]
    [Category(TestCategories.Smoke)]
    public void Invoice_Create_Smoke_SingleLine_Successful()
    {
        var data = InvoiceBuilder
            .New()
            .WithCustomer("C0002 | Minnah Elamin")
            .WithWarehouse("Grand Prime House")
            .WithReferenceNum("Smoke Test")
            .AddLine(
                barcode: "",
                item: "I0001 | Screen Protectors"
            )
            .AsScenario("Create")
            .Build();

        Executor.Execute(data);
    }

    //[Test]
    //[TestCaseSource(nameof(CreateScenarios))]
    [Category(TestCategories.Create)]
    public void Base_Invoice_Create_Json_MultiLine(string jsonPath)
    {
        var data = InvoiceBuilder.FromJson(jsonPath).Build();

        Report.Info($"Scenario: {data.TestDescription}");

        Executor.Execute(data);
    }

    #endregion    

    #region Direct Approval Scenarios

    //[Test]
    //[Category(TestCategories.Direct_Approval)]
    [Category(TestCategories.Smoke)]
    public void Invoice_Approve_Smoke_DirectApproval()
    {
        var data = InvoiceBuilder
            .New()
            .WithCustomer("C0002 | Minnah Elamin")
            .WithWarehouse("Grand Prime House")
            .WithReferenceNum("Direct Approval Smoke Test")
            .AddLine(
                barcode: "",
                item: "I0001 | Screen Protectors"
            )
            .AsScenario("Direct_Approval")
            .Build();

        Executor.Execute(data);
    }

    #endregion

    #region Submit For Approval Scenarios

    //[Test]
    //[Category(TestCategories.Submit)]
    //[Category(TestCategories.Smoke)]
    public void Invoice_CreateAndSubmit_SingleLine()
    {
        var data = InvoiceBuilder
            .New()
            .WithCustomer("C0002 | Minnah Elamin")
            .WithWarehouse("Grand Prime House")
            .WithReferenceNum("Smoke Test With Approval")
            .AddLine(
                barcode: "",
                item: "I0001 | Screen Protectors"
            )
            .AsScenario("Submit")
            .Build();

        Executor.Execute(data);
    }

    #endregion

    #region Approval - Scenario Driven

    //[Test]
    //[Category(TestCategories.Approval)]
    //[Category(TestCategories.Smoke)]
    public void Invoice_Approve_Smoke_SingleLine_Approval()
    {
        var data = InvoiceBuilder
            .New()
            .WithCustomer("C0002 | Minnah Elamin")
            .WithWarehouse("Grand Prime House")
            .WithReferenceNum("Smoke Test With Approval")
            .AddLine(
                barcode: "",
                item: "I0001 | Screen Protectors"
            )
            .AsScenario("Approval")
            .Build();

        Executor.Execute(data);
    }

    [Test]
    [TestCaseSource(nameof(ApprovalScenarios))]
    [Category(TestCategories.Approval)]
    public void Base_Invoice_Approve_Json_Approval(string jsonPath)
    {
        var data = InvoiceBuilder
            .FromJson(jsonPath)
            .AsScenario("Approval")
            .Build();

        Report.Info($"Scenario: {data.TestDescription}");

        Executor.Execute(data);
    }

    #endregion

    #region Test Case Sources
    private static IEnumerable<TestCaseData> CreateScenarios()
        => ScenarioFactory.FromFolder(
            TestDataFolders.Create(FolderPath));

    private static IEnumerable<TestCaseData> ApprovalScenarios()
        => ScenarioFactory.FromFolder(
            TestDataFolders.Approval(FolderPath));

    private static IEnumerable<TestCaseData> ValidationScenarios()
        => ScenarioFactory.FromFolder(
            TestDataFolders.Validation(FolderPath));
    #endregion
}
