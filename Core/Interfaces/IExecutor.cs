namespace App.Automation.Core.Interfaces
{
    public interface IExecutor<TDocument>
    {
        void Execute(TDocument document);
    }
}
