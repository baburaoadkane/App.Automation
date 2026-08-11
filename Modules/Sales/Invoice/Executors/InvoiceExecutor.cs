using App.Automation.Core.Base;
using App.Automation.Core.Engine;
using App.Automation.Core.Utilities;
using App.Automation.Modules.Global.Sections;
using App.Automation.Modules.Global.Validators;
using App.Automation.Modules.Sales.Invoice.Configuration;
using App.Automation.Modules.Sales.Invoice.DataModels;
using App.Automation.Modules.Sales.Invoice.HeaderHandlers;
using App.Automation.Modules.Sales.Invoice.LineHandlers;
using App.Automation.Modules.Sales.Invoice.Validators;
using OpenQA.Selenium;

namespace App.Automation.Modules.Sales.Invoice.Executors;

public class InvoiceExecutor : BaseExecutor<InvoiceDM>
{
    // =====================================================================
    // HANDLERS
    // =====================================================================

    private readonly InvoiceHeaderHandler _headerHandler;
    private readonly InvoiceLineHandler _linesHandler;
    private readonly DiscountHandler _discountHandler;
    private readonly ChargesHandler _chargesHandler;
    private readonly PaymentsHandler _paymentsHandler;
    private readonly OthersHandler _othersHandler;
    private readonly ExpectationHandler _expectationHandler;

    // =====================================================================
    // VALIDATORS
    // =====================================================================

    private readonly HeaderValidator _headerValidator;
    private readonly LinesValidator _linesValidator;
    private readonly TotalsValidator _totalsValidator;
    private readonly MessageValidator _messageValidator;

    // =====================================================================
    // NETWORK
    // =====================================================================

    private readonly NetworkHelper _networkHelper;

    // =====================================================================
    // WORKFLOW ENGINE
    // =====================================================================
    private readonly WorkflowEngine _workflowEngine;

    // =====================================================================
    // CONSTRUCTOR
    // =====================================================================

    public InvoiceExecutor(
        IWebDriver driver,
        WaitHelper wait,
        ReportHelper report)
        : base(driver, wait, report)
    {
        // -----------------------------------------------------------------
        // Handlers
        // -----------------------------------------------------------------

        _headerHandler =
            new InvoiceHeaderHandler(driver, wait, report);

        _linesHandler =
            new InvoiceLineHandler(driver, wait, report);

        _discountHandler =
            new DiscountHandler(driver, wait, report);

        _chargesHandler =
            new ChargesHandler(driver, wait, report);

        _paymentsHandler =
            new PaymentsHandler(driver, wait, report);

        _othersHandler =
            new OthersHandler(driver, wait, report);

        _expectationHandler =
            new ExpectationHandler(driver, wait, report);

        // -----------------------------------------------------------------
        // Validators
        // -----------------------------------------------------------------

        _headerValidator =
            new HeaderValidator(
                driver,
                wait,
                report,
                _expectationHandler);

        _linesValidator =
            new LinesValidator(
                driver,
                wait,
                report,
                _expectationHandler);

        _totalsValidator =
            new TotalsValidator(
                driver,
                wait,
                report,
                _expectationHandler);

        _messageValidator =
            new MessageValidator(
                driver,
                wait,
                report,
                _expectationHandler);

        // -----------------------------------------------------------------
        // Network
        // -----------------------------------------------------------------

        _networkHelper =
            new NetworkHelper(driver);

        // -----------------------------------------------------------------
        // Workflow Engine
        // -----------------------------------------------------------------

        _workflowEngine = new WorkflowEngine(report);
    }

    // =====================================================================
    // ENTRY POINT
    // =====================================================================

    public override void Execute(InvoiceDM document)
    {
        ArgumentNullException.ThrowIfNull(document);

        Report.Info(
            $"── Sales Invoice Executor: {document.ScenarioType} ──");

        Report.Info(
            $"Test: {document.TestDescription}");

        switch (document.ScenarioType?.ToUpperInvariant())
        {
            case "CREATE":
                ExecuteCreate(document);
                break;

            case "APPROVAL":
                ExecuteApproval(document);
                break;

            case "VALIDATION":
                ExecuteValidation(document);
                break;

            default:
                throw new ArgumentException(
                    $"Unknown ScenarioType: {document.ScenarioType}");
        }
    }

