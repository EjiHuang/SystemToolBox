using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using SystemToolBox.Common;
using SystemToolBox.Framwork;
using SystemToolBox.Framwork.ThreadHelper;
using SystemToolBox.UserControls.Dialogs;
using SystemToolBox.ViewModel.DialogsViewModel;

namespace SystemToolBox.ViewModel
{
    public class NetwordToolViewModel : BaseNotifyPropertyChanged
    {
        #region 构造函数

        public NetwordToolViewModel()
        {
            _isStopedPing = true;
            _delay = "0 ms";
            _targetUrl = "lb-192-30-253-125-iad.github.com";
            _destinationAddress = "请输入PID后获取";
        }

        #endregion

        #region 命令

        /// <summary>
        ///     获取延迟
        /// </summary>
        public void GetDelayCommand(object sender)
        {
            var btn = (Button) sender;

            if (btn.Content.ToString().Equals("开始测试"))
            {
                IsStopedPing = false;
                btn.Content = "停止测试";
                // 开启线程执行Ping
                ThreadInvoker.Instance.RunByNewThread(() =>
                {
                    var pinger = new Ping();
                    while (!IsStopedPing)
                        try
                        {
                            var ret = pinger.Send(_targetUrl);
                            if (ret != null)
                                switch (ret.Status)
                                {
                                    case IPStatus.Success:
                                        Delay = ret.RoundtripTime + " ms";
                                        break;
                                    case IPStatus.TimedOut:
                                        Delay = IPStatus.TimedOut.ToString();
                                        break;
                                    case IPStatus.Unknown:
                                        Delay = IPStatus.Unknown.ToString();
                                        break;
                                    case IPStatus.DestinationNetworkUnreachable:
                                        Delay = IPStatus.DestinationNetworkUnreachable.ToString();
                                        break;
                                    case IPStatus.DestinationHostUnreachable:
                                        Delay = IPStatus.DestinationHostUnreachable.ToString();
                                        break;
                                    case IPStatus.DestinationProtocolUnreachable:
                                        Delay = IPStatus.DestinationProtocolUnreachable.ToString();
                                        break;
                                    case IPStatus.DestinationPortUnreachable:
                                        Delay = IPStatus.DestinationPortUnreachable.ToString();
                                        break;
                                    case IPStatus.NoResources:
                                        Delay = IPStatus.NoResources.ToString();
                                        break;
                                    case IPStatus.BadOption:
                                        Delay = IPStatus.BadOption.ToString();
                                        break;
                                    case IPStatus.HardwareError:
                                        Delay = IPStatus.HardwareError.ToString();
                                        break;
                                    case IPStatus.PacketTooBig:
                                        Delay = IPStatus.PacketTooBig.ToString();
                                        break;
                                    case IPStatus.BadRoute:
                                        Delay = IPStatus.BadRoute.ToString();
                                        break;
                                    case IPStatus.TtlExpired:
                                        Delay = IPStatus.TtlExpired.ToString();
                                        break;
                                    case IPStatus.TtlReassemblyTimeExceeded:
                                        Delay = IPStatus.TtlReassemblyTimeExceeded.ToString();
                                        break;
                                    case IPStatus.ParameterProblem:
                                        Delay = IPStatus.ParameterProblem.ToString();
                                        break;
                                    case IPStatus.SourceQuench:
                                        Delay = IPStatus.SourceQuench.ToString();
                                        break;
                                    case IPStatus.BadDestination:
                                        Delay = IPStatus.BadDestination.ToString();
                                        break;
                                    case IPStatus.DestinationUnreachable:
                                        Delay = IPStatus.DestinationUnreachable.ToString();
                                        break;
                                    case IPStatus.TimeExceeded:
                                        Delay = IPStatus.TimeExceeded.ToString();
                                        break;
                                    case IPStatus.BadHeader:
                                        Delay = IPStatus.BadHeader.ToString();
                                        break;
                                    case IPStatus.UnrecognizedNextHeader:
                                        Delay = IPStatus.UnrecognizedNextHeader.ToString();
                                        break;
                                    case IPStatus.IcmpError:
                                        Delay = IPStatus.IcmpError.ToString();
                                        break;
                                    case IPStatus.DestinationScopeMismatch:
                                        Delay = IPStatus.DestinationScopeMismatch.ToString();
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }              

                            Thread.Sleep(1000);
                        }
                        catch
                        {
                            CommonFunc.CheckError();
                            break;
                        }
                });
            }
            else if (btn.Content.ToString().Equals("停止测试"))
            {
                IsStopedPing = true;
                btn.Content = "开始测试";
            }
        }

        /// <summary>
        ///     显示端口信息(tcp)
        /// </summary>
        /// <param name="sender"></param>
        public void ShowPortTcpInfoCommand(object sender)
        {
            _ipPortCollection = new ObservableCollection<IpCtrolViewModel>();
            var unused = new IpCtrolViewModel(ref _ipPortCollection, false);

            ThreadInvoker.Instance.InitDispatcher(((Window) sender).Dispatcher);
            ThreadInvoker.Instance.RunByUiThread(() =>
            {
                new Dialog_IpCtrolTCP(_ipPortCollection, false) {Owner = (Window) sender}.ShowDialog();
            });
        }

