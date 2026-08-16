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

    // ══════════════════════════════════════════════════════════════════════
    // VALIDATION — programmatic, no JSON file needed
    // ══════════════════════════════════════════════════════════════════════

    // Customer is required Validation 
    [Test]
    [Category(TestCategories.Validation)]
    [Category(TestCategories.Smoke)]
    public void Invoice_Validation_MissingCustomer_Smoke()
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

    // Warehouse is required Validation
    [Test]
    [Category(TestCategories.Validation)]
    [Category(TestCategories.Smoke)]
    public void Invoice_Validation_MissingWarehouse_Smoke()
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

    // ══════════════════════════════════════════════════════════════════════
    // VALIDATION - JSON-DRIVEN SCENARIOS [As Many Json Files]
    // ══════════════════════════════════════════════════════════════════════    

    [Test]
    [TestCaseSource(nameof(ValidationScenarios))]
    [Category(TestCategories.Validation)]
    public void Base_Invoice_Validation_ValidationMessage(string jsonPath)
    {
        var data = InvoiceBuilder
            .FromJson(jsonPath)
            .AsScenario("Validation")
            .Build();

        Report.Info($"Scenario: {data.TestDescription}");
        Report.Info($"Expected Error: {data.Expected?.ValidationMessage}");

        Executor.Execute(data);
    }

    // ══════════════════════════════════════════════════════════════════════
    // CREATE - programmatic, no JSON file needed
    // ══════════════════════════════════════════════════════════════════════

    [Test]
    [Category(TestCategories.Create)]
    [Category(TestCategories.Smoke)]
    public void Invoice_Create_Save_View_SingleLine()
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

    // ══════════════════════════════════════════════════════════════════════
    // DIRECT APPROVAL - programmatic, no JSON file needed
    // ══════════════════════════════════════════════════════════════════════

    [Test]
    [Category(TestCategories.Direct_Approval)]
    [Category(TestCategories.Smoke)]
    public void Invoice_DirectApproval_SingleLine()
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

    // ══════════════════════════════════════════════════════════════════════
    // CREATE - JSON-DRIVEN SCENARIOS [As Many Json Files]
    // ══════════════════════════════════════════════════════════════════════

    [Test]
    [TestCaseSource(nameof(CreateScenarios))]
    [Category(TestCategories.Create)]
    public void Base_Invoice_Create_Multiline_ValidateTotal(string jsonPath)
    {
        var data = InvoiceBuilder.FromJson(jsonPath).Build();

        Report.Info($"Scenario: {data.TestDescription}");

        Executor.Execute(data);
    }

    // ══════════════════════════════════════════════════════════════════════
    // CREATE AND APPROVE - programmatic, no JSON file needed
    // ══════════════════════════════════════════════════════════════════════

    [Test]
    [Category(TestCategories.Submit)]
    [Category(TestCategories.Smoke)]
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


    // ══════════════════════════════════════════════════════════════════════
    // CREATE AND APPROVE - programmatic, no JSON file needed
    // ══════════════════════════════════════════════════════════════════════

    [Test]
    [Category(TestCategories.Approval)]
    [Category(TestCategories.Smoke)]
    public void Invoice_SubmitAndApprove_SingleLine()
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

    // ══════════════════════════════════════════════════════════════════════
    // APPROVAL - JSON-DRIVEN SCENARIOS
    // ════════════════════════════════════════════

    [Test]
    [TestCaseSource(nameof(ApprovalScenarios))]
    [Category(TestCategories.Approval)]
    public void Base_Invoice_Approval(string jsonPath)
    {
        var data = InvoiceBuilder
            .FromJson(jsonPath)
            .AsScenario("Approval")
            .Build();

        Report.Info($"Scenario: {data.TestDescription}");

        Executor.Execute(data);
    }


    // ══════════════════════════════════════════════════════════════════════
    // EDIT SCENARIOS
    // ════════════════════════════════════
    //public void Invoice_Edit_Update_Json(string jsonPath)
    //{
    //    var data = SalesInvoiceBuilder
    //        .FromJson(jsonPath)
    //        .AsScenario("Edit")
    //        .Build();

    //    Report.Info($"Scenario: {data.TestDescription}");
    //    Report.Info($"Document: {data.DocumentNo}");

    //    Executor.Execute(data);
    //}

    // ══════════════════════════════════════════════════════════════════════
    // VALIDATION SCENARIOS
    // ══════════════════════════════════

    //[Test]
    //[TestCaseSource(nameof(ValidationScenarios))]
    //[Category(TestCategories.Validation)]
    //public void Invoice_Validation_ExpectedValues_Json(string jsonPath)
    //{
    //    var data = SalesInvoiceBuilder
    //        .FromJson(jsonPath)
    //        .AsScenario("Validation")
    //        .Build();

    //    Report.Info($"Scenario: {data.TestDescription}");
    //    Report.Info($"Document: {data.DocumentNo}");

    //    Executor.Execute(data);
    //}


    // ══════════════════════════════════════════════════════════════════════
    // TEST CASE SOURCES
    // ══════════════════════════════════════════════════════════════════════

    private static IEnumerable<TestCaseData> CreateScenarios()
        => ScenarioFactory.FromFolder(
            TestDataFolders.Create(FolderPath));

    private static IEnumerable<TestCaseData> ApprovalScenarios()
        => ScenarioFactory.FromFolder(
            TestDataFolders.Approval(FolderPath));

    private static IEnumerable<TestCaseData> ValidationScenarios()
        => ScenarioFactory.FromFolder(
            TestDataFolders.Validation(FolderPath));

    private static IEnumerable<TestCaseData> EditScenarios()
        => ScenarioFactory.FromFolder(
            TestDataFolders.Edit(FolderPath));

    private static IEnumerable<TestCaseData> NegativeScenarios()
        => ScenarioFactory.FromFolder(
            TestDataFolders.Negative(FolderPath));
}
