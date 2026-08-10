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
            _sections = sections
                ?? throw new ArgumentNullException(nameof(sections));
            _save = save
                ?? throw new ArgumentNullException(nameof(save));
            _report = report
                ?? throw new ArgumentNullException(nameof(report));
        }

        public void Execute(TData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

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
                    {
                        _report.Info(
                            $"Saving after Section: {section.Name}");

                        _save();
                    }

                    section.Validate?.Invoke(data);

                    _report.Info(
                    $"Section Completed: {section.Name}");
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
