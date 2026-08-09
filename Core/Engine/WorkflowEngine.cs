namespace App.Automation.Core.Engine;

public class WorkflowEngine
{
    public void Execute(WorkflowDefinition workflow)
    {
        if (workflow == null)
            throw new ArgumentNullException(nameof(workflow));

        foreach (var step in workflow.Steps)
        {
            if (step.Action == null)
                continue;

            step.Action.Invoke();
        }
    }
}