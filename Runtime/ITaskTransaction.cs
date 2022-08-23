using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace OpenUGD.TaskTransactions
{
    public interface ITaskTransaction
    {
        void Execute(ITaskContext context);
        void Undo();
    }
}