    // =====================================================================
    // CREATE
    // =====================================================================

    private void ExecuteCreate(InvoiceDM document)
    {
        // -----------------------------------------------------------------
        // STEP 1 — Navigate to Invoice
        // -----------------------------------------------------------------

        ExecuteStep(
            "Navigate to Sales Invoice",
            () =>
            {
                NavigateToModule("Sales");
                NavigateToListing("Invoice");
                OpenFormMode("New");
                SwitchToOldInterface();
            });

        // -----------------------------------------------------------------
        // STEP 2 — Fill Header
        // -----------------------------------------------------------------

        ExecuteStep(
            "Fill Header",
            () =>
            {
                _headerHandler.Fill(document.Header);

                // Header must be saved before working with sections.
                Save();

                ValidateAfterSave(document);
            });

        // -----------------------------------------------------------------
        // STEP 3 — Create Section Definitions
        // -----------------------------------------------------------------

        var sections = InvoiceSections.Create(
            _linesHandler,
            _discountHandler,
            _chargesHandler,
            _paymentsHandler,
            _othersHandler);

        // -----------------------------------------------------------------
        // STEP 4 — Create Section Engine
        //
        // SectionEngine will:
        //
        //   1. Check ShouldRun
        //   2. Execute Action
        //   3. Save if RequiresSave = true
        //   4. Execute optional validation
        //
        // Your InvoiceSections currently use the default:
        //
        //     RequiresSave = true
        //
        // Therefore every executed section will be saved.
        // -----------------------------------------------------------------

        var sectionEngine =
            new SectionEngine<InvoiceDM>(
                sections,
                Save,
                Report);

        // -----------------------------------------------------------------
        // STEP 5 — Create Invoice Workflow
        //
        // Workflow:
        //
        //   Fill Sections
        //        ↓
        //      View
        //        ↓
        //    Validate
        //
        // -----------------------------------------------------------------

        var workflow =
            InvoiceWorkflow.Create(
                fillSections: () =>
                {
                    sectionEngine.Execute(document);
                },

                view: () =>
                {
                    StartTotalsCapture();

                    ClickOnForm("View");
                },

                validate: () =>
                {
                    ValidateAfterView(document);
                });

        // -----------------------------------------------------------------
        // STEP 6 — Execute Workflow
        // -----------------------------------------------------------------

        //ExecuteWorkflow(workflow);
        _workflowEngine.Execute(workflow);
    }

    // =====================================================================
    // APPROVAL
    // =====================================================================

    private void ExecuteApproval(InvoiceDM document)
    {
        // First execute the complete create workflow.
        ExecuteCreate(document);

        // Then approve the document.
        ExecuteStep(
            "Approve Document",
            () =>
            {
                ClickOnForm("Approve");

                Wait.WaitForSeconds(1);
            });

        // Validate approval result.
        ExecuteStep(
            "Validate After Approve",
            () =>
            {
                ValidateAfterApprove(document);
            });
    }

    // =====================================================================
    // VALIDATION SCENARIO
    // =====================================================================

    private void ExecuteValidation(InvoiceDM document)
    {
        // -----------------------------------------------------------------
        // STEP 1 — Navigate
        // -----------------------------------------------------------------

        ExecuteStep(
            "Navigate to Sales Invoice",
            () =>
            {
                NavigateToModule("Sales");
                NavigateToListing("Invoice");
                OpenFormMode("New");
                SwitchToOldInterface();
            });

        // -----------------------------------------------------------------
        // STEP 2 — Fill incomplete / invalid header
        // -----------------------------------------------------------------

        ExecuteStep(
            "Fill Invalid Invoice Data",
            () =>
            {
                _headerHandler.Fill(document.Header);
            });

        // -----------------------------------------------------------------
        // STEP 3 — Save and expect validation
        // -----------------------------------------------------------------

        ExecuteStep(
            "Save Invoice and Expect Validation",
            () =>
            {
                ClickOnForm("Save");
            });

        // -----------------------------------------------------------------
        // STEP 4 — Validate message
        // -----------------------------------------------------------------

        ExecuteStep(
            "Validate Validation Message",
            () =>
            {
                _messageValidator.ValidateValidationMessage(
                    document.Expected);
            });
    }

