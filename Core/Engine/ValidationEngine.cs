using App.Automation.Core.Utilities;

namespace App.Automation.Core.Engine
{
    public class ValidationEngine
    {
        private readonly ReportHelper _report;

        public ValidationEngine(ReportHelper report)
        {
            _report = report;
        }

        public void Execute(IEnumerable<Action> validations)
        {
            foreach (var validation in validations)
            {
                validation();
            }
        }
    }
}
