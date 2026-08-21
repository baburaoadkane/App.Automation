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
    /// Action to be performed.
    /// Used for simple approval scenarios.
    /// For multi-level approval, use ApprovalSteps.
    /// </summary>
    public ApprovalAction Action { get; set; }
        = ApprovalAction.None;

    /// <summary>
    /// Current approval level.
    /// Example: 1, 2, 3...
    /// </summary>
    public int ApprovalLevel { get; set; }

    /// <summary>
    /// Approver user/login.
    /// </summary>
    public string? Approver { get; set; }

    /// <summary>
    /// Approver password.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Comments entered during approval action.
    /// Especially useful for Reject and Revise.
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Multiple approval-level actions.
    /// Used for multi-level approval workflows.
    /// </summary>
    public List<ApprovalStepDM> ApprovalSteps { get; set; } = new();
}