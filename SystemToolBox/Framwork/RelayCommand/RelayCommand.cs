using System;
using System.Windows.Input;

namespace SystemToolBox.Framwork.RelayCommand
{
    public class RelayCommand : BaseCommand, ICommand
    {
        private readonly Action<object> _execute;

        public RelayCommand(Action<object> execute)
        {
            if (null == execute)
                throw new ArgumentNullException("execute");
            _execute = execute;
        }

        public RelayCommand(Action execute)
            : this(obj => execute())
        {
            if (null == execute)
                throw new ArgumentNullException("execute");
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canDo)
            : this(execute)
        {
            if (null == canDo)
                throw new ArgumentNullException("canDo");

            this.CanDo = canDo;
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
            : this(obj => execute(), obj => canExecute())
        {
            if (null == execute)
                throw new ArgumentNullException("execute");
            if (null == canExecute)
                throw new ArgumentNullException("canExecute");
        }

        public void Execute(object parameter)
        {
            try
            {
                IsExecuting = true;
                OnCanExecuteChanged();
                _execute(parameter);
            }
            finally
            {
                IsExecuting = false;
                OnCanExecuteChanged();
            }
        }
    }

    public sealed class RelayCommand<T> : RelayCommand
    {
        public RelayCommand(Action<T> execute)
            : base(obj => execute((T) obj))
        {
            if (null == execute)
                throw new ArgumentNullException("execute");
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
            : base(obj => execute((T) obj), obj => canExecute((T) obj))
        {
            if (null == execute)
                throw new ArgumentNullException("execute");
            if (null == canExecute)
                throw new ArgumentNullException("canExecute");
        }
    }
}