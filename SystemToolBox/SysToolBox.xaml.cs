using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SystemToolBox.Framwork.ThreadHelper;
using SystemToolBox.UserControls.Dialogs;
using SystemToolBox.ViewModel;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace SystemToolBox
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SysToolBoxMain
    {
        private ObservableCollection<ProcessCtrolViewModel> _processCollection;

        // 总上下文
        private TotalDataContext _totalDataContext;

        // 构造函数
        public SysToolBoxMain()
        {
            InitializeComponent();
        }

        #region 控件事件处理

        /// <summary>
        ///     窗口加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Common.CommonFunc.IsAdministrator())
                this.ShowMessageAsync("提醒", "本程序由于涉及权限操作，请在管理员权限下运行。",
                    MessageDialogStyle.Affirmative,
                    new MetroDialogSettings {MaximumBodyHeight = 40, ColorScheme = MetroDialogColorScheme.Accented});
            // 程序加载后禁用取消执行按钮
            basicCtrol_Button4CancelExecute.IsEnabled = false;
        }

        /// <summary>
        ///     TabControl的TabItem选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MetroTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is MetroTabControl)
                switch (((MetroTabItem) e.AddedItems[0]).Header.ToString())
                {
                    case "基本操作":
                        if (!(SystemTool_TabControl.DataContext is TotalDataContext))
                        {
                            // 总上下文包含各种子上下文
                            _totalDataContext = new TotalDataContext
                            {
                                BasicCtrolViewModel = new BasicCtrolViewModel(this),
                                CaptureScreenViewModel = new CaptureScreenViewModel(this),
                                NetwordToolViewModel = new NetwordToolViewModel()
                            };
                            // 设置总上下文
                            SystemTool_TabControl.DataContext = _totalDataContext;
                        }
                        break;
                    case "进程操作":
                        _processCollection = new ObservableCollection<ProcessCtrolViewModel>();
                        processCtrol_ListView.ItemsSource = _processCollection;

                        //var unused = new ProcessCtrolViewModel(ref _processCollection);
                        break;
                    case "TEST":
                        var win32Bios = new Win32Bios();
                        win32Bios.GetSystemInfo();
                        break;
                }
        }

        #region 基础操作

        /// <summary>
        ///     日期时间更改事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DateTimePicker_SelectedDateChanged(object sender,
            TimePickerBaseSelectionChangedEventArgs<DateTime?> e)
        {
            if (e.NewValue < DateTime.Now)
            {
                this.ShowMessageAsync("警告", "所选时间日期小于当前日期！",
                    MessageDialogStyle.Affirmative,
                    new MetroDialogSettings {MaximumBodyHeight = 25, ColorScheme = MetroDialogColorScheme.Accented});
                ((DateTimePicker) sender).SelectedDate = DateTime.Now;
            }
            else
            {
                if (e.NewValue != null)
                    _totalDataContext.BasicCtrolViewModel.ExecuteTime = e.NewValue.Value;
                basicCtrol_TextBlock_ExecuteTimeShow.Text =
                    _totalDataContext.BasicCtrolViewModel.ExecuteTime.ToString(CultureInfo.InvariantCulture);
            }
        }


        /// <summary>
        ///     重置按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Reset_Click(object sender, RoutedEventArgs e)
        {
            if (_totalDataContext.BasicCtrolViewModel.IsCancelExecute)
            {
                // 初始化执行时间
                _totalDataContext.BasicCtrolViewModel.ExecuteTime = DateTime.Now;
                // 初始化剩余时间
                _totalDataContext.BasicCtrolViewModel.Countdown = string.Empty;
                // 初始化日期选择表
                basicCtrol_DateTimePicker.SelectedDateChanged -= DateTimePicker_SelectedDateChanged;
                basicCtrol_DateTimePicker.SelectedDate = DateTime.Now;
                basicCtrol_DateTimePicker.SelectedDateChanged += DateTimePicker_SelectedDateChanged;
                // 清空秒数输入框
                basicCtrol_TextBox4Seconds.Clear();
                // 文本框显示清除
                basicCtrol_TextBlock_ExecuteTimeShow.Text = string.Empty;
            }
        }

        /// <summary>
        ///     秒数输入框文本改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void basicCtrol_TextBox4Seconds_TextChanged(object sender, TextChangedEventArgs e)
        {
            #region 屏蔽中文输入和非法字符粘贴输入

            var aChange = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(aChange, 0);

            var iOffset = aChange[0].Offset;
            if (0 < aChange[0].AddedLength)
            {
                double num;
                if (!double.TryParse(((TextBox) sender).Text, out num))
                {
                    ((TextBox) sender).Text = ((TextBox) sender).Text.Remove(iOffset, aChange[0].AddedLength);
                    ((TextBox) sender).Select(iOffset, 0);
                }
            }

            #endregion

            if (0 >= ((TextBox) sender).Text.Length)
                return;
            _totalDataContext.BasicCtrolViewModel.ExecuteTime =
                DateTime.Now.AddSeconds(double.Parse(((TextBox) sender).Text));
            basicCtrol_TextBlock_ExecuteTimeShow.Text =
                _totalDataContext.BasicCtrolViewModel.ExecuteTime.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     限制文本框只能输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void basicCtrol_TextBox4Seconds_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.All(c => char.IsDigit(c)))
                e.Handled = false;
            else
                e.Handled = true;
        }

        /// <summary>
        ///     CheckBox单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void basicCtrol_CheckBox4IsExeExecute_Click(object sender, RoutedEventArgs e)
        {
            // 关机和执行程序功能二选一
            var isChecked = ((CheckBox) sender).IsChecked;
            if (isChecked != null)
                basicCtrol_SplitButton.IsEnabled = !isChecked.Value;
            // 
            if (_totalDataContext.BasicCtrolViewModel.IsExecuteExe)
            {
                _totalDataContext.BasicCtrolViewModel.CurrFeature = "定时执行程序";
            }
            else
            {
                basicCtrol_SplitButton.SelectedIndex = 0;
                _totalDataContext.BasicCtrolViewModel.CurrFeature = basicCtrol_SplitButton.SelectedValue.ToString();
            }
        }

        #endregion

        #region 进程操作

        /// <summary>
        ///     进程操作-点击表头实现排序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void processCtrol_ListView_ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader)
            {
                // 获取单击的列
                var clickedColumn = (e.OriginalSource as GridViewColumnHeader).Column;
                if (clickedColumn != null)
                    if (clickedColumn.DisplayMemberBinding != null)
                    {
                        var szBindingProperty = ((Binding) clickedColumn.DisplayMemberBinding).Path.Path;
                        if (null != clickedColumn.Header)
                        {
                            // 得到单击列所绑定的属性

                            var sdc = processCtrol_ListView.Items.SortDescriptions;
                            var listSortDirection = ListSortDirection.Ascending;
                            if (0 < sdc.Count)
                            {
                                var sd = sdc[0];
                                // 判断此列当前的排序方式：升序0，倒序1,然后取反进行排序
                                listSortDirection = (ListSortDirection) (((int) sd.Direction + 1) % 2);
                                sdc.Clear();
                            }
                            sdc.Add(new SortDescription(szBindingProperty, listSortDirection));
                        }
                    }
            }
        }

        /// <summary>
        ///     进程操作-右键菜单单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void processCtrol_ListView_ContextMenu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is MenuItem))
                return;

            var targetProcessId = ((ProcessCtrolViewModel) processCtrol_ListView.SelectedItem).ProcessId;

            switch ((e.Source as MenuItem).Header.ToString())
            {
                case "刷新进程":
                    _processCollection = new ObservableCollection<ProcessCtrolViewModel>();
                    processCtrol_ListView.ItemsSource = _processCollection;
                    var unused = new ProcessCtrolViewModel(ref _processCollection);
                    break;
                case "结束选中进程":
                    var result = MessageBox.Show(
                        string.Format("是否结束进程：{0}",
                            ((ProcessCtrolViewModel) processCtrol_ListView.SelectedItem).ProcessName),
                        "注意", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                        try
                        {
                            Process.GetProcessById(targetProcessId).Kill();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    break;
                case "查看进程模块":
                    try
                    {
                        var modules = Process.GetProcessById(targetProcessId).Modules;

                        ThreadInvoker.Instance.InitDispatcher(Dispatcher);
                        ThreadInvoker.Instance.RunByUiThread(() =>
                        {
                            new Dialog_ProcessModulesShow(modules) {Owner = MainMetroWindow}.ShowDialog();
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error：拒绝访问", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
            }
        }

        #endregion

        #region 截图操作

        /// <summary>
        ///     截图操作-右键菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CaptureScreen_InkCanvas_ContextMenu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is MenuItem))
                return;

            switch ((e.Source as MenuItem).Header.ToString())
            {
                case "复制":
                    var v = CaptureScreenViewModel.SaveAsWriteableBitmap(CaptureScreen_InkCanvas);
                    Clipboard.SetImage(v);
                    break;
                case "插入文本":
                    CaptureScreen_InkCanvas.UseCustomCursor = true;
                    CaptureScreen_InkCanvas.Cursor = Cursors.IBeam;
                    CaptureScreen_InkCanvas.MouseLeftButtonDown += CaptureScreen_InkCanvas_MouseLeftButtonDown;
                    break;
                case "另存为...":
                    CaptureScreenViewModel.SaveImageTo();
                    break;
                case "清除所有墨迹":
                    CaptureScreen_InkCanvas.Strokes.Clear();
                    break;
                case "使用画图工具打开":
                    CaptureScreenViewModel.UseMspaintToOpen();
                    break;
            }
        }

        /// <summary>
        ///     限制文本框只能输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CaptureScreen_BrushThickness_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.All(c => char.IsDigit(c)))
                e.Handled = false;
            else
                e.Handled = true;
        }

        /// <summary>
        ///     笔刷粗细输入框文本改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CaptureScreen_BrushThickness_TextChanged(object sender, TextChangedEventArgs e)
        {
            #region 屏蔽中文输入和非法字符粘贴输入

            var aChange = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(aChange, 0);

            var iOffset = aChange[0].Offset;
            if (0 < aChange[0].AddedLength)
            {
                double num;
                if (!double.TryParse(((TextBox) sender).Text, out num))
                {
                    ((TextBox) sender).Text = ((TextBox) sender).Text.Remove(iOffset, aChange[0].AddedLength);
                    ((TextBox) sender).Select(iOffset, 0);
                }
            }

            #endregion

            _totalDataContext.CaptureScreenViewModel.BrushThickness = int.Parse(((TextBox) sender).Text);

            if (0 < ((TextBox) sender).Text.Length)
                CaptureScreen_InkCanvas.DefaultDrawingAttributes.Width =
                    CaptureScreen_InkCanvas.DefaultDrawingAttributes.Height =
                        _totalDataContext.CaptureScreenViewModel.BrushThickness;
        }

        /// <summary>
        ///     获取鼠标右键当前坐标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CaptureScreen_InkCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _totalDataContext.CaptureScreenViewModel.TextBoxPoint = e.GetPosition(sender as InkCanvas);
        }

        /// <summary>
        ///     鼠标左键单击事件，此处为添加文本框事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CaptureScreen_InkCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _totalDataContext.CaptureScreenViewModel.TextBoxPoint = e.GetPosition(sender as InkCanvas);
            CaptureScreenViewModel.InsertText();

            CaptureScreen_InkCanvas.UseCustomCursor = false;
            CaptureScreen_InkCanvas.Cursor = Cursors.Arrow;
            CaptureScreen_InkCanvas.MouseLeftButtonDown -= CaptureScreen_InkCanvas_MouseLeftButtonDown;
        }

        #endregion

        #endregion
    }
}