    // =====================================================================
    // WORKFLOW EXECUTION
    // =====================================================================

    //private void ExecuteWorkflow(WorkflowDefinition workflow)
    //{
    //    ArgumentNullException.ThrowIfNull(workflow);

    //    Report.Info(
    //        $"Starting Workflow: {workflow.Name}");

    //    foreach (var step in workflow.Steps)
    //    {
    //        ExecuteStep(
    //            step.Name,
    //            step.Action);
    //    }

    //    Report.Info(
    //        $"Completed Workflow: {workflow.Name}");
    //}

    // =====================================================================
    // START TOTALS API CAPTURE
    // =====================================================================

    private void StartTotalsCapture()
    {
        ExecuteStep(
            "Start totals API capture",
            () =>
            {
                _networkHelper.Clear();

                _networkHelper.StartCapture(
                    "/SalesInvoice/GetTxnSubtotals");
            });
    }

    // =====================================================================
    // VALIDATION — AFTER SAVE
    // =====================================================================

    private void ValidateAfterSave(InvoiceDM document)
    {
        if (document.Expected == null)
        {
            Report.Warning(
                "No Expected values defined — skipping validation.");

            return;
        }

        _messageValidator.ValidateMessage(
            document.Expected.Messages?.OnSave,
            "dx-toast-message",
            "Save Message");

        _headerValidator.ValidateDocumentNumberGenerated();
    }

    // =====================================================================
    // VALIDATION — AFTER VIEW
    // =====================================================================

    private void ValidateAfterView(InvoiceDM document)
    {
        if (document.Expected == null)
        {
            Report.Warning(
                "No Expected values defined — skipping validation.");

            return;
        }

        // -----------------------------------------------------------------
        // Validate line-level totals
        // -----------------------------------------------------------------

        _linesValidator.ValidateLineTotals(
            document.Lines);

        // -----------------------------------------------------------------
        // Get totals from API
        // -----------------------------------------------------------------

        var totals =
            _networkHelper.GetResponse<TotalsResponseDM>();

        // -----------------------------------------------------------------
        // Validate invoice totals
        // -----------------------------------------------------------------

        _totalsValidator.ValidateTotalsFromApi(
            document.Expected,
            totals);
    }

    // =====================================================================
    // VALIDATION — AFTER APPROVE
    // =====================================================================

    private void ValidateAfterApprove(InvoiceDM document)
    {
        if (document.Expected == null)
        {
            Report.Warning(
                "No Expected values defined — skipping validation.");

            return;
        }

        _messageValidator.ValidateMessage(
            document.Expected.Messages?.OnApprove,
            "dx-toast-message",
            "Approve Message");

        _headerValidator.ValidateDocumentStatus(
            document.Expected);

        _headerValidator.ValidateDocumentPaymentStatus(
            document.Expected);
    }

    // =====================================================================
    // SAVE
    // =====================================================================

    private void Save()
    {
        ExecuteStep(
            "Save",
            () =>
            {
                ClickOnForm("Save");
            });
    }

    // =====================================================================
    // COMMON WORKFLOW STEP WRAPPER
    // =====================================================================

    private void ExecuteStep(
        string stepName,
        Action action)
    {
        try
        {          
            action();
        }
        catch (Exception ex)
        {
            Report.Fail(
                $"Failed at step: {stepName} | {ex.Message}");

            throw;
        }
    }
}