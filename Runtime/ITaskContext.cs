using System;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace OpenUGD.TaskTransactions
{
    public interface ITaskContext : IResolve
    {
        void Success();
        void Fail();
        Task<TaskResultStatus> Sequence(params Func<ITaskTransaction>[] tasks);
        Task<TaskResultStatus> Parallels(params Func<ITaskTransaction>[] tasks);
        Lifetime Lifetime { get; }
        Task<TaskResultStatus> Task { get; }
    }
}