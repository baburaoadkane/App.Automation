//     namespace App.Automation.Core.Engine
// {
//     public class WorkflowStep
//     {
//         public string Name { get; set; } = "";

//         public Action Action { get; set; } = () => { };
//     }
// }


using App.Automation.Core.Enums;

namespace App.Automation.Core.Engine;

public class WorkflowStep
{
    /// <summary>
    /// Display name of the workflow step.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Action executed by this workflow step.
    /// </summary>
    public Action Action { get; set; } = () => { };

    /// <summary>
    /// Determines whether this step should execute.
    /// </summary>
    public Func<bool> ShouldRun { get; set; } = () => true;

    /// <summary>
    /// Optional validation after the step.
    /// </summary>
    public Action? Validate { get; set; }

    /// <summary>
    /// Indicates whether this is an approval-related step.
    /// </summary>
    public bool IsApprovalStep { get; set; }

    /// <summary>
    /// Approval action represented by this step.
    /// </summary>
    public ApprovalAction ApprovalAction { get; set; } = ApprovalAction.None;

    /// <summary>
    /// Approval level.
    /// 0 means normal transaction step.
    /// 1 = first approval level, 2 = second level, etc.
    /// </summary>
    public int ApprovalLevel { get; set; }

    /// <summary>
    /// Whether the workflow should save after this step.
    /// </summary>
    public bool RequiresSave { get; set; }

    /// <summary>
    /// Whether workflow execution should stop if this step fails.
    /// </summary>
    public bool StopOnFailure { get; set; } = true;
}