using System;

namespace SystemToolBox.Framwork.RelayCommand
{
    public interface ICommandEx
    {
        bool CanExecute(object parameter);
        void Execute(object parameter);

        event EventHandler CanExecuteChanged;
    }
}