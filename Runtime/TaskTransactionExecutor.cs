using System;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace OpenUGD.TaskTransactions
{
    public class TaskTransactionExecutor : ITaskTransactionExecutor
    {
        private readonly Lifetime _lifetime;
        private readonly IInjector _injector;

        public TaskTransactionExecutor(Lifetime lifetime, IInjector injector)
        {
            _lifetime = lifetime;
            _injector = injector;
        }

        public Task<TaskResultStatus> Execute(Func<ITaskTransaction> action)
        {
            return Execute(_lifetime, _injector, action);
        }

        public Task<TaskResultStatus> Execute(IInjector injector, Func<ITaskTransaction> action)
        {
            return Execute(_lifetime, injector, action);
        }

        public Task<TaskResultStatus> Execute(Lifetime lifetime, Func<ITaskTransaction> action)
        {
            return Execute(lifetime, _injector, action);
        }

        public Task<TaskResultStatus> Execute(Lifetime lifetime, IInjector injector, Func<ITaskTransaction> action)
        {
            var context = new TaskContext(lifetime, injector);
            var inst = action();
            _injector.Inject(inst);
            inst.Execute(context);
            return context.Task;
        }

        public Task<TaskResultStatus> Parallels(IInjector injector, params Func<ITaskTransaction>[] tasks)
        {
            return Parallels(_lifetime, injector, tasks);
        }

        public Task<TaskResultStatus> Parallels(Lifetime lifetime, params Func<ITaskTransaction>[] tasks)
        {
            return Parallels(lifetime, _injector, tasks);
        }

        public Task<TaskResultStatus> Parallels(Lifetime lifetime, IInjector injector,
            params Func<ITaskTransaction>[] tasks)
        {
            var context = new TaskContext(lifetime, injector);

            foreach (var task in tasks)
            {
#pragma warning disable CS4014
                context.Add(task);
#pragma warning restore CS4014
            }

            context.Success();

            return context.Task;
        }

        public Task<TaskResultStatus> Parallels(params Func<ITaskTransaction>[] tasks)
        {
            return Parallels(_lifetime, _injector, tasks);
        }

        public Task<TaskResultStatus> Sequence(IInjector injector, params Func<ITaskTransaction>[] tasks)
        {
            return Sequence(_lifetime, injector, tasks);
        }

        public Task<TaskResultStatus> Sequence(Lifetime lifetime, params Func<ITaskTransaction>[] tasks)
        {
            return Sequence(lifetime, _injector, tasks);
        }

        public Task<TaskResultStatus> Sequence(Lifetime lifetime, IInjector injector,
            params Func<ITaskTransaction>[] tasks)
        {
            return Execute(lifetime, injector, () => new TaskSequence(tasks));
        }

        public Task<TaskResultStatus> Sequence(params Func<ITaskTransaction>[] tasks)
        {
            return Sequence(_lifetime, _injector, tasks);
        }
    }
}