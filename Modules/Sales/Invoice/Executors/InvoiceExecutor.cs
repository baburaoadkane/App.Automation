using App.Automation.Core.Base;
using App.Automation.Core.Engine;
using App.Automation.Core.Enums;
using App.Automation.Core.Utilities;
using App.Automation.Modules.Global.Sections;
using App.Automation.Modules.Global.Validators;
using App.Automation.Modules.Sales.Invoice.Approval;
using App.Automation.Modules.Sales.Invoice.Configuration;
using App.Automation.Modules.Sales.Invoice.DataModels;
using App.Automation.Core.DataModels.Shared;
using App.Automation.Modules.Sales.Invoice.HeaderHandlers;
using App.Automation.Modules.Sales.Invoice.LineHandlers;
using App.Automation.Modules.Sales.Invoice.Validators;
using OpenQA.Selenium;

namespace App.Automation.Modules.Sales.Invoice.Executors;

public class InvoiceExecutor : BaseExecutor<InvoiceDM>
{
    #region HANDLERS

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

    #endregion

    #region VALIDATORS

    private readonly HeaderValidator _headerValidator;
    private readonly LinesValidator _linesValidator;
    private readonly TotalsValidator _totalsValidator;
    private readonly MessageValidator _messageValidator;

    #endregion

    #region NETWORK

    private readonly NetworkHelper _networkHelper;

    #endregion

    #region CONSTRUCTOR

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

    #endregion

    #region EXECUTOR ENTRY POINT
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

                if (document.TxnParameter?.EnableApprovalWorkflow != false)
                {
                    throw new InvalidOperationException(
                        "Direct approval requires " +
                        "EnableApprovalWorkflow = false.");
                }

                ExecuteDirectApproval(document);

                break;

            case "SUBMIT":

                if (document.TxnParameter?.EnableApprovalWorkflow != true)
                {
                    throw new InvalidOperationException(
                        "Submit for approval requires " +
                        "EnableApprovalWorkflow = true.");
                }

                ExecuteSubmitForApproval(document);

                break;

            case "APPROVAL":

                if (document.TxnParameter?.EnableApprovalWorkflow != true)
                {
                    throw new InvalidOperationException(
                        "Approval workflow requires " +
                        "EnableApprovalWorkflow = true.");
                }

                ExecuteApproval(document);

                break;

