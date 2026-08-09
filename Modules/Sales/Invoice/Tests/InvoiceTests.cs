using App.Automation.Core.Base;
using App.Automation.Core.Utilities;
using App.Automation.Modules.Sales.Invoice.Builders;
using App.Automation.Modules.Sales.Invoice.Executors;
using App.Automation.Tests.Common;

namespace App.Automation.Modules.Sales.Invoice.Tests;

[TestFixture]
[Category("Sales")]
[Category("SalesInvoice")]
public class InvoiceTests : BaseTest
{
    // ── Executor — initialized per test in SetUp ───────────────────────────
    private InvoiceExecutor _executor = null!;

    // ── JSON folder paths ──────────────────────────────────────────────────
    private const string Module = TestDataFolders.Sales.Invoice;

    // ── SetUp ─────────────────────────────────────────────────────

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _executor = new InvoiceExecutor(Driver, Wait, Report);
    }

    // ══════════════════════════════════════════════════════════════════════
    // VALIDATION — programmatic, no JSON file needed
    // ══════════════════════════════════════════════════════════════════════

    [Test]
    [Category("Validation")]
    [Category("Smoke")]
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

        _executor.Execute(data);
    }

    [Test]
    [Category("Validation")]
    [Category("Smoke")]
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

        _executor.Execute(data);
    }

    // ══════════════════════════════════════════════════════════════════════
    // VALIDATION - JSON-DRIVEN SCENARIOS
    // ══════════════════════════════════════════════════════════════════════

    [Test]
    [TestCaseSource(nameof(ValidationScenarios))]
    [Category("Validation")]
    public void Base_Invoice_Validation_ValidationMessage(string jsonPath)
    {
        var data = InvoiceBuilder
            .FromJson(jsonPath)
            .AsScenario("Validation")
            .Build();

        Report.Info($"Scenario: {data.TestDescription}");
        Report.Info($"Expected Error: {data.Expected?.ValidationMessage}");

        _executor.Execute(data);
    }

    // ══════════════════════════════════════════════════════════════════════
    // CREATE - programmatic, no JSON file needed
    // ══════════════════════════════════════════════════════════════════════

    [Test]
    [Category("Create")]
    [Category("Smoke")]
    public void Invoice_Create_SingleLine()
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

        _executor.Execute(data);
    }

    // ══════════════════════════════════════════════════════════════════════
    // CREATE AND APPROVE - programmatic, no JSON file needed
    // ══════════════════════════════════════════════════════════════════════

    [Test, Order(5)]
    [Category("Approval")]
    [Category("Smoke")]
    public void Invoice_Approval_SingleLine()
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
            .WithApproval()
            .Build();

        _executor.Execute(data);
    }

    // ══════════════════════════════════════════════════════════════════════
    // CREATE - JSON-DRIVEN SCENARIOS
    // ══════════════════════════════════════════════════════════════════════

    [Test, Order(6)]
    [TestCaseSource(nameof(CreateScenarios))]
    [Category("Create")]
    public void Base_Invoice_Create_Multiline_ValidateTotal(string jsonPath)
    {
        var data = InvoiceBuilder.FromJson(jsonPath).Build();

        Report.Info($"Scenario: {data.TestDescription}");

        _executor.Execute(data);
    }

    // ══════════════════════════════════════════════════════════════════════
    // APPROVAL - JSON-DRIVEN SCENARIOS
    // ══════════════════════════════════════════════════════════════════════

    [Test, Order(7)]
    [TestCaseSource(nameof(ApprovalScenarios))]
    [Category("Approval")]
    public void Base_Invoice_Approval_MultiLine_ValidateTotal(string jsonPath)
    {
        var data = InvoiceBuilder
            .FromJson(jsonPath)
            .WithApproval()
            .Build();

        Report.Info($"Scenario: {data.TestDescription}");

        _executor.Execute(data);
    }


    // ══════════════════════════════════════════════════════════════════════
    // EDIT SCENARIOS
    // ══════════════════════════════════════════════════════════════════════

    //[Test]
    //[TestCaseSource(nameof(EditScenarios))]
    //[Category("Edit")]
    //public void Invoice_Edit_Update_Json(string jsonPath)
    //{
    //    var data = SalesInvoiceBuilder
    //        .FromJson(jsonPath)
    //        .AsScenario("Edit")
    //        .Build();

    //    Report.Info($"Scenario: {data.TestDescription}");
    //    Report.Info($"Document: {data.DocumentNo}");

    //    _executor.Execute(data);
    //}

    // ══════════════════════════════════════════════════════════════════════
    // VALIDATION SCENARIOS
    // ══════════════════════════════════════════════════════════════════════

    //[Test]
    //[TestCaseSource(nameof(ValidationScenarios))]
    //[Category("Validation")]
    //public void Invoice_Validation_ExpectedValues_Json(string jsonPath)
    //{
    //    var data = SalesInvoiceBuilder
    //        .FromJson(jsonPath)
    //        .AsScenario("Validation")
    //        .Build();

    //    Report.Info($"Scenario: {data.TestDescription}");
    //    Report.Info($"Document: {data.DocumentNo}");

    //    _executor.Execute(data);
    //}

    // ══════════════════════════════════════════════════════════════════════
    // TEST CASE SOURCES
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Returns all JSON file paths from the Create folder.</summary>
    private static IEnumerable<TestCaseData> CreateScenarios()
    => BuildTestCases(TestDataFolders.Create(Module));

    /// <summary>Returns all JSON file paths from the Approval folder.</summary>
    private static IEnumerable<TestCaseData> ApprovalScenarios()
        => BuildTestCases(TestDataFolders.Approval(Module));

    /// <summary>Returns all JSON file paths from the Negative folder.</summary>
    private static IEnumerable<TestCaseData> NegativeScenarios()
        => BuildTestCases(TestDataFolders.Negative(Module));

    /// <summary>Returns all JSON file paths from the Edit folder.</summary>
    private static IEnumerable<TestCaseData> EditScenarios()
        => BuildTestCases(TestDataFolders.Edit(Module));

    /// <summary>Returns all JSON file paths from the Validation folder.</summary>
    private static IEnumerable<TestCaseData> ValidationScenarios()
        => BuildTestCases(TestDataFolders.Validation(Module));

    /// <summary>
    /// Discovers all JSON files in the given folder relative to the
    /// test output directory (AppContext.BaseDirectory).
    ///
    /// Called at DISCOVERY TIME by NUnit — must never throw,
    /// must never use TestContext (it is null during discovery).
    /// Returns empty silently if the folder has no files yet.
    /// </summary>
    /// 
    private static IEnumerable<TestCaseData> BuildTestCases(string folderPath)
    {
        IEnumerable<string> files;

        try
        {
            files = JsonLoader.GetAllFiles(folderPath);
        }
        catch
        {
            // Folder doesn't exist yet — return empty so suite doesn't fail
            yield break;
        }

        foreach (string filePath in files)
        {
            string testName = Path.GetFileNameWithoutExtension(filePath);

            yield return new TestCaseData(filePath)
                .SetName(testName)         // Shows filename as test name in report
                .SetDescription(testName); // Shows in NUnit test explorer
        }
    }
}
