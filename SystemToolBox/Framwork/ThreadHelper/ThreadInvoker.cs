using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SystemToolBox.Framwork.ThreadHelper
{
    internal class ThreadInvoker
    {
        #region Singleton

        private ThreadInvoker()
        {
        }

        public static ThreadInvoker Instance
        {
            get { return Nested.Instance; }
        }

        private class Nested
        {
            internal static readonly ThreadInvoker Instance = new ThreadInvoker();

            // 显式静态构造函数告诉C＃编译器不要将类型标记为beforefieldinit
            static Nested()
            {
            }
        }

        #endregion

        #region New Thread

        private static readonly object Padlock = new object();

        public void RunByNewThread(Action action)
        {
            lock (Padlock)
            {
                action.BeginInvoke(ar => ActionCompleted(ar, action.EndInvoke), null);
            }
        }

        public void RunByNewThread<TResult>(Func<TResult> func, Action<TResult> callbackAction)
        {
            lock (Padlock)
            {
                func.BeginInvoke(ar => FuncCompleted(ar, func.EndInvoke, callbackAction), null);
            }
        }

        private static void ActionCompleted(IAsyncResult asyncResult, Action<IAsyncResult> endInvoke)
        {
            if (asyncResult.IsCompleted)
                endInvoke(asyncResult);
        }

        private static void FuncCompleted<TResult>(IAsyncResult asyncResult, Func<IAsyncResult, TResult> endInvoke,
            Action<TResult> callbackAction)
        {
            if (asyncResult.IsCompleted)
            {
                var response = endInvoke(asyncResult);
                if (null != callbackAction)
                    callbackAction(response);
            }
        }

        #endregion

        #region UI Thread

        private Dispatcher _dispatcher;

        // 你必须在每个UI线程中初始化一次Dsipatcher，如果只有一个Dispatcher的话，每个程序只需要初始化一次
        public void InitDispatcher(Dispatcher dispatcher = null)
        {
            _dispatcher = dispatcher ?? new UserControl().Dispatcher;
        }

        public void RunByUiThread(Action action)
        {
            #region UI Thread Safety

            if (_dispatcher.Thread != Thread.CurrentThread)
            {
                _dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
                return;
            }

            action();

            #endregion
        }

        public T RunByUiThread<T>(Func<T> function)
        {
            #region UI Thread Safety

            if (_dispatcher.Thread != Thread.CurrentThread)
                return (T) _dispatcher.Invoke(DispatcherPriority.Normal, function);

            return function();

            #endregion
        }

        #endregion
    }
}