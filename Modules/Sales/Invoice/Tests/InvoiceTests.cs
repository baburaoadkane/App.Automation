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

    #region VALIDATION SCENARIOS 

    [Test]
    [TestCaseSource(nameof(ValidationScenarios))]
    [Category(TestCategories.Smoke)]
    [Category(TestCategories.Validation)]
    public void Base_Invoice_Validation_Json_ValidateMessage(string jsonPath)
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

    #region CREATE SCENARIOS

    [Test]
    [TestCaseSource(nameof(CreateScenarios))]
    [Category(TestCategories.Create)]
    public void Base_Invoice_Create_Json_MultiLine_ValidateTotal(string jsonPath)
    {
        var data = InvoiceBuilder
            .FromJson(jsonPath)
            .AsScenario("Create")
            .Build();

        Report.Info($"Scenario: {data.TestDescription}");

        Executor.Execute(data);
    }

    #endregion    

    #region DIRECT APPROVAL SCENARIOS

    [Test]
    [Category(TestCategories.Direct_Approval)]
    [Category(TestCategories.Smoke)]
    public void Base_Invoice_Approval_Smoke_ValidateApproval()
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

    #region SUBMIT FOR APPROVAL SCENARIOS

    [Test]
    [Category(TestCategories.Submit)]
    [Category(TestCategories.Smoke)]
    public void Base_Invoice_Submit_Smoke_ValidateSubmitForApproval()
    {
        var data = InvoiceBuilder
            .New()
            .WithCustomer("C0002 | Minnah Elamin")
            .WithWarehouse("Grand Prime House")
            .WithReferenceNum("Smoke Test Submit For Approval")
            .AddLine(
                barcode: "",
                item: "I0001 | Screen Protectors"
            )
            .AsScenario("Submit")
            .Build();

        Executor.Execute(data);
    }

    #endregion

    #region APPROVAL SCENARIOS

    [Test]
    [TestCaseSource(nameof(ApprovalScenarios))]
    [Category(TestCategories.Approval)]
    public void Base_Invoice_Approval_Json_ValidateApproval(string jsonPath)
    {
        var data = InvoiceBuilder
            .FromJson(jsonPath)
            .AsScenario("Approval")
            .Build();

        Report.Info($"Scenario: {data.TestDescription}");

        Executor.Execute(data);
    }

    #endregion

    #region TEST CASE SOURCES
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
