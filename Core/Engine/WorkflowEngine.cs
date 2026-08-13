//using App.Automation.Core.Utilities;

//namespace App.Automation.Core.Engine;

//public class WorkflowEngine
//{
//    private readonly ReportHelper _report;

//    public WorkflowEngine(ReportHelper report)
//    {
//        _report = report
//            ?? throw new ArgumentNullException(nameof(report));
//    }

//    public void Execute(WorkflowDefinition workflow)
//    {       

//        ArgumentNullException.ThrowIfNull(workflow);

//        _report.Info(
//            $"Starting Workflow: {workflow.Name}");

//        foreach (var step in workflow.Steps)
//        {
//            try
//            {
//                _report.Info(
//                    $"Executing Workflow Step: {step.Name}");

//                step.Action();

//                _report.Info(
//                    $"Completed Workflow Step: {step.Name}");
//            }
//            catch (Exception ex)
//            {
//                _report.Fail(
//                    $"Workflow Step Failed: {step.Name} | {ex.Message}");

//                throw;
//            }
//        }

//        _report.Info(
//            $"Workflow Completed: {workflow.Name}");
//    }
//}

using App.Automation.Core.Utilities;

namespace App.Automation.Core.Engine;

public class WorkflowEngine
{
    private readonly ReportHelper _report;

    public WorkflowEngine(ReportHelper report)
    {
        _report = report
            ?? throw new ArgumentNullException(nameof(report));
    }

    public void Execute(WorkflowDefinition workflow)
    {
        ArgumentNullException.ThrowIfNull(workflow);

        _report.Info(
            $"Starting Workflow: {workflow.Name}");

        foreach (var step in workflow.Steps)
        {
            if (!step.ShouldRun())
            {
                _report.Info(
                    $"Skipping Workflow Step: {step.Name} | Condition not met");

                continue;
            }

            try
            {
                _report.Info(
                    $"Executing Workflow Step: {step.Name}");

                step.Action();

                if (step.RequiresSave)
                {
                    _report.Info(
                        $"Save required after Workflow Step: {step.Name}");
                }

                step.Validate?.Invoke();

                _report.Info(
                    $"Completed Workflow Step: {step.Name}");
            }
            catch (Exception ex)
            {
                _report.Fail(
                    $"Workflow Step Failed: {step.Name} | {ex.Message}");

                if (step.StopOnFailure)
                    throw;
            }
        }

        _report.Info(
            $"Workflow Completed: {workflow.Name}");
    }
}