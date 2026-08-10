using App.Automation.Core.Base;
using App.Automation.Core.Engine;
using App.Automation.Core.Utilities;
using App.Automation.Modules.Global.Sections;
using App.Automation.Modules.Global.Validators;
using App.Automation.Modules.Sales.Invoice.DataModels;
using App.Automation.Modules.Sales.Invoice.HeaderHandlers;
using App.Automation.Modules.Sales.Invoice.LineHandlers;
using App.Automation.Modules.Sales.Invoice.Validators;
using OpenQA.Selenium;

namespace App.Automation.Modules.Sales.Invoice.Executors;

public class InvoiceExecutor : BaseExecutor<InvoiceDM>
{
    // ── Handlers ───────────────────────────────────────────────────────────
    private readonly InvoiceHeaderHandler _headerHandler;
    private readonly InvoiceLineHandler _linesHandler;
    private readonly DiscountHandler _discountHandler;
    private readonly ChargesHandler _chargesHandler;
    private readonly PaymentsHandler _paymentsHandler;
    private readonly OthersHandler _othersHandler;
    private readonly ExpectationHandler _expectationHandler;

    // ── Validators ─────────────────────────────────────────────────────────
    private readonly HeaderValidator _headerValidator;
    private readonly LinesValidator _linesValidator;
    private readonly TotalsValidator _totalsValidator;
    private readonly MessageValidator _messageValidator;

    // ── Network ────────────────────────────────────────────────────────────
    private readonly NetworkHelper _networkHelper;

    private const string EditInvoiceRoute = "sales/invoice/edit/{0}";

    public InvoiceExecutor(IWebDriver driver, WaitHelper wait, ReportHelper report)
        : base(driver, wait, report)
    {
        _headerHandler = new InvoiceHeaderHandler(driver, wait, report);
        _linesHandler = new InvoiceLineHandler(driver, wait, report);
        _discountHandler = new DiscountHandler(driver, wait, report);
        _chargesHandler = new ChargesHandler(driver, wait, report);
        _paymentsHandler = new PaymentsHandler(driver, wait, report);
        _othersHandler = new OthersHandler(driver, wait, report);
        _expectationHandler = new ExpectationHandler(driver, wait, report);

        _headerValidator = new HeaderValidator(driver, wait, report, _expectationHandler);
        _linesValidator = new LinesValidator(driver, wait, report, _expectationHandler);
        _totalsValidator = new TotalsValidator(driver, wait, report, _expectationHandler);
        _messageValidator = new MessageValidator(driver, wait, report, _expectationHandler);

        _networkHelper = new NetworkHelper(driver);
    }

    // ── Entry ──────────────────────────────────────────────────────────────

    public override void Execute(InvoiceDM document)
    {
        Report.Info($"── Sales Invoice Executor: {document.ScenarioType} ──");
        Report.Info($"Test: {document.TestDescription}");

        switch (document.ScenarioType?.ToUpperInvariant())
        {
            case "CREATE":
                ExecuteCreate(document);
                break;

            case "APPROVAL":
                ExecuteApproval(document);
                break;

            //case "NEGATIVE":
            //    ExecuteNegative(document);
            //    break;

            case "VALIDATION":
                ExecuteValidation(document);
                break;

            default:
                throw new ArgumentException($"Unknown ScenarioType: {document.ScenarioType}");
        }
    }

