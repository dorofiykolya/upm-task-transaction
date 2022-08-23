using System;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace OpenUGD.TaskTransactions
{
    public interface ITaskTransactionExecutor
    {
        Task<TaskResultStatus> Execute(Func<ITaskTransaction> tasks);
        Task<TaskResultStatus> Execute(IInjector injector, Func<ITaskTransaction> tasks);
        Task<TaskResultStatus> Execute(Lifetime lifetime, Func<ITaskTransaction> tasks);
        Task<TaskResultStatus> Execute(Lifetime lifetime, IInjector injector, Func<ITaskTransaction> tasks);

        Task<TaskResultStatus> Parallels(IInjector injector, params Func<ITaskTransaction>[] tasks);
        Task<TaskResultStatus> Parallels(Lifetime lifetime, params Func<ITaskTransaction>[] tasks);
        Task<TaskResultStatus> Parallels(Lifetime lifetime, IInjector injector, params Func<ITaskTransaction>[] tasks);
        Task<TaskResultStatus> Parallels(params Func<ITaskTransaction>[] tasks);

        Task<TaskResultStatus> Sequence(IInjector injector, params Func<ITaskTransaction>[] tasks);
        Task<TaskResultStatus> Sequence(Lifetime lifetime, params Func<ITaskTransaction>[] tasks);
        Task<TaskResultStatus> Sequence(Lifetime lifetime, IInjector injector, params Func<ITaskTransaction>[] tasks);
        Task<TaskResultStatus> Sequence(params Func<ITaskTransaction>[] tasks);
    }
}