            default:
                throw new ArgumentException(
                    $"Unknown ScenarioType: {document.ScenarioType}");
        }
    }
    #endregion

    #region CREATE EXECUTION
    private void ExecuteCreate(InvoiceDM document)
    {
        NavigateToInvoice();

        ExecuteStep(
            "Fill Header",
            () =>
            {
                _headerHandler.Fill(document.Header);

                Save();

                ValidateAfterSave(document);

            });

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

    #endregion

    #region DIRECT APPROVAL EXECUTION

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

    #endregion

    #region SUBMIT FOR APPROVAL EXECUTION

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

    #endregion    

    #region SUBMITTED REQUEST - APPROVAL EXECUTION

    private void ExecuteApproval(InvoiceDM document)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(document.Approval);

        // ================================================================
        // 1. CREATE → SAVE → VIEW
        // ================================================================

        ExecuteCreate(document);

        string documentNo =
            _expectationHandler.ReadDocumentNumber();

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
        // 3. EXECUTE APPROVAL WORKFLOW
        // ================================================================

        switch (document.Approval.WorkflowType)
        {
            case ApprovalWorkflowType.SubmitForApproval:

                ExecuteSingleLevelApproval(
                    document, documentNo);

                break;

            case ApprovalWorkflowType.MultiLevelApproval:

                ExecuteMultiLevelApproval(
                    document,
                    documentNo,
                    document.Approval.ApprovalSteps);

                break;

            default:

                throw new ArgumentException(
                    $"Unsupported approval workflow type: " +
                    $"{document.Approval.WorkflowType}");
        }
    }

    #endregion

    #region SINGLE LEVEL APPROVAL WORKFLOW

    private void ExecuteSingleLevelApproval(
        InvoiceDM document,
        string documentNo)
    {
        ArgumentNullException.ThrowIfNull(document.Approval);

        int approvalLevel =
            document.Approval.ApprovalLevel;

        if (approvalLevel <= 0)
        {
            approvalLevel = 1;
        }

        ApprovalAction action =
            document.Approval.Action;

        if (action == ApprovalAction.None)
        {
            throw new ArgumentException(
                "Approval action must be configured for " +
                "a single-level approval scenario.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
        document.Approval.Approver);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            document.Approval.Password);

        // ================================================================
        // LOGOUT SUBMITTER
        // ================================================================

        ExecuteStep(
            "Logout Submitter",
            () =>
            {
                _loginHelper.Logout();

                Wait.WaitForSeconds(2);
            });

        // ================================================================
        // LOGIN AS APPROVER
        // ================================================================

        ExecuteStep(
            "Login as Approver",
            () =>
            {
                _loginHelper.Login(
                    document.Approval.Approver!,
                    document.Approval.Password!);
            });

        // ================================================================
        // OPEN APPROVAL TRANSACTION
        // ================================================================

        OpenApprovalTransaction(documentNo);

        // ================================================================
        // EXECUTE ACTION
        // ================================================================

        ExecuteSingleApproval(document);
    }

    #endregion

    #region OPEN APPROVAL TRANSACTION

    private void OpenApprovalTransaction(
        string documentNo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentNo);

        ExecuteStep(
            "Open My Approvals",
            () =>
            {
                _approvalNavigationHandler
                    .ClickOnNotification();
            });

        ExecuteStep(
            $"Open Approval Transaction - {documentNo}",
            () =>
            {
                _approvalNavigationHandler
                    .FindAndOpenApprovalTransaction(
                        documentNo);
            });
    }

    #endregion

    #region SINGLE LEVEL APPROVAL EXECUTION

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

        ApprovalAction action =
            document.Approval.Action;

        if (action == ApprovalAction.None)
        {
            throw new ArgumentException(
                "Approval action must be configured for " +
                "a single-level approval scenario.");
        }

        Report.Info(
            $"Approval Level: {approvalLevel}");

        Report.Info(
            $"Approval Action: {action}");

        ExecuteApprovalAction(
            document,
            approvalLevel,
            action,
            document.Approval.Comments,
            isFinalApprovalLevel: true);
    }

    #endregion

    #region MULTI LEVEL APPROVAL EXECUTION

    private void ExecuteMultiLevelApproval(
        InvoiceDM document,
        string documentNo,
        List<ApprovalStepDM> approvalSteps)
    {
        ValidateApprovalSteps(approvalSteps);

        var orderedApprovalSteps =
            approvalSteps
                .OrderBy(x => x.Level)
                .ToList();

        // ================================================================
        // PROCESS EACH APPROVAL LEVEL
        // ================================================================

        for (int i = 0;
             i < orderedApprovalSteps.Count;
             i++)
        {
            var approvalStep =
                orderedApprovalSteps[i];
            
            Report.Info(
                "================================================");

            Report.Info(
                $"Starting Approval Level: " +
                $"{approvalStep.Level}");

            Report.Info(
                $"Approver: " +
                $"{approvalStep.Approver}");

            Report.Info(
                $"Action: " +
                $"{approvalStep.Action}");

            Report.Info(
                "================================================");

            // ============================================================
            // LOGOUT CURRENT USER
            //
            // Level 1:
            // Submitter → Logout
            //
            // Level 2+:
            // Previous Approver → Logout
            // ============================================================

            ExecuteStep(
                $"Logout Current User - Before Level {approvalStep.Level}",
                () =>
                {
                    _loginHelper.Logout();

                    WaitForPageLoad();

                    Wait.WaitForSeconds(3);
                });

            // ============================================================
            // LOGIN AS CURRENT LEVEL APPROVER
            // ============================================================

            ExecuteStep(
                $"Login Approver - Level {approvalStep.Level}",
                () =>
                {
                    _loginHelper.Login(
                        approvalStep.Approver!,
                        approvalStep.Password!);
                });

            // ============================================================
            // OPEN APPROVAL TRANSACTION
            // ============================================================

            OpenApprovalTransaction(
                documentNo);

            // ============================================================
            // EXECUTE APPROVAL ACTION
            // ============================================================

            bool isFinalApprovalLevel =
                i == orderedApprovalSteps.Count - 1;

            ExecuteApprovalAction(
                document,
                approvalStep.Level,
                approvalStep.Action,
                approvalStep.Comments,
                isFinalApprovalLevel);

            // ============================================================
            // STOP WORKFLOW
            //
            // Reject → Stop
            // Revise → Stop
            //
            // Only Approve continues to next level.
            // ============================================================

            if (approvalStep.Action ==
                    ApprovalAction.Reject ||
                approvalStep.Action ==
                    ApprovalAction.Revise)
            {
                Report.Info(
                    $"Multi-level approval stopped at " +
                    $"Level {approvalStep.Level}. " +
                    $"Action: {approvalStep.Action}");

                break;
            }

            Report.Info(
                $"Approval Level {approvalStep.Level} " +
                $"completed successfully.");
        }
    }

    #endregion

    #region APPROVAL STEP VALIDATION

    private static void ValidateApprovalStep(
        ApprovalStepDM approvalStep)
    {
        ArgumentNullException.ThrowIfNull(
            approvalStep);

        if (approvalStep.Level <= 0)
        {
            throw new ArgumentException(
                "Approval level must be greater than zero.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            approvalStep.Approver);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            approvalStep.Password);

        if (approvalStep.Action != ApprovalAction.Approve &&
        approvalStep.Action != ApprovalAction.Reject &&
        approvalStep.Action != ApprovalAction.Revise)
        {
            throw new ArgumentException(
                $"Unsupported approval action " +
                $"'{approvalStep.Action}' for Level " +
                $"{approvalStep.Level}. " +
                "Only Approve, Reject, and Revise are supported.");
        }
    }

    #endregion

    #region APPROVAL WORKFLOW VALIDATION

    private static void ValidateApprovalSteps(
        List<ApprovalStepDM> approvalSteps)
    {
        if (approvalSteps == null ||
            approvalSteps.Count == 0)
        {
            throw new ArgumentException(
                "Approval steps must be configured for " +
                "a multi-level approval workflow.");
        }

        var orderedSteps = approvalSteps
            .OrderBy(x => x.Level)
            .ToList();

        // ================================================================
        // VALIDATE FIRST LEVEL
        // ================================================================

        if (orderedSteps[0].Level != 1)
        {
            throw new ArgumentException(
                "Multi-level approval must start from Level 1.");
        }

        // ================================================================
        // VALIDATE SEQUENTIAL LEVELS
        // ================================================================

        for (int i = 0; i < orderedSteps.Count; i++)
        {
            int expectedLevel = i + 1;

            if (orderedSteps[i].Level != expectedLevel)
            {
                throw new ArgumentException(
                    $"Invalid approval level sequence. " +
                    $"Expected Level {expectedLevel}, " +
                    $"but found Level {orderedSteps[i].Level}.");
            }

            ValidateApprovalStep(orderedSteps[i]);
        }
    }

    #endregion

    #region APPROVAL ACTIONS
    private void ExecuteApprovalAction(
        InvoiceDM document,
        int approvalLevel,
        ApprovalAction action,
        string? comments = null,
        bool isFinalApprovalLevel = false)
    {
        switch (action)
        {
            case ApprovalAction.Approve:

                ExecuteStep(
                    $"Approve Level {approvalLevel}",
                    () =>
                    {
                        _approvalHandler.Approve(comments);

                        if (isFinalApprovalLevel)
                        {
                            ValidateAfterApprove(document);
                        }
                        else
                        {
                            Report.Info(
                                $"Level {approvalLevel} approved successfully. " +
                                "Final approval validation will be performed " +
                                "after the last approval level.");
                        }
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
    #endregion

    #region VALIDATION EXECUTION
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
    #endregion

    #region SAVE ACTION
    private void Save()
    {
        ClickOnForm("Save");
    }
    #endregion

    #region NAVIGATION
    private void NavigateToInvoice()
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
    #endregion

    #region NETWORK CALL
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
    #endregion

    #region VALIDATIONS
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

    #endregion

    #region STEPS EXECUTION

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
    #endregion 

}