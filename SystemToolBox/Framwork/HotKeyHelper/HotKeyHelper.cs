using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Interop;
using System.Windows.Input;
using System.Runtime.InteropServices;

namespace SystemToolBox.Framwork.HotKeyHelper
{
    // ******************************************************************
    // This class is from: https://stackoverflow.com/questions/48935/
    // Usage:
    //  var _hotKey = new HotKey(Key.F9, KeyModifier.Shift | KeyModifier.Win, OnHotKeyHandler);
    //  private void OnHotKeyHandler(HotKey hotKey) { do something... }
    // ******************************************************************

    public class HotKeyHelper : IDisposable
    {
        private static Dictionary<int, HotKeyHelper> _dictHotKeyToCalBackProc;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const int WmHotKey = 0x0312;

        private bool _isDisposed;

        public Key Key { get; private set; }
        public KeyModifier KeyModifiers { get; private set; }
        public Action<HotKeyHelper> Action { get; private set; }
        public int Id { get; private set; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyModifier"></param>
        /// <param name="action"></param>
        /// <param name="isRegister"></param>
        public HotKeyHelper(Key key, KeyModifier keyModifier, Action<HotKeyHelper> action, bool isRegister = true)
        {
            Key = key;
            KeyModifiers = keyModifier;
            Action = action;
            if (isRegister)
            {
                Register();
            }
        }

        /// <summary>
        /// Register hotkey
        /// </summary>
        /// <returns></returns>
        public bool Register()
        {
            int virtualKeyCode = KeyInterop.VirtualKeyFromKey(Key);
            Id = virtualKeyCode + ((int)KeyModifiers * 0x10000);
            bool result = RegisterHotKey(IntPtr.Zero, Id, (uint)KeyModifiers, (uint)virtualKeyCode);

            if (null == _dictHotKeyToCalBackProc)
            {
                _dictHotKeyToCalBackProc = new Dictionary<int, HotKeyHelper>();
                ComponentDispatcher.ThreadFilterMessage += new ThreadMessageEventHandler(ComponentDispatcher_ThreadFilterMessage);
            }

            _dictHotKeyToCalBackProc.Add(Id, this);

            Debug.Print(result + ", " + Id + ", " + virtualKeyCode);

            return result;
        }

        /// <summary>
        /// UnRegister hotkey
        /// </summary>
        public void UnRegister()
        {
            HotKeyHelper hotKey;
            if (_dictHotKeyToCalBackProc.TryGetValue(Id,out hotKey))
            {
                UnregisterHotKey(IntPtr.Zero, Id);
            }
        }

        /// <summary>
        /// Keyboard message filtering and processing
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="handled"></param>
        private static void ComponentDispatcher_ThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (!handled)
            {
                if (WmHotKey == msg.message)
                {
                    HotKeyHelper hotKey;

                    if (_dictHotKeyToCalBackProc.TryGetValue((int)msg.wParam,out hotKey))
                    {
                        if (null != hotKey.Action)
                        {
                            hotKey.Action.Invoke(hotKey);
                        }
                        handled = true;
                    }
                }
            }
        }


        // ******************************************************************
        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // ******************************************************************
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be _disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be _disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    UnRegister();
                }

                // Note disposing has been done.
                _isDisposed = true;
            }
        }

    }

    [Flags]
    public enum KeyModifier
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        NoRepeat = 0x4000,
        Shift = 0x0004,
        Win = 0x0008
    }
}
