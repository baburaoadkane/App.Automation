namespace App.Automation.Core.Engine
{
    public class SectionDefinition<TData>
    {
        public string Name { get; set; } = string.Empty;

        // Condition to run section
        public Func<TData, bool> ShouldRun { get; set; } = _ => true;

        // Action to execute
        public Action<TData> Action { get; set; } = _ => { };

        // Optional validation
        public Action<TData>? Validate { get; set; }

        // Should save after execution?
        public bool RequiresSave { get; set; } = true;
    }
}
