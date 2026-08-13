using App.Automation.Core.Engine;
using App.Automation.Core.Enums;

namespace App.Automation.Modules.Sales.Invoice.Configuration;

public static class ApprovalWorkflow
{
    // ================================================================
    // DIRECT APPROVAL
    // ================================================================

    /// <summary>
    /// Workflow:
    /// Create → Save → View → Direct Approve
    /// </summary>
    public static WorkflowDefinition CreateDirectApprovalWorkflow(
        Action approve,
        Action validate)
    {
        ArgumentNullException.ThrowIfNull(approve);
        ArgumentNullException.ThrowIfNull(validate);

        var workflow = new WorkflowDefinition
        {
            Name = "Sales Invoice - Direct Approval"
        };

        workflow.AddStep(new WorkflowStep
        {
            Name = "Approve Invoice",
            Action = approve,
            Validate = validate,
            IsApprovalStep = true,
            ApprovalAction = ApprovalAction.Approve,
            ApprovalLevel = 0
        });

        return workflow;
    }


    // ================================================================
    // SUBMIT FOR APPROVAL
    // ================================================================

    /// <summary>
    /// Workflow:
    /// Create → Save → View → Submit for Approval
    /// </summary>
    public static WorkflowDefinition CreateSubmitWorkflow(
        Action submit,
        Action validateSubmit)
    {
        ArgumentNullException.ThrowIfNull(submit);
        ArgumentNullException.ThrowIfNull(validateSubmit);

        var workflow = new WorkflowDefinition
        {
            Name = "Sales Invoice - Submit for Approval"
        };

        workflow.AddStep(new WorkflowStep
        {
            Name = "Submit for Approval",
            Action = submit,
            Validate = validateSubmit,
            IsApprovalStep = true,
            ApprovalAction = ApprovalAction.Submit,
            ApprovalLevel = 0
        });

        return workflow;
    }


    // ================================================================
    // APPROVAL LEVEL
    // ================================================================

    /// <summary>
    /// Creates one approval level.
    ///
    /// At each level the approver can perform:
    ///     Approve
    ///     Reject
    ///     Revise
    ///
    /// Example:
    /// Level 1 → Approve
    /// Level 1 → Reject
    /// Level 1 → Revise
    ///
    /// The executor decides which action is actually performed.
    /// </summary>
    public static WorkflowDefinition CreateApprovalLevelWorkflow(
        int approvalLevel,
        Action approve,
        Action reject,
        Action revise,
        Action validate)
    {
        if (approvalLevel <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(approvalLevel),
                "Approval level must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(approve);
        ArgumentNullException.ThrowIfNull(reject);
        ArgumentNullException.ThrowIfNull(revise);
        ArgumentNullException.ThrowIfNull(validate);

        var workflow = new WorkflowDefinition
        {
            Name =
                $"Sales Invoice - Approval Level {approvalLevel}"
        };

        // ------------------------------------------------------------
        // APPROVE
        // ------------------------------------------------------------

        workflow.AddStep(new WorkflowStep
        {
            Name = $"Approve Level {approvalLevel}",
            Action = approve,
            Validate = validate,
            IsApprovalStep = true,
            ApprovalAction = ApprovalAction.Approve,
            ApprovalLevel = approvalLevel
        });

        // ------------------------------------------------------------
        // REJECT
        // ------------------------------------------------------------

        workflow.AddStep(new WorkflowStep
        {
            Name = $"Reject Level {approvalLevel}",
            Action = reject,
            IsApprovalStep = true,
            ApprovalAction = ApprovalAction.Reject,
            ApprovalLevel = approvalLevel
        });

        // ------------------------------------------------------------
        // REVISE
        // ------------------------------------------------------------

        workflow.AddStep(new WorkflowStep
        {
            Name = $"Revise Level {approvalLevel}",
            Action = revise,
            IsApprovalStep = true,
            ApprovalAction = ApprovalAction.Revise,
            ApprovalLevel = approvalLevel
        });

        return workflow;
    }
}