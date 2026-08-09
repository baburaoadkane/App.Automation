namespace App.Automation.Core.Engine
{
    public class WorkflowDefinition
    {
        public string Name { get; set; } = "";

        public List<WorkflowStep> Steps { get; } = new();
    }
}
