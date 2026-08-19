using App.Automation.Core.Enums;

namespace App.Automation.Core.DataModels.Shared;

public class ApprovalStepDM
{
    /// <summary>
    /// Approval level.
    /// Example: 1, 2, 3...
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Approver user/login.
    /// </summary>
    public string? Approver { get; set; }

    /// <summary>
    /// Password for the approver.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Action to perform at this approval level.
    /// </summary>
    public ApprovalAction Action { get; set; }
        = ApprovalAction.None;    

    /// <summary>
    /// Comments for Reject/Revise/other approval actions.
    /// </summary>
    public string? Comments { get; set; }
}