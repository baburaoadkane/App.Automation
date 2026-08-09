namespace App.Automation.Core.Interfaces
{
    public interface ILineHandler<TLine> : IHandler
    {
        void Fill(IEnumerable<TLine> lines);
    }
}
