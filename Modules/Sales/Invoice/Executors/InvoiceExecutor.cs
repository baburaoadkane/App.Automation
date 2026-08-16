using App.Automation.Core.Base;
using App.Automation.Core.Engine;
using App.Automation.Core.Enums;
using App.Automation.Core.Utilities;
using App.Automation.Modules.Global.Sections;
using App.Automation.Modules.Global.Validators;
using App.Automation.Modules.Sales.Invoice.Approval;
using App.Automation.Modules.Sales.Invoice.Configuration;
using App.Automation.Modules.Sales.Invoice.DataModels;
using App.Automation.Modules.Sales.Invoice.HeaderHandlers;
using App.Automation.Modules.Sales.Invoice.LineHandlers;
using App.Automation.Modules.Sales.Invoice.Validators;
using OpenQA.Selenium;

namespace App.Automation.Modules.Sales.Invoice.Executors;

public class InvoiceExecutor : BaseExecutor<InvoiceDM>
{
    // ================================================================
    // HANDLERS
    // ================================================================
    private readonly LoginHelper _loginHelper;
    private readonly InvoiceHeaderHandler _headerHandler;
    private readonly InvoiceLineHandler _linesHandler;
    private readonly DiscountHandler _discountHandler;
    private readonly ChargesHandler _chargesHandler;
    private readonly PaymentsHandler _paymentsHandler;
    private readonly OthersHandler _othersHandler;
    private readonly ExpectationHandler _expectationHandler;
    private readonly InvoiceApprovalHandler _approvalHandler;
    private readonly ApprovalNavigationHandler _approvalNavigationHandler;

    // ================================================================
    // VALIDATORS
    // ================================================================

    private readonly HeaderValidator _headerValidator;
    private readonly LinesValidator _linesValidator;
    private readonly TotalsValidator _totalsValidator;
    private readonly MessageValidator _messageValidator;

    // ================================================================
    // NETWORK
    // ================================================================

    private readonly NetworkHelper _networkHelper;

    // ================================================================
    // CONSTRUCTOR
    // ================================================================

    public InvoiceExecutor(
        IWebDriver driver,
        WaitHelper wait,
        ReportHelper report)
        : base(driver, wait, report)
    {
        _loginHelper = new LoginHelper(driver, wait);

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

        _approvalHandler =
            new InvoiceApprovalHandler(driver, wait, report);

        _approvalNavigationHandler =
            new ApprovalNavigationHandler(driver, wait, report);

        _headerValidator =
            new HeaderValidator(driver, wait, report,
                _expectationHandler);

        _linesValidator =
            new LinesValidator(driver, wait, report,
                _expectationHandler);

        _totalsValidator =
            new TotalsValidator(driver, wait, report,
                _expectationHandler);

        _messageValidator =
            new MessageValidator(driver, wait, report,
                _expectationHandler);

        _networkHelper =
            new NetworkHelper(driver);
    }

    // ================================================================
    // ENTRY POINT
    // ================================================================

    public override void Execute(InvoiceDM document)
    {
        ArgumentNullException.ThrowIfNull(document);

        Report.Info(
            $"── Sales Invoice Executor: {document.ScenarioType} ──");

        Report.Info(
            $"Test: {document.TestDescription}");

        switch (document.ScenarioType?.Trim().ToUpperInvariant())
        {
            case "CREATE":
                ExecuteCreate(document);
                break;

            case "VALIDATION":
                ExecuteValidation(document);
                break;

            case "DIRECT_APPROVAL":
                ExecuteDirectApproval(document);
                break;

            case "SUBMIT":
                ExecuteSubmitForApproval(document);
                break;

            case "APPROVAL":
                ExecuteApproval(document);
                break;

            case "REJECT":
                ExecuteReject(document);
                break;

            case "REVISE":
                ExecuteRevise(document);
                break;

            default:
                throw new ArgumentException(
                    $"Unknown ScenarioType: {document.ScenarioType}");
        }
    }

    // ================================================================
    // CREATE
    // ================================================================

