using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Survoicerium.Client.Common
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> commandAction;
        private readonly Predicate<object> canExecuteAction;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            commandAction = execute;
            canExecuteAction = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return canExecuteAction == null || canExecuteAction(parameter);
        }

        public virtual void Execute(object parameter)
        {
            commandAction(parameter);
        }
    }
}
