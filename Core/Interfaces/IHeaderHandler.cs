namespace App.Automation.Core.Interfaces
{
    public interface IHeaderHandler<THeader> : IHandler
    {
        void Fill(THeader header);
    }
}
