// namespace App.Automation.Core.Engine
// {
//     public class WorkflowDefinition
//     {
//         public string Name { get; set; } = "";

//         public List<WorkflowStep> Steps { get; } = new();
//     }
// }


namespace App.Automation.Core.Engine;

public class WorkflowDefinition
{
    /// <summary>
    /// Name of the workflow.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Ordered workflow steps.
    /// </summary>
    public List<WorkflowStep> Steps { get; } = new();

    /// <summary>
    /// Adds a workflow step.
    /// </summary>
    public void AddStep(WorkflowStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        Steps.Add(step);
    }
}