namespace App.Automation.Core.Engine;

public class SectionDefinition<TData>
{
    /// <summary>
    /// Name displayed in reports/logs.
    /// Example: Lines, Discount, Charges, Payments, Others.
    /// </summary>
    public string Name { get; set; } = string.Empty;


    /// <summary>
    /// Determines whether this section should be executed.
    /// </summary>
    public Func<TData, bool> ShouldRun { get; set; } = _ => true;


    /// <summary>
    /// Action executed when the section is enabled.
    /// </summary>
    public Action<TData> Action { get; set; } = _ => { };


    /// <summary>
    /// Optional validation executed after the section action.
    /// </summary>
    public Action<TData>? Validate { get; set; }


    /// <summary>
    /// Determines whether the document should be saved
    /// after this section is executed.
    /// </summary>
    public bool RequiresSave { get; set; } = true;
}