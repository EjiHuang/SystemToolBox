using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using SystemToolBox.Framwork;
using SystemToolBox.Framwork.ThreadHelper;
using MahApps.Metro.Controls.Dialogs;
using SystemToolBox.Native;
using SystemToolBox.Native.Framwork;

namespace SystemToolBox.ViewModel
{
    public class BasicCtrolViewModel : BaseNotifyPropertyChanged
    {
        private readonly SysToolBoxMain _oTargetAcceptor;

        #region 构造函数

        public BasicCtrolViewModel() { }

        public BasicCtrolViewModel(object oTargetAcceptor)
        {
            // 初始化功能字符串
            _features = new List<string>
            {
                "关机","重启","注销"
            };

            _currFeature = _features[0];
            _executeTime = DateTime.Now;
            _isCancelExecute = true;
            _countdown = string.Empty;
            _executeFileName = "指定执行文件";
            _isExecuteExe = false;

            _oTargetAcceptor = ((SysToolBoxMain)oTargetAcceptor);
        }

        #endregion

        #region 成员

        /// <summary>
        /// 功能列表
        /// </summary>
        private List<string> _features;
        public List<string> Features
        {
            get { return _features; }
            set { _features = value; FirePropertyChanged(() => Features); }
        }

        /// <summary>
        /// 当前功能
        /// </summary>
        private string _currFeature;
        public string CurrFeature
        {
            get { return _currFeature; }
            set { _currFeature = value; FirePropertyChanged(() => CurrFeature); }
        }

        /// <summary>
        /// 执行时间
        /// </summary>
        private DateTime _executeTime;
        public DateTime ExecuteTime
        {
            get { return _executeTime; }
            set { _executeTime = value; FirePropertyChanged(() => ExecuteTime); }
        }

        /// <summary>
        /// 是否取消执行
        /// </summary>
        private bool _isCancelExecute;
        public bool IsCancelExecute
        {
            get { return _isCancelExecute; }
            set { _isCancelExecute = value; FirePropertyChanged(() => IsCancelExecute); }
        }

        /// <summary>
        /// 时间差
        /// </summary>
        private string _countdown;
        public string Countdown
        {
            get { return _countdown; }
            set { _countdown = value; FirePropertyChanged(() => Countdown); }
        }

        /// <summary>
        /// 执行的文件名
        /// </summary>
        private string _executeFileName;
        public string ExecuteFileName
        {
            get { return _executeFileName; }
            set { _executeFileName = value; FirePropertyChanged(() => ExecuteFileName); }
        }

        /// <summary>
        /// 执行文件的全路径
        /// </summary>
        private string _executeFileFullPath;
        public string ExecuteFileFullPath
        {
            get { return _executeFileFullPath; }
            set { _executeFileFullPath = value; FirePropertyChanged(() => ExecuteFileFullPath); }
        }

        /// <summary>
        /// 执行类型
        /// </summary>
        private bool _isExecuteExe;
        public bool IsExecuteExe
        {
            get { return _isExecuteExe; }
            set { _isExecuteExe = value; FirePropertyChanged(() => IsExecuteExe); }
        }

        #endregion

        #region 实现方法

        /// <summary>
        /// 判断是否到达执行时间与显示时间差
        /// </summary>
        /// <returns></returns>
        private bool IsTimeToExecute()
        {
            // 时间差
            while (true)
            {
                if (DateTime.Now >= ExecuteTime)
                    break;
                // 取消执行则退出定时器
                if (IsCancelExecute)
                    return false;

                // 计时剩余时间并且显示
                var interval = ExecuteTime - DateTime.Now;
                Countdown = string.Format("{0}天{1}小时{2}分钟{3}秒", interval.Days,
                    interval.Hours, interval.Minutes, interval.Seconds);

                Thread.Sleep(500);
            }
            return true;
        }

        /// <summary>
        /// 执行前初始化
        /// </summary>
        private bool InitExecute()
        {
            if (DateTime.Now > ExecuteTime)
            {
                _oTargetAcceptor.ShowMessageAsync("警告", "当前时间大于执行时间！请重新设置执行时间",
                    MessageDialogStyle.Affirmative, new MetroDialogSettings { MaximumBodyHeight = 40, ColorScheme = MetroDialogColorScheme.Accented });
                return false;
            }

            // 判断是否为定时执行文件操作
            _oTargetAcceptor.basicCtrol_CheckBox4IsExeExecute.IsEnabled = IsExecuteExe;


            // 启用取消执行控件
            _oTargetAcceptor.basicCtrol_Button4CancelExecute.IsEnabled = true;
            // 禁用干扰控件
            _oTargetAcceptor.basicCtrol_Button4Execute.IsEnabled = false;
            _oTargetAcceptor.basicCtrol_TextBox4Seconds.IsEnabled = false;
            _oTargetAcceptor.basicCtrol_SplitButton.IsEnabled = false;
            _oTargetAcceptor.basicCtrol_DateTimePicker.IsEnabled = false;

            _oTargetAcceptor.basicCtrol_TextBlock_ExecuteTimeShow.Text = ExecuteTime.ToString(CultureInfo.InvariantCulture);
            // 取消执行标识
            IsCancelExecute = false;

            return true;
        }

