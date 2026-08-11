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
    // SECTION ENGINE
    // =====================================================================

    private readonly List<SectionDefinition<InvoiceDM>> _sections;


    // =====================================================================
    // NETWORK
    // =====================================================================

    private readonly NetworkHelper _networkHelper;


    // =====================================================================
    // CONSTANTS
    // =====================================================================


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
        // Section configuration
        //
        // InvoiceSections is responsible for defining:
        //     Lines
        //     Discount
        //     Charges
        //     Payments
        //     Others
        // -----------------------------------------------------------------

        _sections = InvoiceSections.Create(
            _linesHandler,
            _discountHandler,
            _chargesHandler,
            _paymentsHandler,
            _othersHandler);
    }


    // =====================================================================
    // ENTRY POINT
    // =====================================================================

    public override void Execute(InvoiceDM document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

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
        // STEP 1: Navigate to Sales Invoice
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
        // STEP 2: Fill Header + Save
        //
        // Header is intentionally handled outside SectionEngine.
        // The invoice must be saved after the header before processing
        // Lines / Discount / Charges / Payments / Others.
        // -----------------------------------------------------------------

        ExecuteStep(
            "Fill Header",
            () =>
            {
                _headerHandler.Fill(document.Header);

                Save();

                ValidateAfterSave(document);
            });


        // -----------------------------------------------------------------
        // STEP 3: Execute Invoice Sections
        //
        // Sections are configured in InvoiceSections.cs.
        //
        // Current order:
        //     Lines
        //     Discount
        //     Charges
        //     Payments
        //     Others
        // -----------------------------------------------------------------

        var engine = new SectionEngine<InvoiceDM>(
            _sections,
            Save,
            Report);

        ExecuteStep(
            "Execute Sections",
            () =>
            {
                engine.Execute(document);
            });


        // -----------------------------------------------------------------
        // STEP 4: Start totals API capture
        // -----------------------------------------------------------------

        ExecuteStep(
            "Start totals API capture",
            () =>
            {
                _networkHelper.Clear();

                _networkHelper.StartCapture(
                    "/SalesInvoice/GetTxnSubtotals");
            });


        // -----------------------------------------------------------------
        // STEP 5: Open View Mode
        // -----------------------------------------------------------------

        ExecuteStep(
            "Open View Mode",
            () =>
            {
                ClickOnForm("View");
            });


        // -----------------------------------------------------------------
        // STEP 6: Validate totals and lines
        // -----------------------------------------------------------------

        ExecuteStep(
            "Validate After View",
            () =>
            {
                ValidateAfterView(document);
            });
    }


    // =====================================================================
    // APPROVAL
    // =====================================================================

    private void ExecuteApproval(InvoiceDM document)
    {
        // First create the invoice.
        ExecuteCreate(document);


        // -----------------------------------------------------------------
        // Approve
        // -----------------------------------------------------------------

        ExecuteStep(
            "Approve Document",
            () =>
            {
                ClickOnForm("Approve");

                Wait.WaitForSeconds(1);
            });


        // -----------------------------------------------------------------
        // Validate approval
        // -----------------------------------------------------------------

        ExecuteStep(
            "Validate After Approve",
            () =>
            {
                ValidateAfterApprove(document);
            });
    }


    // =====================================================================
    // VALIDATION
    // =====================================================================

    private void ExecuteValidation(InvoiceDM document)
    {
        // -----------------------------------------------------------------
        // STEP 1: Navigate to Sales Invoice
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
        // STEP 2: Fill invalid/incomplete data
        // -----------------------------------------------------------------

        ExecuteStep(
            "Fill Validation Data",
            () =>
            {
                _headerHandler.Fill(document.Header);
            });


        // -----------------------------------------------------------------
        // STEP 3: Attempt Save
        // -----------------------------------------------------------------

        ExecuteStep(
            "Attempt Save",
            () =>
            {
                ClickOnForm("Save");
            });


        // -----------------------------------------------------------------
        // STEP 4: Validate expected message
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
    // VALIDATION - AFTER SAVE
    // =====================================================================

    private void ValidateAfterSave(InvoiceDM document)
    {
        if (document.Expected == null)
        {
            Report.Warning(
                "No Expected values defined — skipping validation.");

            return;
        }


        // Validate save message

        _messageValidator.ValidateMessage(
            document.Expected.Messages?.OnSave,
            "dx-toast-message",
            "Save Message");


        // Validate generated document number

        _headerValidator.ValidateDocumentNumberGenerated();
    }


    // =====================================================================
    // VALIDATION - AFTER VIEW
    // =====================================================================

    private void ValidateAfterView(InvoiceDM document)
    {
        if (document.Expected == null)
        {
            Report.Warning(
                "No Expected values defined — skipping validation.");

            return;
        }


        // Validate line-level totals

        _linesValidator.ValidateLineTotals(
            document.Lines);


        // Get totals from API

        var totals =
            _networkHelper.GetResponse<TotalsResponseDM>();


        // Validate invoice totals

        _totalsValidator.ValidateTotalsFromApi(
            document.Expected,
            totals);
    }


    // =====================================================================
    // VALIDATION - AFTER APPROVE
    // =====================================================================

    private void ValidateAfterApprove(InvoiceDM document)
    {
        if (document.Expected == null)
        {
            Report.Warning(
                "No Expected values defined — skipping validation.");

            return;
        }


        // Validate approval message

        _messageValidator.ValidateMessage(
            document.Expected.Messages?.OnApprove,
            "dx-toast-message",
            "Approve Message");


        // Validate document status

        _headerValidator.ValidateDocumentStatus(
            document.Expected);


        // Validate payment status

        _headerValidator.ValidateDocumentPaymentStatus(
            document.Expected);
    }


    // =====================================================================
    // COMMON EXECUTION STEP
    // =====================================================================

    private void ExecuteStep(
        string stepName,
        Action action)
    {
        try
        {
            Report.Info($"Step: {stepName}");

            action();
        }
        catch (Exception ex)
        {
            Report.Fail(
                $"Failed at step: {stepName} | {ex.Message}");

            throw;
        }
    }


    // =====================================================================
    // SAVE
    // =====================================================================

    private void Save()
    {
        ClickOnForm("Save");
    }
}