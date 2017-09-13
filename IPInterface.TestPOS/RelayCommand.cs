using System;
using System.Windows.Input;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private bool canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, bool canExecute = true)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute;
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }
    }
}
