using App.Automation.Core.Engine;

namespace App.Automation.Modules.Sales.Invoice.Configuration;

public static class InvoiceWorkflow
{
    /// <summary>
    /// Creates the Sales Invoice workflow.
    ///
    /// Section execution and saving are handled by SectionEngine.
    /// Each section is configured with RequiresSave = true.
    ///
    /// Workflow:
    ///     Fill Sections
    ///     Save
    ///     Validate
    ///     Submit (optional)
    ///     Approve (optional)
    /// </summary>
    public static WorkflowDefinition Create(
        Action fillSections,
        Action view,
        Action validate,
        Action? submit = null,
        Action? approve = null)
    {
        ArgumentNullException.ThrowIfNull(fillSections);
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(validate);

        var workflow = new WorkflowDefinition
        {
            Name = "Sales Invoice Workflow"
        };

        workflow.Steps.Add(new WorkflowStep
        {
            Name = "Fill Sections",
            Action = fillSections
        });

        workflow.Steps.Add(new WorkflowStep
        {
            Name = "View",
            Action = view
        });

        workflow.Steps.Add(new WorkflowStep
        {
            Name = "Validate",
            Action = validate
        });

        if (submit != null)
        {
            workflow.Steps.Add(new WorkflowStep
            {
                Name = "Submit",
                Action = submit
            });
        }

        if (approve != null)
        {
            workflow.Steps.Add(new WorkflowStep
            {
                Name = "Approve",
                Action = approve
            });
        }

        return workflow;
    }
}