        /// <summary>
        ///     显示端口信息(udp)
        /// </summary>
        /// <param name="sender"></param>
        public void ShowPortUdpInfoCommand(object sender)
        {
            _ipPortCollection = new ObservableCollection<IpCtrolViewModel>();
            var unused = new IpCtrolViewModel(ref _ipPortCollection, true);

            ThreadInvoker.Instance.InitDispatcher(((Window) sender).Dispatcher);
            ThreadInvoker.Instance.RunByUiThread(() =>
            {
                new Dialog_IpCtrolUDP(_ipPortCollection, true) {Owner = (Window) sender}.ShowDialog();
            });
        }

        /// <summary>
        ///     显示RawSocketSniffer
        /// </summary>
        /// <param name="sender"></param>
        public void ShowRawSocketSnifferCommand(object sender)
        {
            ThreadInvoker.Instance.InitDispatcher(((Window) sender).Dispatcher);
            ThreadInvoker.Instance.RunByUiThread(() =>
            {
                new Dialog_RawSocketSniffer {Owner = (Window) sender}.ShowDialog();
            });
        }

        /// <summary>
        ///     通过进程ID获取进程的目标地址(udp)
        /// </summary>
        /// <param name="sender"></param>
        public void GetDestinationAddressByPidCommand(object sender)
        {
            var pid = ((TextBox) sender).Text;
            var udpLocalPort = GetUdpLocalPortByPid(Convert.ToInt32(pid));
            
            if (0 == udpLocalPort) return;
            RawSocketSnifferViewModel rawSocketSniffer =
                new RawSocketSnifferViewModel(udpLocalPort);
            rawSocketSniffer.SendObjectEvent += headers =>
            {
                DestinationAddress = headers[0].DestinationAddress.ToString();
            };
        }
        
        #endregion

        #region 常量

        /// <summary>
        ///     超时
        /// </summary>
        private const int TimeOut = 100;

        /// <summary>
        ///     包大小
        /// </summary>
        private const int PacketSize = 32;

        /// <summary>
        ///     重试时间
        /// </summary>
        private const int TryTimes = 2;

        #endregion

        #region 内部方法

        #region 自定义规则的Ping

        public static float Test(string strHost, int packetSize, int timeOut, int tryTimes)
        {
            return LaunchPing(string.Format("{0} -n {1} -l {2} -w {3}", strHost, TryTimes, PacketSize, TimeOut));
        }

        public static float Test(string strHost)
        {
            return LaunchPing(string.Format("{0} -n {1} -l {2} -w {3}", strHost, TryTimes, PacketSize, TimeOut));
        }

        private static float LaunchPing(string arguments)
        {
            using (var p = new Process())
            {
                p.StartInfo.Arguments = arguments;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "ping.exe";
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;

                p.Start();
                var strResult = p.StandardOutput.ReadToEnd();
                p.Close();

                return ParseResult(strResult);
            }
        }

        /// <summary>
        ///     将输出结果转换为浮点型数据
        /// </summary>
        /// <param name="strBuffer"></param>
        /// <returns></returns>
        private static float ParseResult(string strBuffer)
        {
            var regex = new Regex(@"平均 = (.*?)ms", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            if (1 > strBuffer.Length)
                return 0.0F;

            var mc = regex.Matches(strBuffer);

            if (1 > mc.Count)
                return 0.0F;

            int avg;
            if (!int.TryParse(mc[0].Groups[1].Value, out avg))
                return 0.0F;
            if (0 >= avg)
                return 1024.0F;

            return avg;
        }
        
        #endregion

        /// <summary>
        /// 通过进程Pid获取基于UDP的远程端口
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        private static ushort GetUdpLocalPortByPid(int pid)
        {
            var collection = new ObservableCollection<IpCtrolViewModel>();
            var unused = new IpCtrolViewModel(ref collection, isUDP: true);

            var result = (from model in collection 
                where model.Pid == pid 
                select model.LocalPort).FirstOrDefault();

            return result;
        }

        /// <summary>
        /// 获取正在使用的IP地址
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetLocalIpv4()
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if(networkInterface.OperationalStatus != OperationalStatus.Up)
                    continue;
                if(networkInterface.GetIPProperties().GatewayAddresses.Count == 0)
                    continue;
                foreach (var ip in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        return ip.Address;
                }
            }
            return null;
        }

        #endregion

        #region 成员

        /// <summary>
        ///     网络端口信息集合
        /// </summary>
        private ObservableCollection<IpCtrolViewModel> _ipPortCollection;

        /// <summary>
        ///     UDP目标地址
        /// </summary>
        private string _destinationAddress;
        
        public string DestinationAddress
        {
            get { return _destinationAddress; }
            set 
            { 
                _destinationAddress = value;
                FirePropertyChanged(() => DestinationAddress);
            }
        }

        /// <summary>
        ///     网络延迟
        /// </summary>
        private string _delay;

        public string Delay
        {
            get { return _delay; }
            set
            {
                _delay = value;
                FirePropertyChanged(() => Delay);
            }
        }

        /// <summary>
        ///     目标地址或IP
        /// </summary>
        private string _targetUrl;

        public string TargetUrl
        {
            get { return _targetUrl; }
            set
            {
                _targetUrl = value;
                FirePropertyChanged(() => TargetUrl);
            }
        }

        /// <summary>
        ///     是否正在执行Ping
        /// </summary>
        private bool _isStopedPing;

        public bool IsStopedPing
        {
            get { return _isStopedPing; }
            set
            {
                _isStopedPing = value;
                FirePropertyChanged(() => IsStopedPing);
            }
        }

        #endregion
    }
}