        #endregion

        #region 命令

        /// <summary>
        /// 执行命令
        /// </summary>
        public void ExecuteCommand()
        {
            // 获取权限
            Privileges.EnablePrivilege(NativeConstants.SecurityEntity.SE_SHUTDOWN_NAME);

            switch (CurrFeature)
            {
                case "关机":
                    if (InitExecute())
                    {
                        ThreadInvoker.Instance.RunByNewThread(IsTimeToExecute, (b) =>
                        {
                            if (b)
                            {
                                Countdown = string.Format("计算机即将{0}!", CurrFeature);
                                NativeMethods.ExitWindowsEx(NativeConstants.ExitWindowsAction.EWX_SHUTDOWN, 0);
                            }
                        });
                    }
                    break;
                case "重启":
                    if (InitExecute())
                    {
                        ThreadInvoker.Instance.RunByNewThread(IsTimeToExecute, (b) =>
                        {
                            if (b)
                            {
                                Countdown = string.Format("计算机即将{0}!", CurrFeature);
                                NativeMethods.ExitWindowsEx(NativeConstants.ExitWindowsAction.EWX_REBOOT, 0);
                            }
                        });
                    }
                    break;
                case "注销":
                    if (InitExecute())
                    {
                        ThreadInvoker.Instance.RunByNewThread(IsTimeToExecute, (b) =>
                        {
                            if (b)
                            {
                                Countdown = string.Format("计算机即将{0}!", CurrFeature);
                                NativeMethods.ExitWindowsEx(NativeConstants.ExitWindowsAction.EWX_LOGOFF, 0);
                            }
                        });
                    }
                    break;
                case "定时执行程序":
                    if (InitExecute())
                    {
                        if (ExecuteFileName != "指定执行文件" && File.Exists(ExecuteFileFullPath))
                        {
                            ThreadInvoker.Instance.RunByNewThread(IsTimeToExecute, (b) =>
                            {
                                if (b)
                                {
                                    System.Diagnostics.Process.Start(ExecuteFileFullPath);
                                    Countdown = string.Format("即将执行{0}", ExecuteFileName);
                                    // 更新UI状态
                                    ThreadInvoker.Instance.InitDispatcher(_oTargetAcceptor.Dispatcher);
                                    ThreadInvoker.Instance.RunByUiThread(CancelExecuteCommand);
                                }
                            });
                        }
                        else
                        {
                            CancelExecuteCommand();
                            OpenFileDialogCommand();
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 取消执行命令
        /// </summary>
        public void CancelExecuteCommand()
        {
            IsCancelExecute = true;

            _oTargetAcceptor.basicCtrol_Button4Execute.IsEnabled = true;
            _oTargetAcceptor.basicCtrol_TextBox4Seconds.IsEnabled = true;
            _oTargetAcceptor.basicCtrol_SplitButton.IsEnabled = true;
            _oTargetAcceptor.basicCtrol_DateTimePicker.IsEnabled = true;
            _oTargetAcceptor.basicCtrol_Button4CancelExecute.IsEnabled = false;
            // 判断是否为定时执行文件操作
            if (!IsExecuteExe)
                _oTargetAcceptor.basicCtrol_CheckBox4IsExeExecute.IsEnabled = true;
            else
            {
                _oTargetAcceptor.basicCtrol_CheckBox4IsExeExecute.IsEnabled = true;
            }
        }

        /// <summary>
        /// 打开文件对话框命令
        /// </summary>
        public void OpenFileDialogCommand()
        {
            // 定时器开始时，不允许重新选择可执行文件
            if (_oTargetAcceptor.basicCtrol_Button4CancelExecute.IsEnabled) return;

            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "Execute file",
                DefaultExt = ".exe",
                Filter = "Execute files (.exe)|*.exe"
            };

            var result = dlg.ShowDialog();
            if (result == null || !result.Value) return;

            ExecuteFileName = dlg.SafeFileName;
            ExecuteFileFullPath = dlg.FileName;
        }

        #endregion
    }
}