    private void ExecuteCreate(InvoiceDM document)
    {
        NavigateToInvoiceNew();

        // ------------------------------------------------------------
        // HEADER
        // ------------------------------------------------------------

        ExecuteStep(
            "Fill Header",
            () =>
            {
                _headerHandler.Fill(document.Header);

                Save();

                ValidateAfterSave(document);

            });

        // ------------------------------------------------------------
        // SECTIONS
        //
        // InvoiceSections defines:
        //     Lines
        //     Discount
        //     Charges
        //     Payments
        //     Others
        //
        // SectionEngine automatically saves after every section
        // because RequiresSave = true.
        // ------------------------------------------------------------

        var sections =
            InvoiceSections.Create(
                _linesHandler,
                _discountHandler,
                _chargesHandler,
                _paymentsHandler,
                _othersHandler);

        var sectionEngine =
            new SectionEngine<InvoiceDM>(
                sections,
                Save,
                Report);

        ExecuteStep(
            "Execute Invoice Sections",
            () =>
            {
                sectionEngine.Execute(document);
            });

        // ------------------------------------------------------------
        // TOTALS API CAPTURE
        // ------------------------------------------------------------

        StartTotalsCapture();

        // ------------------------------------------------------------
        // VIEW
        // ------------------------------------------------------------

        ExecuteStep(
            "Open View Mode",
            () =>
            {
                ClickOnForm("View");
            });

        // ------------------------------------------------------------
        // VALIDATE
        // ------------------------------------------------------------

        ExecuteStep(
            "Validate Invoice",
            () =>
            {
                ValidateAfterView(document);
            });
    }

    // ================================================================
    // DIRECT APPROVAL
    //
    // Workflow:
    //
    // Create
    //   ↓
    // Save
    //   ↓
    // View
    //   ↓
    // Direct Approve
    //
    // ================================================================

    private void ExecuteDirectApproval(InvoiceDM document)
    {
        ExecuteCreate(document);

        var workflow =
            ApprovalWorkflow.CreateDirectApprovalWorkflow(
                approve: () =>
                {
                    ClickOnForm("Approve");
                },

                validate: () =>
                {
                    ValidateAfterApprove(document);
                });

        var engine =
            new WorkflowEngine(Report);

        engine.Execute(workflow);
    }

    // ================================================================
    // SUBMIT FOR APPROVAL
    //
    // Workflow:
    //
    // Create
    //   ↓
    // Save
    //   ↓
    // View
    //   ↓
    // Submit for Approval
    //
    // Approver acts separately.
    //
    // ================================================================

    private void ExecuteSubmitForApproval(InvoiceDM document)
    {
        ExecuteCreate(document);

        var workflow =
            ApprovalWorkflow.CreateSubmitWorkflow(
                submit: () =>
                {
                    _approvalHandler.Submit();
                },

                validateSubmit: () =>
                {
                    ValidateAfterSubmit(document);
                });

        var engine =
            new WorkflowEngine(Report);

        engine.Execute(workflow);
    }

    // ================================================================
    // APPROVAL
    //
    // Supports:
    //
    // Single Level:
    //     Level 1 → Approve / Reject / Revise
    //
    // Multi Level:
    //     Level 1 → Approve
    //     Level 2 → Approve
    //     Level 3 → Approve
    //
    // Or any level can Reject / Revise.
    //
    // ================================================================

    private void ExecuteApproval(InvoiceDM document)
    {
        ArgumentNullException.ThrowIfNull(document);

        // ================================================================
        // 1. CREATE → SAVE → VIEW
        // ================================================================

        ExecuteCreate(document);

        _headerValidator
                    .ValidateDocumentNumberGenerated();

        string documentNo = _expectationHandler.ReadDocumentNumber();

        // ================================================================
        // 2. SUBMIT FOR APPROVAL
        // ================================================================

        ExecuteStep(
            "Submit Invoice for Approval",
            () =>
            {
                _approvalHandler.Submit();

                ValidateAfterSubmit(document);
            });

        // ================================================================
        // 3. LOGOUT SUBMITTER
        // ================================================================

        ExecuteStep(
            "Logout Submitter",
            () =>
            {
                _loginHelper.Logout();
                Thread.Sleep(1000);
            });

        // ================================================================
        // 4. LOGIN AS APPROVER
        // ================================================================

        ExecuteStep(
            "Login as Approver",
            () =>
            {
                _loginHelper.Login(
                    Config.ApproverUsername,
                    Config.ApproverPassword);
            });

        // ================================================================
        // 5. OPEN NOTIFICATION → MY APPROVALS
        // ================================================================

        ExecuteStep(
            "Open My Approvals",
            () =>
            {
                _approvalNavigationHandler.ClickOnNotification();

                //_approvalNavigationHandler.ClickOnMyApprovals();
            });

        // ================================================================
        // 6. FIND AND OPEN TRANSACTION
        // ================================================================        

        ExecuteStep(
            $"Open Approval Transaction - {documentNo}",
            () =>
            {
                _approvalNavigationHandler
                    .FindAndOpenApprovalTransaction(
                        documentNo);
            });

        // ================================================================
        // 7. APPROVE
        // ================================================================

        ExecuteStep(
            "Approve Invoice",
            () =>
            {
                _approvalHandler.Approve();

                ValidateAfterApprove(document);
            });
    }

