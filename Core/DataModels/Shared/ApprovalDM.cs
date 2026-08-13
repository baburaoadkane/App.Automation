using App.Automation.Core.Enums;

namespace App.Automation.Core.DataModels.Shared;

public class ApprovalDM
{
    /// <summary>
    /// Defines how the approval workflow should be executed.
    /// </summary>
    public ApprovalWorkflowType WorkflowType { get; set; }
        = ApprovalWorkflowType.None;

    /// <summary>
    /// Action to be performed by the approver.
    /// </summary>
    public ApprovalAction Action { get; set; }
        = ApprovalAction.None;

    /// <summary>
    /// Approval level for multi-level approval.
    /// Level 1, Level 2, Level 3, etc.
    /// </summary>
    public int ApprovalLevel { get; set; }

    /// <summary>
    /// Approver user/login.
    /// </summary>
    public string? Approver { get; set; }

    /// <summary>
    /// Delegated approver user/login.
    /// Used when approval is delegated.
    /// </summary>
    public string? DelegatedApprover { get; set; }

    /// <summary>
    /// Comments entered during approval action.
    /// Useful for Reject and Revise.
    /// </summary>
    public string? Comments { get; set; }
}