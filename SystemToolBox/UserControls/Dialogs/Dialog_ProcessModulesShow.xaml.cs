using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SystemToolBox.ViewModel.DialogsViewModel;

namespace SystemToolBox.UserControls.Dialogs
{
    /// <summary>
    ///     Dialog_ProcessModulesShow.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_ProcessModulesShow
    {
        private ProcessModulesViewModel _processModulesVm;

        public Dialog_ProcessModulesShow()
        {
            InitializeComponent();
        }

        public Dialog_ProcessModulesShow(ProcessModuleCollection modules)
            : this()
        {
            var modulesCollection = new ObservableCollection<ProcessModulesViewModel>();
            _processModulesVm = new ProcessModulesViewModel(modules, ref modulesCollection);
            modulesCtrol_ListView.ItemsSource = modulesCollection;
        }

        /// <summary>
        ///     点击表头实现排序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modulesCtrol_ListView_Click(object sender, RoutedEventArgs e)
        {
            var header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                // 获取单击的列
                var clickedColumn = header.Column;
                if (clickedColumn != null)
                {
                    if (clickedColumn.DisplayMemberBinding != null)
                    {
                        var szBindingProperty = ((Binding) clickedColumn.DisplayMemberBinding).Path.Path;
                        if (null != clickedColumn.Header)
                        {
                            // 得到单击列所绑定的属性

                            var sdc = modulesCtrol_ListView.Items.SortDescriptions;
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
        }
    }
}