    // ================================================================
    // SINGLE LEVEL APPROVAL
    // ================================================================

    private void ExecuteSingleApproval(
        InvoiceDM document)
    {
        ArgumentNullException.ThrowIfNull(document.Approval);

        int approvalLevel =
            document.Approval.ApprovalLevel;

        if (approvalLevel <= 0)
        {
            approvalLevel = 1;
        }

        var action =
            document.Approval.Action;

        Report.Info(
            $"Approval Level: {approvalLevel}");

        Report.Info(
            $"Approval Action: {action}");

        ExecuteApprovalAction(
            document,
            approvalLevel,
            action);
    }

    // ================================================================
    // MULTI LEVEL APPROVAL
    // ================================================================

    private void ExecuteMultiLevelApproval(
        InvoiceDM document,
        List<Core.DataModels.Shared.ApprovalStepDM> approvalSteps)
    {
        foreach (var approvalStep in
                 approvalSteps.OrderBy(x => x.Level))
        {
            if (approvalStep.Level <= 0)
            {
                throw new ArgumentException(
                    $"Invalid approval level: " +
                    $"{approvalStep.Level}");
            }

            if (approvalStep.Action == ApprovalAction.None)
            {
                throw new ArgumentException(
                    $"Approval action is not configured " +
                    $"for Level {approvalStep.Level}.");
            }

            Report.Info(
                $"================================================");

            Report.Info(
                $"Approval Level: {approvalStep.Level}");

            Report.Info(
                $"Approver: {approvalStep.Approver}");

            Report.Info(
                $"Action: {approvalStep.Action}");

            Report.Info(
                $"================================================");

            // --------------------------------------------------------
            // Navigate to pending approval for this level
            // --------------------------------------------------------

            NavigateToPendingApproval(document);

            // --------------------------------------------------------
            // Execute configured action
            // --------------------------------------------------------

            ExecuteApprovalAction(
                document,
                approvalStep.Level,
                approvalStep.Action,
                approvalStep.Comments);

            // --------------------------------------------------------
            // Stop immediately if request is rejected or revised.
            //
            // There is no next approval level after:
            //
            // Reject
            // Revise
            //
            // --------------------------------------------------------

            if (approvalStep.Action == ApprovalAction.Reject ||
                approvalStep.Action == ApprovalAction.Revise)
            {
                Report.Info(
                    $"Approval workflow ended at Level " +
                    $"{approvalStep.Level} with action " +
                    $"{approvalStep.Action}.");

                break;
            }
        }
    }

    // ================================================================
    // APPROVAL ACTION
    // ================================================================

    private void ExecuteApprovalAction(
        InvoiceDM document,
        int approvalLevel,
        ApprovalAction action,
        string? comments = null)
    {
        switch (action)
        {
            case ApprovalAction.Approve:

                ExecuteStep(
                    $"Approve Level {approvalLevel}",
                    () =>
                    {
                        _approvalHandler.Approve();

                        ValidateAfterApprove(document);
                    });

                break;

            case ApprovalAction.Reject:

                ExecuteStep(
                    $"Reject Level {approvalLevel}",
                    () =>
                    {
                        _approvalHandler.Reject(comments);

                        ValidateAfterReject(document);
                    });

                break;

            case ApprovalAction.Revise:

                ExecuteStep(
                    $"Revise Level {approvalLevel}",
                    () =>
                    {
                        _approvalHandler.Revise(comments);

                        ValidateAfterRevise(document);
                    });

                break;

            default:

                throw new ArgumentException(
                    $"Unsupported approval action " +
                    $"'{action}' at approval level " +
                    $"{approvalLevel}.");
        }
    }

    // ================================================================
    // REJECT SCENARIO
    // ================================================================

    private void ExecuteReject(InvoiceDM document)
    {
        ArgumentNullException.ThrowIfNull(document.Approval);

        NavigateToPendingApproval(document);

        int approvalLevel =
            document.Approval.ApprovalLevel > 0
                ? document.Approval.ApprovalLevel
                : 1;

        ExecuteApprovalAction(
            document,
            approvalLevel,
            ApprovalAction.Reject,
            document.Approval.Comments);
    }

    // ================================================================
    // REVISE SCENARIO
    // ================================================================

    private void ExecuteRevise(InvoiceDM document)
    {
        ArgumentNullException.ThrowIfNull(document.Approval);

        NavigateToPendingApproval(document);

        int approvalLevel =
            document.Approval.ApprovalLevel > 0
                ? document.Approval.ApprovalLevel
                : 1;

        ExecuteApprovalAction(
            document,
            approvalLevel,
            ApprovalAction.Revise,
            document.Approval.Comments);
    }

