using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace OpenUGD.TaskTransactions
{
    public class TaskParallels : ITaskTransaction
    {
        private readonly Queue<Func<ITaskTransaction>> _actions;
        private readonly Stack<ITaskTransaction> _completed;
        private bool _isUndo;

        public TaskParallels(params Func<ITaskTransaction>[] actions)
        {
            _completed = new Stack<ITaskTransaction>();
            _actions = new Queue<Func<ITaskTransaction>>(actions);
        }

        async void ITaskTransaction.Execute(ITaskContext context)
        {
            var hasFail = false;
            var tasks = new List<Task<TaskResultStatus>>();
            while (_actions.Count != 0)
            {
                var provider = (ITaskTransactionProvider)context;
                var action = _actions.Dequeue();
                var ctx = new TaskContext(context.Lifetime, provider.Injector);
                var inst = action();
                provider.Injector.Inject(inst);
                tasks.Add(ctx.Task);
                _completed.Push(inst);
                inst.Execute(ctx);
            }

            await Task.WhenAll(tasks);
            if (context.Lifetime.IsTerminated) return;

            foreach (var task in tasks)
            {
                if (task.Result == TaskResultStatus.Fail || _isUndo)
                {
                    hasFail = true;
                    break;
                }
            }

            if (hasFail)
            {
                while (_completed.Count != 0)
                {
                    var inst = _completed.Pop();
                    inst.Undo();
                }

                context.Fail();
            }
            else
            {
                context.Success();
                _completed.Clear();
            }
        }

        public void Undo()
        {
            _isUndo = true;
        }
    }
}