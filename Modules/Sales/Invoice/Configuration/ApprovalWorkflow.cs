using App.Automation.Core.Engine;
using App.Automation.Core.Enums;

namespace App.Automation.Modules.Sales.Invoice.Configuration;

public static class ApprovalWorkflow
{
    /// <summary>
    /// Creates the workflow for submitting an invoice for approval.
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

    /// <summary>
    /// Creates an approval-level workflow.
    /// </summary>
    public static WorkflowDefinition CreateApprovalWorkflow(
        int approvalLevel,
        Action approve,
        Action reject,
        Action revise,
        Action delegateApproval,
        Action validate)
    {
        if (approvalLevel <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(approvalLevel),
                "Approval level must be greater than zero.");

        ArgumentNullException.ThrowIfNull(approve);
        ArgumentNullException.ThrowIfNull(reject);
        ArgumentNullException.ThrowIfNull(revise);
        ArgumentNullException.ThrowIfNull(delegateApproval);
        ArgumentNullException.ThrowIfNull(validate);

        var workflow = new WorkflowDefinition
        {
            Name = $"Sales Invoice - Approval Level {approvalLevel}"
        };

        workflow.AddStep(new WorkflowStep
        {
            Name = $"Approve Level {approvalLevel}",
            Action = approve,
            IsApprovalStep = true,
            ApprovalAction = ApprovalAction.Approve,
            ApprovalLevel = approvalLevel,
            Validate = validate
        });

        return workflow;
    }
}