using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace OpenUGD.TaskTransactions
{
    public class TaskContext : ITaskContext, ITaskTransactionProvider
    {
        private readonly TaskCompletionSource<TaskResultStatus> _task;
        private readonly List<ITaskTransaction> _actions;
        private readonly Stack<ITaskTransaction> _completed;
        private TaskResultStatus? _status;
        private bool _isUndo;

        public TaskContext(Lifetime lifetime, IInjector injector)
        {
            Lifetime = lifetime;
            Injector = injector;
            _task = new TaskCompletionSource<TaskResultStatus>();
            _actions = new List<ITaskTransaction>();
            _completed = new Stack<ITaskTransaction>();
        }

        public async Task<TaskResultStatus> Add(Func<ITaskTransaction> action)
        {
            if (_status.HasValue)
                throw new InvalidOperationException($"task already completed with status: {_status.Value}");

            var ctx = new TaskContext(Lifetime, Injector);
            var inst = action();
            Injector.Inject(inst);
            _actions.Add(inst);
            inst.Execute(ctx);
            var result = await ctx.Task;
            _actions.Remove(inst);
            _completed.Push(inst);
            if (result == TaskResultStatus.Fail || ctx.Lifetime.IsTerminated)
            {
                _isUndo = true;
            }

            if (_isUndo)
            {
                while (_completed.Count != 0)
                {
                    _completed.Pop().Undo();
                }
            }

            TryFinish();
            return result;
        }

        public Task<TaskResultStatus> Sequence(params Func<ITaskTransaction>[] tasks)
        {
            return Add(() => new TaskSequence(tasks));
        }

        public Task<TaskResultStatus> Parallels(params Func<ITaskTransaction>[] tasks)
        {
            return Add(() => new TaskParallels(tasks));
        }

        public void Success()
        {
            if (_status.HasValue)
                throw new InvalidOperationException($"task already completed with status: {_status.Value}");
            _status = TaskResultStatus.Success;
            TryFinish();
        }

        public void Fail()
        {
            if (_status.HasValue)
                throw new InvalidOperationException($"task already completed with status: {_status.Value}");
            _status = TaskResultStatus.Fail;
            TryFinish();
        }

        public Task<TaskResultStatus> Task => _task.Task;

        private void TryFinish()
        {
            if (_isUndo && _completed.Count == 0 && _actions.Count == 0)
            {
                _status = TaskResultStatus.Fail;
                _task.SetResult(TaskResultStatus.Fail);
                return;
            }

            if (!_status.HasValue) return;
            if (_actions.Count != 0) return;

            _task.SetResult(_status.Value);
            _completed.Clear();
        }

        public Lifetime Lifetime { get; private set; }
        public IInjector Injector { get; private set; }
        object IResolve.Resolve(Type type) => Injector.Resolve(type);
    }
}