    // ── CREATE ──────────────────────────────────────────────────────────────
    private void ExecuteCreate(InvoiceDM document)
    {
        ExecuteStep("Navigate to Sales Invoice", () =>
        {
            NavigateToModule("Sales");
            NavigateToListing("Invoice");
            OpenFormMode("New");
            SwitchToOldInterface();
        });

        ExecuteStep("Fill Header", () =>
        {
            _headerHandler.Fill(document.Header);
            Save();
            ValidateAfterSave(document);
        });

        // ── Execute Sections ──────────────────────────────────────────────
        var sections = new List<SectionDefinition<InvoiceDM>>
        {
            new()
            {
                Name = "Lines",
                ShouldRun = d => d.Lines?.Any() == true,
                Action = d => _linesHandler.Fill(d.Lines)
            },
            new()
            {
                 Name = "Discount",
                 ShouldRun = d => d.Discount?.HasData() == true,
                 Action = d => _discountHandler.Fill(d.Discount)
            },
            new()
            {
                Name = "Charges",
                ShouldRun = d => d.AppPreference?.IsChargesEnabled == true &&
                d.Charges?.Items?.Any() == true,
                Action = d => _chargesHandler.Fill(d.Charges)
            },
            new()
            {
                Name = "Payments",
                ShouldRun = d => d.TxnParameter?.UseMultiplePaymentMethod == true &&
                d.Payments?.Entries?.Any() == true,
                Action = d => _paymentsHandler.Fill(d.Payments)
            },
            new()
            {
                Name = "Others",
                ShouldRun = d => d.Others?.HasData() == true,
                Action = d => _othersHandler.Fill(d.Others)
            }
        };

        var engine = new SectionEngine<InvoiceDM>(
            sections,
            Save,
            Report
        );

        ExecuteStep("Execute Sections", () =>
        {
            engine.Execute(document);
        });

        ExecuteStep("Start totals API capture", () =>
        {
            _networkHelper.Clear();
            _networkHelper.StartCapture("/SalesInvoice/GetTxnSubtotals");
        });

        ExecuteStep("Open View Mode", () =>
        {
            ClickOnForm("View");
        });

        ExecuteStep("Validate After View", () =>
        {
            ValidateAfterView(document);
        });
    }

    // ── APPROVAL ───────────────────────────────────────────────────────────

    private void ExecuteApproval(InvoiceDM document)
    {
        ExecuteCreate(document);

        ExecuteStep("Approve Document", () =>
        {
            ClickOnForm("Approve");
            Wait.WaitForSeconds(1);
        });

        ExecuteStep("Validate After Approve", () =>
        {
            ValidateAfterApprove(document);
        });
    }

    // ── VALIDATION ─────────────────────────────────────────────────────────

    private void ExecuteValidation(InvoiceDM document)
    {
        Report.Info("Step 1: Navigate to Sales Invoice");
        NavigateToModule("Sales");
        NavigateToListing("Invoice");
        OpenFormMode("New");
        SwitchToOldInterface();

        Report.Info("Step 2: Fill form with invalid/incomplete data");
        _headerHandler.Fill(document.Header);

        Report.Info("Step 3: Attempt to Save (expecting validation error)");
        ClickOnForm("Save");

        Report.Info("Step 4: Validate validation message");
        _messageValidator.ValidateValidationMessage(document.Expected);
    }

    // ── VALIDATIONS ────────────────────────────────────────────────────────

    private void ValidateAfterSave(InvoiceDM document)
    {
        if (document.Expected == null)
        {
            Report.Warning("No Expected values defined — skipping validation.");
            return;
        }

        _messageValidator.ValidateMessage(document.Expected?.Messages?.OnSave,
            "dx-toast-message",
            "Save Message"
        );
        _headerValidator.ValidateDocumentNumberGenerated();
    }

    private void ValidateAfterView(InvoiceDM document)
    {
        if (document.Expected == null)
        {
            Report.Warning("No Expected values defined — skipping validation.");
            return;
        }

        _linesValidator.ValidateLineTotals(document.Lines);

        var totals = _networkHelper.GetResponse<TotalsResponseDM>();

        _totalsValidator.ValidateTotalsFromApi(document.Expected, totals);
    }

    private void ValidateAfterApprove(InvoiceDM document)
    {
        if (document.Expected == null)
        {
            Report.Warning("No Expected values defined — skipping validation.");
            return;
        }

        _messageValidator.ValidateMessage(document.Expected?.Messages?.OnApprove,
            "dx-toast-message",
            "Approve Message"
        );
        _headerValidator.ValidateDocumentStatus(document.Expected);
        _headerValidator.ValidateDocumentPaymentStatus(document.Expected);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private void ExecuteStep(string stepName, Action action)
    {
        try
        {
            Report.Info($"Step: {stepName}");
            action();
        }
        catch (Exception ex)
        {
            Report.Fail($"Failed at step: {stepName} | {ex.Message}");
            throw;
        }
    }

    private void Save()
    {
        ClickOnForm("Save");
    }
}
