using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SystemToolBox.ViewModel.DialogsViewModel;

namespace SystemToolBox.UserControls.Dialogs
{
    /// <summary>
    ///     Dialog_IpCtrolTCP.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_IpCtrolTCP
    {
        private ObservableCollection<IpCtrolViewModel> _ipPortCollection;
        private IpCtrolViewModel _ipCtrol;

        public Dialog_IpCtrolTCP()
        {
            InitializeComponent();
        }

        public Dialog_IpCtrolTCP(ObservableCollection<IpCtrolViewModel> targetCollection, bool isUDP) : this()
        {
            _ipPortCollection = new ObservableCollection<IpCtrolViewModel>();
            _ipPortCollection = targetCollection;
            ipCtrol_ListView.ItemsSource = _ipPortCollection;
            _ipCtrol = new IpCtrolViewModel(ref targetCollection, isUDP);
        }

        /// <summary>
        ///     端口地址-点击表头实现排序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ipCtrol_ListView_ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader)
            {
                // 获取单击的列
                var clickedColumn = (e.OriginalSource as GridViewColumnHeader).Column;
                if (clickedColumn != null)
                    if (clickedColumn.DisplayMemberBinding != null)
                    {
                        var szBindingProperty = ((Binding)clickedColumn.DisplayMemberBinding).Path.Path;
                        if (null != clickedColumn.Header && "本地地址" != (string)clickedColumn.Header &&
                            "目标地址" != (string)clickedColumn.Header)
                        {
                            // 得到单击列所绑定的属性

                            var sdc = ipCtrol_ListView.Items.SortDescriptions;
                            var listSortDirection = ListSortDirection.Ascending;
                            if (0 < sdc.Count)
                            {
                                var sd = sdc[0];
                                // 判断此列当前的排序方式：升序0，倒序1,然后取反进行排序
                                listSortDirection = (ListSortDirection)(((int)sd.Direction + 1) % 2);
                                sdc.Clear();
                            }
                            sdc.Add(new SortDescription(szBindingProperty, listSortDirection));
                        }
                    }
            }
        }

        /// <summary>
        ///     端口地址-右键菜单点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ipCtrol_ListView_ContextMenu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is MenuItem))
                return;

            switch ((e.Source as MenuItem).Header.ToString())
            {
                case "刷新":
                    _ipPortCollection = new ObservableCollection<IpCtrolViewModel>();
                    ipCtrol_ListView.ItemsSource = _ipPortCollection;

                    _ipCtrol = new IpCtrolViewModel(ref _ipPortCollection, false);
                    break;
            }
        }
    }
}