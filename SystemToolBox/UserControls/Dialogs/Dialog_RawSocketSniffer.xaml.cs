using System.Net;
using System.Windows;
using System.Windows.Controls;
using SystemToolBox.ViewModel.DialogsViewModel;

namespace SystemToolBox.UserControls.Dialogs
{
    /// <summary>
    /// Dialog_RawSocketSniffer.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_RawSocketSniffer
    {
        private RawSocketSnifferViewModel _rawSocketSniffer;
        
        public Dialog_RawSocketSniffer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// IP地址选择框加载完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_LoadIpAddressList_Loaded(object sender, RoutedEventArgs e)
        {
            var cmb = sender as ComboBox;
            // 构造函数执行，顺便填充comboBox
            _rawSocketSniffer = new RawSocketSnifferViewModel(cmb);
            // 设置数据绑定上下文
            DataContext = _rawSocketSniffer;
        }

        /// <summary>
        /// IP地址选择框选项改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_LoadIpAddressList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmb = sender as ComboBox;
            if (cmb != null) 
                _rawSocketSniffer.SelectedIpAddress = cmb.SelectedItem as IPAddress;
        }   
    }
}
