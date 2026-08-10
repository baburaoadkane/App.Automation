using App.Automation.Core.Base;
using App.Automation.Core.Enums;
using App.Automation.Core.Interfaces;
using App.Automation.Core.Factories;

namespace App.Automation.Tests.Common;

public abstract class BaseTransactionTest<TDocument> : BaseTest
{
    protected IExecutor<TDocument> Executor { get; private set; } = null!;

    protected abstract ModuleType Module { get; }

    protected abstract TransactionType Transaction { get; }

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();

        Executor = ExecutorFactory.Create<TDocument>(
            Module,
            Transaction,
            Driver,
            Wait,
            Report);
    }
}