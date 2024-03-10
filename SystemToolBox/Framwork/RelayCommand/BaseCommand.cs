using System;

namespace SystemToolBox.Framwork.RelayCommand
{
    public abstract class BaseCommand
    {
        protected Func<object, bool> CanDo;
        protected bool IsExecuting;

        // 事件声明，因此不需要事件委托
        // 基类事件声明
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (null == CanDo)
                return !IsExecuting;
            return !IsExecuting && CanDo(parameter);
        }

        // 实例化事件
        protected void OnCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (null != handler)
                handler(this, EventArgs.Empty);
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }
    }
}