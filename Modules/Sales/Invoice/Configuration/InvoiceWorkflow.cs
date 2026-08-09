using App.Automation.Core.Engine;

namespace App.Automation.Modules.Sales.Invoice.Configuration;

public static class InvoiceWorkflow
{
    public static WorkflowDefinition Create(
        Action fillSections,
        Action save,
        Action validate,
        Action? submit = null,
        Action? approve = null)
    {
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
            Name = "Save",
            Action = save
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