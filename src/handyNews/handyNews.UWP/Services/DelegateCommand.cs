using System;
using System.Windows.Input;

namespace handyNews.UWP.Services
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _executeAction;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> executeAction)
        {
            if (executeAction == null) throw new ArgumentNullException(nameof(executeAction));
            _executeAction = executeAction;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _executeAction(parameter);
        }
    }
}