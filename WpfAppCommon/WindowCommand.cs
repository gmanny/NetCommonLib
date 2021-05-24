using System;
using System.Windows.Input;

namespace WpfAppCommon
{
    public class WindowCommand : ICommand
    {
        private readonly Action executeDelegate;
        
        public WindowCommand(Action executeDelegate)
        {
            this.executeDelegate = executeDelegate;
        }

        public bool CanExecute(object parameter) => true;

#pragma warning disable 67
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67

        public void Execute(object parameter) => executeDelegate();
    }
}