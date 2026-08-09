using App.Automation.Core.Utilities;

namespace App.Automation.Core.Engine
{
    public class SectionEngine<TData>
    {
        private readonly List<SectionDefinition<TData>> _sections;
        private readonly Action _save;
        private readonly ReportHelper _report;

        public SectionEngine(
            List<SectionDefinition<TData>> sections,
            Action save,
            ReportHelper report)
        {
            _sections = sections;
            _save = save;
            _report = report;
        }

        public void Execute(TData data)
        {
            foreach (var section in _sections)
            {
                try
                {
                    if (!section.ShouldRun(data))
                    {
                        _report.Info($"Skipping Section: {section.Name} | Condition not met");
                        continue;
                    }

                    _report.Info($"Executing Section: {section.Name}");

                    section.Action(data);

                    if (section.RequiresSave)
                        _save();

                    section.Validate?.Invoke(data);
                }
                catch (Exception ex)
                {
                    _report.Fail($"Section Failed: {section.Name} | {ex.Message}");
                    throw;
                }
            }
        }
    }
}
