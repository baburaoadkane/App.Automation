namespace App.Automation.Core.Engine
{
    public class WorkflowStep
    {
        public string Name { get; set; } = "";

        public Action Action { get; set; } = () => { };
    }
}