    // ================================================================
    // VALIDATION SCENARIO
    // ================================================================

    private void ExecuteValidation(InvoiceDM document)
    {
        ExecuteStep(
            "Navigate to Sales Invoice",
            () =>
            {
                NavigateToModule("Sales");
                NavigateToListing("Invoice");
                OpenFormMode("New");
                SwitchToOldInterface();
            });

        ExecuteStep(
            "Fill Invalid Invoice",
            () =>
            {
                _headerHandler.Fill(document.Header);
            });

        ExecuteStep(
            "Attempt Save",
            () =>
            {
                ClickOnForm("Save");
            });

        ExecuteStep(
            "Validate Validation Message",
            () =>
            {
                _messageValidator
                    .ValidateValidationMessage(
                        document.Expected);
            });
    }

    // ================================================================
    // SAVE
    // ================================================================

    private void Save()
    {
        ClickOnForm("Save");
    }

    // ================================================================
    // NAVIGATION
    // ================================================================

    private void NavigateToInvoiceNew()
    {
        ExecuteStep(
            "Navigate to Sales Invoice",
            () =>
            {
                NavigateToModule("Sales");
                NavigateToListing("Invoice");
                OpenFormMode("New");
                SwitchToOldInterface();
            });
    }

    private void NavigateToPendingApproval(
        InvoiceDM document)
    {
        Report.Info(
            $"Navigate to pending approval " +
            $"for document: {document.DocumentNo}");

        // TODO:
        // Replace this implementation with the actual
        // ERP navigation to the Pending Approval screen.
        //
        // Example:
        //
        // NavigateToModule("Sales");
        // NavigateToListing("Invoice");
        // OpenPendingApproval(document.DocumentNo);
    }

    // ================================================================
    // NETWORK
    // ================================================================

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

    // ================================================================
    // VALIDATIONS
    // ================================================================

    private void ValidateAfterSave(
        InvoiceDM document)
    {
        if (document.Expected == null)
        {
            Report.Info(
                "No Expected values defined — " +
                "skipping validation.");

            return;
        }

        _messageValidator.ValidateMessage(
            document.Expected.Messages?.OnSave,
            "dx-toast-message",
            "Save Message");

        _headerValidator
            .ValidateDocumentNumberGenerated();
    }

    private void ValidateAfterView(
        InvoiceDM document)
    {
        if (document.Expected == null)
        {
            Report.Info(
                "No Expected values defined — " +
                "skipping validation.");

            return;
        }

        _linesValidator
            .ValidateLineTotals(document.Lines);

        var totals =
            _networkHelper
                .GetResponse<TotalsResponseDM>();

        _totalsValidator
            .ValidateTotalsFromApi(
                document.Expected,
                totals);
    }

    private void ValidateAfterSubmit(
        InvoiceDM document)
    {
        if (document.Expected == null)
        {
            Report.Info(
                "No Expected values defined — " +
                "skipping validation.");

            return;
        }

        _messageValidator.ValidateMessage(
            document.Expected.Messages?.OnSubmit,
            "dx-toast-message",
            "Submit Message");
    }

    private void ValidateAfterApprove(
        InvoiceDM document)
    {
        if (document.Expected == null)
        {
            Report.Info(
                "No Expected values defined — " +
                "skipping validation.");

            return;
        }

        _messageValidator.ValidateMessage(
            document.Expected.Messages?.OnApprove,
            "dx-toast-message",
            "Approve Message");

        _headerValidator
            .ValidateDocumentStatus(
                document.Expected);

        _headerValidator
            .ValidateDocumentPaymentStatus(
                document.Expected);
    }

    private void ValidateAfterReject(
        InvoiceDM document)
    {
        if (document.Expected == null)
        {
            Report.Info(
                "No Expected values defined — " +
                "skipping validation.");

            return;
        }

        _messageValidator.ValidateMessage(
            document.Expected.Messages?.OnReject,
            "dx-toast-message",
            "Reject Message");
    }

    private void ValidateAfterRevise(
        InvoiceDM document)
    {
        if (document.Expected == null)
        {
            Report.Info(
                "No Expected values defined — " +
                "skipping validation.");

            return;
        }

        _messageValidator.ValidateMessage(
            document.Expected.Messages?.OnRevise,
            "dx-toast-message",
            "Revise Message");
    }

    // ================================================================
    // GENERIC STEP EXECUTION
    // ================================================================

    private void ExecuteStep(
        string stepName,
        Action action)
    {
        try
        {
            Report.Info(
                $"Step: {stepName}");

            action();
        }
        catch (Exception ex)
        {
            Report.Fail(
                $"Failed at step: {stepName} | " +
                $"{ex.Message}");

            throw;
        }
    }
}