using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SystemToolBox.Framwork;

namespace SystemToolBox.ViewModel
{
    public class ProcessCtrolViewModel : BaseNotifyPropertyChanged
    {
        #region 构造函数

        private ProcessCtrolViewModel() { }

        public ProcessCtrolViewModel(ref ObservableCollection<ProcessCtrolViewModel> targetCollection)
        {
            var aProcess = Process.GetProcesses();
            foreach (var p in aProcess)
            {
                try
                {
                    targetCollection.Add(new ProcessCtrolViewModel
                    {

                        IconEx = (p.Id == 4 || p.Id == 0 || p.ProcessName == "audiodg") ? null : ChangeIconToImageSource(Icon.ExtractAssociatedIcon(p.MainModule.FileName)),
                        ProcessName = p.ProcessName,
                        ProcessId = p.Id,
                        _processPriority = p.BasePriority,
                        ProcessPath = (p.Id == 4 || p.Id == 0 || p.ProcessName == "audiodg") ? string.Empty : p.MainModule.FileName
                    });
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine($"[{p.ProcessName}]" + ex.Message);
                }
            }
        }

        #endregion

        #region 成员

        /// <summary>
        /// 进程图标
        /// </summary>
        private ImageSource _iconEx;
        public ImageSource IconEx
        {
            get { return _iconEx; }
            set { _iconEx = value; FirePropertyChanged(() => IconEx); }
        }

        /// <summary>
        /// 进程名
        /// </summary>
        private string _processName;
        public string ProcessName
        {
            get { return _processName; }
            set { _processName = value; FirePropertyChanged(() => ProcessName); }
        }

        /// <summary>
        /// 进程ID
        /// </summary>
        private int _processId;
        public int ProcessId
        {
            get { return _processId; }
            set { _processId = value; FirePropertyChanged(() => ProcessId); }
        }

        /// <summary>
        /// 进程优先级
        /// </summary>
        private int _processPriority;
        public int ProcessPriority
        {
            get { return _processPriority; }
            set { _processPriority = value; FirePropertyChanged(() => ProcessPriority); }
        }

        /// <summary>
        /// 进程路径
        /// </summary>
        private string _processPath;
        public string ProcessPath
        {
            get { return _processPath; }
            set { _processPath = value; FirePropertyChanged(() => ProcessPath); }
        }

        #endregion

        #region 实用方法

        /// <summary>
        /// 从Icon到ImageSource的转换
        /// </summary>
        public static ImageSource ChangeIconToImageSource(Icon icon)
        {

            ImageSource imageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            return imageSource;
        }

        #endregion
    }
}
