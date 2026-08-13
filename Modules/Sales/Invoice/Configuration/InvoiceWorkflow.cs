//using App.Automation.Core.Engine;

//namespace App.Automation.Modules.Sales.Invoice.Configuration;

//public static class InvoiceWorkflow
//{
//    /// <summary>
//    /// Creates the Sales Invoice workflow.
//    ///
//    /// Section execution and section-level saving are handled
//    /// by SectionEngine.
//    ///
//    /// Supported workflow:
//    ///
//    /// Create
//    ///     → Fill Sections
//    ///     → View
//    ///     → Validate
//    ///
//    /// Approval Workflow
//    ///     → Fill Sections
//    ///     → View
//    ///     → Validate
//    ///     → Submit For Approval
//    ///     → Approval Process
//    ///
//    /// The Approval Process is intentionally represented as one
//    /// workflow step because the approval handler is responsible
//    /// for handling:
//    ///     - Approve
//    ///     - Reject
//    ///     - Revise
//    ///     - Delegate
//    ///     - Multiple approval levels
//    /// </summary>
//    public static WorkflowDefinition Create(
//        Action fillSections,
//        Action view,
//        Action validate,
//        Action? submitForApproval = null,
//        Action? approvalProcess = null)
//    {
//        ArgumentNullException.ThrowIfNull(fillSections);
//        ArgumentNullException.ThrowIfNull(view);
//        ArgumentNullException.ThrowIfNull(validate);

//        var workflow = new WorkflowDefinition
//        {
//            Name = "Sales Invoice Workflow"
//        };

//        // =============================================================
//        // 1. FILL SECTIONS
//        // =============================================================

//        workflow.Steps.Add(new WorkflowStep
//        {
//            Name = "Fill Sections",
//            Action = fillSections
//        });

//        // =============================================================
//        // 2. VIEW
//        // =============================================================

//        workflow.Steps.Add(new WorkflowStep
//        {
//            Name = "View",
//            Action = view
//        });

//        // =============================================================
//        // 3. VALIDATE
//        // =============================================================

//        workflow.Steps.Add(new WorkflowStep
//        {
//            Name = "Validate",
//            Action = validate
//        });

//        // =============================================================
//        // 4. SUBMIT FOR APPROVAL
//        // =============================================================

//        if (submitForApproval != null)
//        {
//            workflow.Steps.Add(new WorkflowStep
//            {
//                Name = "Submit For Approval",
//                Action = submitForApproval
//            });
//        }

//        // =============================================================
//        // 5. APPROVAL PROCESS
//        // =============================================================

//        if (approvalProcess != null)
//        {
//            workflow.Steps.Add(new WorkflowStep
//            {
//                Name = "Approval Process",
//                Action = approvalProcess
//            });
//        }

//        return workflow;
//    }
//}

//using App.Automation.Core.Engine;

//namespace App.Automation.Modules.Sales.Invoice.Configuration;

//public static class InvoiceWorkflow
//{
//    public static WorkflowDefinition Create(
//        Action fillSections,
//        Action view,
//        Action validate)
//    {
//        ArgumentNullException.ThrowIfNull(fillSections);
//        ArgumentNullException.ThrowIfNull(view);
//        ArgumentNullException.ThrowIfNull(validate);

//        var workflow = new WorkflowDefinition
//        {
//            Name = "Sales Invoice - Create"
//        };

//        workflow.AddStep(new WorkflowStep
//        {
//            Name = "Fill Sections",
//            Action = fillSections
//        });

//        workflow.AddStep(new WorkflowStep
//        {
//            Name = "View",
//            Action = view
//        });

//        workflow.AddStep(new WorkflowStep
//        {
//            Name = "Validate",
//            Action = validate
//        });

//        return workflow;
//    }
//}

using App.Automation.Core.Engine;
using App.Automation.Core.Enums;

namespace App.Automation.Modules.Sales.Invoice.Configuration;

public static class InvoiceWorkflow
{
    public static WorkflowDefinition Create(
        Action fillSections,
        Action view,
        Action validate,
        Action? submit = null,
        Action? approve = null,
        Action? reject = null,
        Action? revise = null,
        Action? delegateApproval = null)
    {
        ArgumentNullException.ThrowIfNull(fillSections);
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(validate);

        var workflow = new WorkflowDefinition
        {
            Name = "Sales Invoice Workflow"
        };

        // =============================================================
        // FILL SECTIONS
        // =============================================================
        workflow.AddStep(new WorkflowStep
        {
            Name = "Fill Sections",
            Action = fillSections,
            RequiresSave = false
        });

        // =============================================================
        // VIEW
        // =============================================================
        workflow.AddStep(new WorkflowStep
        {
            Name = "View",
            Action = view,
            RequiresSave = false
        });

        // =============================================================
        // VALIDATE
        // =============================================================
        workflow.AddStep(new WorkflowStep
        {
            Name = "Validate",
            Action = validate,
            RequiresSave = false
        });

        // =============================================================
        // SUBMIT FOR APPROVAL
        // =============================================================
        if (submit != null)
        {
            workflow.AddStep(new WorkflowStep
            {
                Name = "Submit for Approval",
                Action = submit,
                IsApprovalStep = true,
                ApprovalAction = ApprovalAction.Submit,
                RequiresSave = false
            });
        }

        // =============================================================
        // APPROVE
        // =============================================================
        if (approve != null)
        {
            workflow.AddStep(new WorkflowStep
            {
                Name = "Approve",
                Action = approve,
                IsApprovalStep = true,
                ApprovalAction = ApprovalAction.Approve,
                RequiresSave = false
            });
        }

        // =============================================================
        // REJECT
        // =============================================================
        if (reject != null)
        {
            workflow.AddStep(new WorkflowStep
            {
                Name = "Reject",
                Action = reject,
                IsApprovalStep = true,
                ApprovalAction = ApprovalAction.Reject,
                RequiresSave = false
            });
        }

        // =============================================================
        // REVISE
        // =============================================================
        if (revise != null)
        {
            workflow.AddStep(new WorkflowStep
            {
                Name = "Revise",
                Action = revise,
                IsApprovalStep = true,
                ApprovalAction = ApprovalAction.Revise,
                RequiresSave = false
            });
        }

        // =============================================================
        // DELEGATE
        // =============================================================
        if (delegateApproval != null)
        {
            workflow.AddStep(new WorkflowStep
            {
                Name = "Delegate Approval",
                Action = delegateApproval,
                IsApprovalStep = true,
                ApprovalAction = ApprovalAction.Delegate,
                RequiresSave = false
            });
        }

        return workflow;
    }
}