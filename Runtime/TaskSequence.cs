using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace OpenUGD.TaskTransactions
{
    public class TaskSequence : ITaskTransaction
    {
        private readonly Queue<Func<ITaskTransaction>> _actions;
        private readonly Stack<ITaskTransaction> _completed;
        private bool _isUndo;

        public TaskSequence(params Func<ITaskTransaction>[] actions)
        {
            _completed = new Stack<ITaskTransaction>();
            _actions = new Queue<Func<ITaskTransaction>>(actions);
        }

        async void ITaskTransaction.Execute(ITaskContext context)
        {
            var hasFail = false;
            while (_actions.Count != 0)
            {
                var provider = (ITaskTransactionProvider)context;
                var action = _actions.Dequeue();
                var ctx = new TaskContext(context.Lifetime, provider.Injector);
                var inst = action();
                provider.Injector.Inject(inst);
                inst.Execute(ctx);
                var status = await ctx.Task;
                if (context.Lifetime.IsTerminated) return;
                _completed.Push(inst);
                if (status == TaskResultStatus.Fail || _isUndo || ctx.Lifetime.IsTerminated)
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