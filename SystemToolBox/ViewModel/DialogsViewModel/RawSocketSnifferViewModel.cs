using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SystemToolBox.Framwork;

namespace SystemToolBox.ViewModel.DialogsViewModel
{
    public class RawSocketSnifferViewModel : BaseNotifyPropertyChanged
    {
        #region 类成员

        public delegate void SendObjectEventHandler(ObservableCollection<IpHeader> ipHeaders);

        public event SendObjectEventHandler SendObjectEvent;

        #region 私有成员

        #region 扩展成员

        private readonly ObservableCollection<IpHeader> _ipHeaders;

        #endregion

        /// <summary>
        ///     用于STA线程
        /// </summary>
        private readonly Dispatcher _dispatcherUi;

        /// <summary>
        ///     数据报总数
        /// </summary>
        private int _totalPacketNumber;

        /// <summary>
        ///     数据报树节点集合
        /// </summary>
        private ObservableCollection<TreeViewItem> _packetsItems;

        /// <summary>
        ///     是否正在捕获
        /// </summary>
        private bool _isCapturing;

        /// <summary>
        ///     选中的IP地址
        /// </summary>
        private IPAddress _selectedIpAddress;

        /// <summary>
        ///     主套接字
        /// </summary>
        private Socket _mainSocket;

        /// <summary>
        ///     捕获的数据
        /// </summary>
        private byte[] _capturedData;

        #endregion

        #region 公开成员

        /// <summary>
        ///     选中的IP地址
        /// </summary>
        public IPAddress SelectedIpAddress
        {
            get { return _selectedIpAddress; }
            set
            {
                _selectedIpAddress = value;
                FirePropertyChanged(() => SelectedIpAddress);
            }
        }

        /// <summary>
        ///     数据报树节点集合
        /// </summary>
        public ObservableCollection<TreeViewItem> PacketsItems
        {
            get { return _packetsItems; }
            set
            {
                _packetsItems = value;
                FirePropertyChanged(() => PacketsItems);
            }
        }

        /// <summary>
        ///     数据报总数
        /// </summary>
        public int TotalPacketNumber
        {
            get { return _totalPacketNumber; }
            set
            {
                _totalPacketNumber = value;
                FirePropertyChanged(() => TotalPacketNumber);
            }
        }

        #endregion

        #region 构造函数

        public RawSocketSnifferViewModel(ushort port)
        {
            // 初始化内部成员

            _isCapturing = false;
            _capturedData = new byte[4096];
            _ipHeaders = new ObservableCollection<IpHeader>();

            _selectedIpAddress = NetwordToolViewModel.GetLocalIpv4();
            StartCaptureByPort(port);
        }

        public RawSocketSnifferViewModel(ComboBox targetListBox)
        {
            _dispatcherUi = targetListBox.Dispatcher;
            _dispatcherUi.ShutdownFinished += (s, e) => { _mainSocket.Close(); };
            // 初始化内部成员
            var ipAddressesList = new List<IPAddress>();
            _isCapturing = false;
            _capturedData = new byte[4096];
            _packetsItems = new ObservableCollection<TreeViewItem>();

            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            if (0 < hostEntry.AddressList.Length)
            {
                foreach (var ipAddress in hostEntry.AddressList)
                    ipAddressesList.Add(ipAddress);
                targetListBox.ItemsSource = ipAddressesList;
            }
        }

        #endregion

        #region 命令

        /// <summary>
        ///     捕获包
        /// </summary>
        /// <param name="sender"></param>
        public void CapturePacketCommand(object sender)
        {
            var btn = sender as Button;

            if (null == _selectedIpAddress)
            {
                MessageBox.Show(@"请在选择框中选择您当前正在使用的IP地址。", @"提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (btn == null) return;
            switch (btn.Content.ToString().Trim())
            {
                case "开始捕获":
                    _packetsItems.Clear();
                    StartCapture();
                    btn.Content = "停止捕获";
                    break;
                case "停止捕获":
                    _isCapturing = false;
                    btn.Content = "开始捕获";
                    break;
            }
        }

        #endregion

        #region 内部方法

        #region 扩展类方法

        /// <summary>
        ///     根据端口号进行捕获
        /// </summary>
        /// <param name="port"></param>
        private void StartCaptureByPort(ushort port)
        {
            try
            {
                if (!_isCapturing)
                {
                    _isCapturing = true;
                    // 建立套接字
                    _mainSocket = new Socket(AddressFamily.InterNetwork, // 使用IPv4 地址
                        SocketType.Raw, // 支持对基础传输协议的访问
                        ProtocolType.IP); // 使用IP协议
                    // 套接字绑定到所选的IP地址
                    _mainSocket.Bind(new IPEndPoint(_selectedIpAddress, port));
                    // 设置套接字选项
                    _mainSocket.SetSocketOption(SocketOptionLevel.IP, // 仅适用于IP类
                        SocketOptionName.HeaderIncluded, // 指示应用程序为输出数据包提供IP头
                        true);

                    var In = new byte[] {1, 0, 0, 0};
                    var Out = new byte[] {1, 0, 0, 0};
                    // .net下的IOControl来源至native下的WSAIoctl，枚举指定控制代码，为套接字设置底层操作模式
                    _mainSocket.IOControl(IOControlCode.ReceiveAll, // 启用对网络上所有IPv4数据包的接受
                        In, Out);
                    // 启动套接字
                    _mainSocket.BeginReceive(_capturedData, 0, _capturedData.Length, SocketFlags.None, OnReceiveEx,
                        null);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        ///     套接字接受数据事件
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceiveEx(IAsyncResult ar)
        {
            try
            {
                if (_ipHeaders.Count > 0)
                {
                    _isCapturing = false;
                    if (SendObjectEvent != null)
                        SendObjectEvent(_ipHeaders);
                }

                var received = _mainSocket.EndReceive(ar);

                PacketAnalyzerEx(_capturedData, received);

                if (_isCapturing)
                {
                    _capturedData = new byte[4096];

                    _mainSocket.BeginReceive(_capturedData, 0, _capturedData.Length, SocketFlags.None, OnReceiveEx,
                        null);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"错误", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        /// <summary>
        ///     数据报分析器
        /// </summary>
        /// <param name="data"></param>
        /// <param name="receivedLength"></param>
        private void PacketAnalyzerEx(byte[] data, int receivedLength)
        {
            // 由于所有协议数据包都封装在IP数据报中，所以我们首先解析IP报头并查看它的协议数据
            var ipHeader = new IpHeader(data, receivedLength);
            _ipHeaders.Add(ipHeader);
        }

        #endregion

        /// <summary>
        ///     开始捕获
        /// </summary>
        private void StartCapture()
        {
            try
            {
                if (!_isCapturing)
                {
                    _isCapturing = true;
                    // 建立套接字
                    _mainSocket = new Socket(AddressFamily.InterNetwork, // 使用IPv4 地址
                        SocketType.Raw, // 支持对基础传输协议的访问
                        ProtocolType.IP); // 使用IP协议
                    // 套接字绑定到所选的IP地址
                    _mainSocket.Bind(new IPEndPoint(_selectedIpAddress, 0));
                    // 设置套接字选项
                    _mainSocket.SetSocketOption(SocketOptionLevel.IP, // 仅适用于IP类
                        SocketOptionName.HeaderIncluded, // 指示应用程序为输出数据包提供IP头
                        true);

                    var In = new byte[] {1, 0, 0, 0};
                    var Out = new byte[] {1, 0, 0, 0};
                    // .net下的IOControl来源至native下的WSAIoctl，枚举指定控制代码，为套接字设置底层操作模式
                    _mainSocket.IOControl(IOControlCode.ReceiveAll, // 启用对网络上所有IPv4数据包的接受
                        In, Out);
                    // 启动套接字
                    _mainSocket.BeginReceive(_capturedData, 0, _capturedData.Length, SocketFlags.None, OnReceive, null);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        ///     套接字接受数据事件
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                var received = _mainSocket.EndReceive(ar);

                _dispatcherUi.Invoke(new Action(() => { PacketAnalyzer(_capturedData, received); }));

                if (_isCapturing)
                {
                    _capturedData = new byte[4096];

                    _mainSocket.BeginReceive(_capturedData, 0, _capturedData.Length, SocketFlags.None, OnReceive, null);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"错误", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        /// <summary>
        ///     数据报分析器
        /// </summary>
        /// <param name="data"></param>
        /// <param name="receivedLength"></param>
        private void PacketAnalyzer(byte[] data, int receivedLength)
        {
            var rootNode = new TreeViewItem();
            // 由于所有协议数据包都封装在IP数据报中，所以我们首先解析IP报头并查看它的协议数据
            var ipHeader = new IpHeader(data, receivedLength);
            var ipNode = MakeIpTreeNode(ipHeader);
            rootNode.Items.Add(ipNode);
            // 现在根据IP数据报携带的协议，我们解析数据报的数据字段
            switch (ipHeader.ProtocolType)
            {
                case Protocol.Tcp:
                    var tcpHeader = new TcpHeader(ipHeader.Data, // IP数据报所存放的数据
                        ipHeader.DataLength);
                    var tcpNode = MakeTcpTreeNode(tcpHeader);
                    rootNode.Items.Add(tcpNode);

                    // 如果端口等于53，则底层协议是DNS
                    if ("53" == tcpHeader.DestinationPort || "53" == tcpHeader.SourcePort)
                    {
                        var dnsNode = MakeDnsTreeNode(tcpHeader.Data, tcpHeader.DataLength);
                        rootNode.Items.Add(dnsNode);
                    }
                    break;
                case Protocol.Udp:
                    var udpHeader = new UdpHeader(ipHeader.Data, // IP数据报所存放的数据
                        ipHeader.DataLength);
                    var udpNode = MakeUdpTreeNode(udpHeader);
                    rootNode.Items.Add(udpNode);
                    // 如果端口等于53，则底层协议是DNS
                    if ("53" == udpHeader.DestinationPort || "53" == udpHeader.SourcePort)
                    {
                        // 因为UDP数据报头为8位，故数据长度需减去8
                        var dnsNode = MakeDnsTreeNode(udpHeader.Data, Convert.ToInt32(udpHeader.Length) - 8);
                        rootNode.Items.Add(dnsNode);
                    }
                    break;
                case Protocol.Unknown:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            rootNode.Header = string.Format("{0} - {1}", ipHeader.SourceAddress, ipHeader.DestinationAddress);

            _packetsItems.Add(rootNode);
            _totalPacketNumber = _packetsItems.Count;
        }

        /// <summary>
        ///     构建IP报头树节点
        /// </summary>
        /// <param name="ipHeader"></param>
        /// <returns></returns>
        private static TreeViewItem MakeIpTreeNode(IpHeader ipHeader)
        {
            var ipNode = new TreeViewItem {Header = @"IP"};

            ipNode.Items.Add("Version: " + ipHeader.IpVersion);
            ipNode.Items.Add("Header length: " + ipHeader.HeaderLength);
            ipNode.Items.Add("Differntiated services: " + ipHeader.DifferentiatedServices);
            ipNode.Items.Add("Total length: " + ipHeader.TotalLength);
            ipNode.Items.Add("Identification: " + ipHeader.Identification);
            ipNode.Items.Add("Flags: " + ipHeader.Flags);
            ipNode.Items.Add("Fragmentation offset: " + ipHeader.FragmentationOffset);
            ipNode.Items.Add("Time to live: " + ipHeader.Ttl);
            switch (ipHeader.ProtocolType)
            {
                case Protocol.Tcp:
                    ipNode.Items.Add("Protocol: TCP");
                    break;
                case Protocol.Udp:
                    ipNode.Items.Add("Protocol: UDP");
                    break;
                case Protocol.Unknown:
                    ipNode.Items.Add("Protocol: Unknown");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            ipNode.Items.Add("Checksum: " + ipHeader.Checksum);
            ipNode.Items.Add("Source address: " + ipHeader.SourceAddress);
            ipNode.Items.Add("Destination address: " + ipHeader.DestinationAddress);

            return ipNode;
        }

        /// <summary>
        ///     构建TCP报头树节点
        /// </summary>
        /// <param name="tcpHeader"></param>
        private static TreeViewItem MakeTcpTreeNode(TcpHeader tcpHeader)
        {
            var tcpNode = new TreeViewItem {Header = @"TCP"};

            tcpNode.Items.Add("Source port: " + tcpHeader.SourcePort);
            tcpNode.Items.Add("Destination port: " + tcpHeader.DestinationPort);
            tcpNode.Items.Add("Sequence number: " + tcpHeader.SequenceNumber);
            if ("" != tcpHeader.AcknowledgementNumber)
                tcpNode.Items.Add("Acknowledgement number: " + tcpHeader.AcknowledgementNumber);
            tcpNode.Items.Add("Header length: " + tcpHeader.HeaderLength);
            tcpNode.Items.Add("Flags: " + tcpHeader.Flags);
            tcpNode.Items.Add("Window size: " + tcpHeader.WindowSize);
            tcpNode.Items.Add("Checksum: " + tcpHeader.Checksum);
            if ("" != tcpHeader.UrgentPointer)
                tcpNode.Items.Add("Urgent pointer: " + tcpHeader.UrgentPointer);

            return tcpNode;
        }

        /// <summary>
        ///     构建UDP数据报头树节点
        /// </summary>
        /// <param name="udpHeader"></param>
        /// <returns></returns>
        private static TreeViewItem MakeUdpTreeNode(UdpHeader udpHeader)
        {
            var udpNode = new TreeViewItem {Header = @"UDP"};

            udpNode.Items.Add("Source port: " + udpHeader.SourcePort);
            udpNode.Items.Add("Destination port: " + udpHeader.DestinationPort);
            udpNode.Items.Add("Length: " + udpHeader.Length);
            udpNode.Items.Add("Checksum: " + udpHeader.Checksum);

            return udpNode;
        }

        /// <summary>
        ///     构建DNS数据报头树节点
        /// </summary>
        /// <param name="data"></param>
        /// <param name="receivedLength"></param>
        /// <returns></returns>
        private static TreeViewItem MakeDnsTreeNode(byte[] data, int receivedLength)
        {
            var dnsHeader = new DnsHeader(data, receivedLength);
            var dnsNode = new TreeViewItem {Header = @"DNS"};

            dnsNode.Items.Add("Identification: " + dnsHeader.Identification);
            dnsNode.Items.Add("Flags: " + dnsHeader.Flags);
            dnsNode.Items.Add("Questions: " + dnsHeader.TotalQuestions);
            dnsNode.Items.Add("Answer rrs: " + dnsHeader.TotalAnswerRRs);
            dnsNode.Items.Add("Authority rrs: " + dnsHeader.TotalAuthorityRRs);
            dnsNode.Items.Add("Additional rrs: " + dnsHeader.TotalAdditionalRRs);

            return dnsNode;
        }

        #endregion

        #endregion
    }


    #region IP头信息记录类

    public class IpHeader
    {
        #region 构造函数

        public IpHeader(byte[] buffer, int received)
        {
            try
            {
                using (var memoryStream = new MemoryStream(buffer, 0, received))
                {
                    // 使用BinaryReader读取二进制流的值很方便
                    var binaryReader = new BinaryReader(memoryStream);
                    _versionAndHeaderLength = binaryReader.ReadByte(); // 8 bits
                    _differentiatedServices = binaryReader.ReadByte(); // 8 bits
                    _totalLength = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16()); // 16 bits
                    _identification = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16()); // 16 bits
                    _flagsAndOffset = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16()); // 16 bits
                    _ttl = binaryReader.ReadByte(); // 8 bits
                    _protocol = binaryReader.ReadByte(); // 8 bits
                    _checksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16()); // 16 bits
                    _sourceIpAddress = (uint) binaryReader.ReadInt32(); // 32 bits
                    _destinationIpAddress = (uint) binaryReader.ReadInt32(); // 32 bits

                    // 计算头部长度
                    _headerLength = _versionAndHeaderLength;
                    // 版本和头部长度字段的最后四位包含头部长度，我们执行一些简单的二进制生成操作来提取它们
                    _headerLength <<= 4;
                    _headerLength >>= 4;
                    _headerLength *= 4;

                    // 拷贝封包数据，除去头部数据
                    Array.Copy(buffer, _headerLength, _ipData, 0, _totalLength - _headerLength);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"错误", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        #endregion

        #region 类成员

        #region IP头成员

        /// <summary>
        ///     4位IP版本号+4位首部长度
        /// </summary>
        private readonly byte _versionAndHeaderLength;

        /// <summary>
        ///     8位服务类型TOS
        /// </summary>
        private readonly byte _differentiatedServices;

        /// <summary>
        ///     16位数据包总长度（字节）
        /// </summary>
        private readonly ushort _totalLength;

        /// <summary>
        ///     16位标识
        /// </summary>
        private readonly ushort _identification;

        /// <summary>
        ///     3位标志位
        /// </summary>
        private readonly ushort _flagsAndOffset;

        /// <summary>
        ///     8位生存时间TTL
        /// </summary>
        private readonly byte _ttl;

        /// <summary>
        ///     8位协议(TCP, UDP, ICMP, Etc.)
        /// </summary>
        private readonly byte _protocol;

        /// <summary>
        ///     16位IP首部校验和
        /// </summary>
        private readonly short _checksum;

        /// <summary>
        ///     32位源IP地址
        /// </summary>
        private readonly uint _sourceIpAddress;

        /// <summary>
        ///     32位目的IP地址
        /// </summary>
        private readonly uint _destinationIpAddress;

        #endregion

        #region 私有成员

        /// <summary>
        ///     IP头长度
        /// </summary>
        private readonly byte _headerLength;

        /// <summary>
        ///     数据报携带的数据
        /// </summary>
        private readonly byte[] _ipData = new byte[4096];

        #endregion

        #region 公开成员

        /// <summary>
        ///     IP版本
        /// </summary>
        public string IpVersion
        {
            get
            {
                // 计算版本与长度字段获取IP版本
                switch (_versionAndHeaderLength >> 4)
                {
                    case 4:
                        return "IPv4";
                    case 6:
                        return "IPv6";
                }
                return "Unknown";
            }
        }

        /// <summary>
        ///     IP头长度
        /// </summary>
        public string HeaderLength
        {
            get { return _headerLength.ToString(); }
        }

        /// <summary>
        ///     数据包长度
        /// </summary>
        public ushort DataLength
        {
            get { return (ushort) (_totalLength - _headerLength); }
        }

        /// <summary>
        ///     服务类型TOS，以16进制与10进制显示
        /// </summary>
        public string DifferentiatedServices
        {
            get { return string.Format("0x{0:x2} ({1})", _differentiatedServices, _differentiatedServices); }
        }

        /// <summary>
        ///     标志
        /// </summary>
        public string Flags
        {
            get
            {
                // 标志和偏移字段的前3位记录标志信息，用于表示数据是否被分段
                switch (_flagsAndOffset >> 13)
                {
                    case 2:
                        return "数据不分段";
                    case 1:
                        return "数据分段";
                    default:
                        return (_flagsAndOffset >> 13).ToString();
                }
            }
        }

        /// <summary>
        ///     数据段偏移
        /// </summary>
        public string FragmentationOffset
        {
            get
            {
                // 标志和偏移字段的后13位记录数据段偏移的信息
                var offset = _flagsAndOffset << 3;
                offset >>= 3;
                return offset.ToString();
            }
        }

        /// <summary>
        ///     数据包被路由器丢弃之前允许通过的网段数量
        /// </summary>
        public string Ttl
        {
            get { return _ttl.ToString(); }
        }

        /// <summary>
        ///     协议类型
        /// </summary>
        public Protocol ProtocolType
        {
            get
            {
                if (6 == _protocol)
                    return Protocol.Tcp;
                return 17 != _protocol ? Protocol.Unknown : Protocol.Udp;
            }
        }

        /// <summary>
        ///     校验和
        /// </summary>
        public string Checksum
        {
            get { return string.Format("0x{0:x2} ({1})", _checksum, _checksum); }
        }

        /// <summary>
        ///     源地址
        /// </summary>
        public IPAddress SourceAddress
        {
            get { return new IPAddress(_sourceIpAddress); }
        }

        /// <summary>
        ///     目标地址
        /// </summary>
        public IPAddress DestinationAddress
        {
            get { return new IPAddress(_destinationIpAddress); }
        }

        /// <summary>
        ///     封包总长度
        /// </summary>
        public string TotalLength
        {
            get { return _totalLength.ToString(); }
        }

        /// <summary>
        ///     16位标识
        /// </summary>
        public string Identification
        {
            get { return _identification.ToString(); }
        }

        /// <summary>
        ///     封包数据（不包括头部数据）
        /// </summary>
        public byte[] Data
        {
            get { return _ipData; }
        }

        #endregion

        #endregion
    }

    #endregion

    #region TCP头信息记录类

    public class TcpHeader
    {
        #region 构造函数

        public TcpHeader(byte[] buffer, int received)
        {
            try
            {
                using (var memoryStream = new MemoryStream(buffer, 0, received))
                {
                    // 使用BinaryReader读取二进制流的值很方便
                    var binaryReader = new BinaryReader(memoryStream);
                    _sourcePort = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    _destinationPort = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    _sequenceNumber = (uint) IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                    _acknowledgementNumber = (uint) IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                    _dataOffsetAndFlags = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    _window = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    _checksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    _urgentPointer = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    // 数据偏移与标志前4位记录头部长度
                    _headerLength = (byte) (_dataOffsetAndFlags >> 12);
                    _headerLength *= 4;
                    // 数据长度
                    _dataLength = (ushort) (received - _headerLength);

                    Array.Copy(buffer, _headerLength, _tcpData, 0, received - _headerLength);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region 类成员

        #region TCP头成员

        /// <summary>
        ///     16位源端口
        /// </summary>
        private readonly ushort _sourcePort;

        /// <summary>
        ///     16位目标端口
        /// </summary>
        private readonly ushort _destinationPort;

        /// <summary>
        ///     32位序列号
        /// </summary>
        private readonly uint _sequenceNumber = 555;

        /// <summary>
        ///     32位确认号码
        /// </summary>
        private readonly uint _acknowledgementNumber = 555;

        /// <summary>
        ///     16位数据偏移和标志
        /// </summary>
        private readonly ushort _dataOffsetAndFlags = 555;

        /// <summary>
        ///     16位窗口大小
        /// </summary>
        private readonly ushort _window = 555;

        /// <summary>
        ///     16位校验和
        /// </summary>
        private readonly short _checksum = 555;

        /// <summary>
        ///     16位紧急指针
        /// </summary>
        private readonly ushort _urgentPointer;

        #endregion

        #region 私有成员

        /// <summary>
        ///     头部长度
        /// </summary>
        private readonly byte _headerLength;

        /// <summary>
        ///     数据长度（不包括头部数据）
        /// </summary>
        private readonly ushort _dataLength;

        /// <summary>
        ///     tcp数据（不包括头部数据）
        /// </summary>
        private readonly byte[] _tcpData = new byte[4096];

        #endregion

        #region 公开变量

        /// <summary>
        ///     源端口
        /// </summary>
        public string SourcePort
        {
            get { return _sourcePort.ToString(); }
        }

        /// <summary>
        ///     目标端口
        /// </summary>
        public string DestinationPort
        {
            get { return _destinationPort.ToString(); }
        }

        /// <summary>
        ///     序列号
        /// </summary>
        public string SequenceNumber
        {
            get { return _sequenceNumber.ToString(); }
        }

        /// <summary>
        ///     确认号码
        /// </summary>
        public string AcknowledgementNumber
        {
            get
            {
                // 只有在确认字段中有一个有效值，AKC标志才会被设置，所以在返回它数值之前进行检查
                return 0 != (_dataOffsetAndFlags & 0x10) ? _acknowledgementNumber.ToString() : "";
            }
        }

        /// <summary>
        ///     头部长度
        /// </summary>
        public string HeaderLength
        {
            get { return _headerLength.ToString(); }
        }

        /// <summary>
        ///     窗口尺寸
        /// </summary>
        public string WindowSize
        {
            get { return _window.ToString(); }
        }

        /// <summary>
        ///     紧急指针
        /// </summary>
        public string UrgentPointer
        {
            get
            {
                // 只有在确认字段中有一个有效值，紧急指针才会被设置，所以在返回它数值之前进行检查
                return 0 != (_dataOffsetAndFlags & 0x20) ? _urgentPointer.ToString() : "";
            }
        }


        public string Flags
        {
            get
            {
                // 数据偏移和标志字段的最后6位，包含控制位

                // 获取标志
                var flags = _dataOffsetAndFlags & 0x3F;
                var strflags = string.Format("0x{0:x2} (", flags);

                // 查看是否设置个别位
                if (0 != (flags & 0x01))
                    strflags += "FIN, ";
                if (0 != (flags & 0x02))
                    strflags += "SYN, ";
                if (0 != (flags & 0x04))
                    strflags += "RST, ";
                if (0 != (flags & 0x08))
                    strflags += "PSH, ";
                if (0 != (flags & 0x10))
                    strflags += "ACK, ";
                if (0 != (flags & 0x20))
                    strflags += "RTG";
                strflags += ")";

                if (strflags.Contains("()"))
                    strflags = strflags.Remove(strflags.Length - 3);
                else if (strflags.Contains(", )"))
                    strflags = strflags.Remove(strflags.Length - 3, 2);

                return strflags;
            }
        }

        /// <summary>
        ///     校验和
        /// </summary>
        public string Checksum
        {
            get { return string.Format("0x{0:x2}", _checksum); }
        }

        /// <summary>
        ///     封包数据（不包括头部数据）
        /// </summary>
        public byte[] Data
        {
            get { return _tcpData; }
        }

        /// <summary>
        ///     数据长度（不包括头部长度）
        /// </summary>
        public ushort DataLength
        {
            get { return _dataLength; }
        }

        #endregion

        #endregion
    }

    #endregion

    #region UDP头信息记录类

    public class UdpHeader
    {
        #region 类成员

        #region UDP头成员

        /// <summary>
        ///     16位源端口
        /// </summary>
        private readonly ushort _sourcePort;

        /// <summary>
        ///     16位目标端口
        /// </summary>
        private readonly ushort _destinationPort;

        /// <summary>
        ///     16位UDP头长度
        /// </summary>
        private readonly ushort _length;

        /// <summary>
        ///     16位校验值
        /// </summary>
        private readonly short _checksum;

        #endregion

        #region 私有成员

        /// <summary>
        ///     UDP封包数据（不包括头部数据）
        /// </summary>
        private readonly byte[] _udpData = new byte[4096];

        #endregion

        #region 公开成员

        /// <summary>
        ///     源端口
        /// </summary>
        public string SourcePort
        {
            get { return _sourcePort.ToString(); }
        }

        /// <summary>
        ///     目标端口
        /// </summary>
        public string DestinationPort
        {
            get { return _destinationPort.ToString(); }
        }

        /// <summary>
        ///     UDP头长度
        /// </summary>
        public string Length
        {
            get { return _length.ToString(); }
        }

        /// <summary>
        ///     校验和
        /// </summary>
        public string Checksum
        {
            get { return string.Format("0x{0:x2}", _checksum); }
        }

        /// <summary>
        ///     UDP封包数据（不包括头部数据）
        /// </summary>
        public byte[] Data
        {
            get { return _udpData; }
        }

        #endregion

        #region 构造函数

        public UdpHeader(byte[] buffer, int received)
        {
            try
            {
                using (var memoryStream = new MemoryStream(buffer, 0, received))
                {
                    var binaryReader = new BinaryReader(memoryStream);
                    _sourcePort = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    _destinationPort = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    _length = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    _checksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    // UDP的头部长度为8
                    Array.Copy(buffer, 8, _udpData, 0, received - 8);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #endregion
    }

    #endregion

    #region DNS头信息记录类

    public class DnsHeader
    {
        #region 构造函数

        public DnsHeader(byte[] buffer, int received)
        {
            try
            {
                using (var memoryStream = new MemoryStream(buffer, 0, received))
                {
                    var binaryReader = new BinaryReader(memoryStream);
                    _identification = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    _flags = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    _totalQuestions = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    _totalAnswerRRs = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    _totalAuthorityRRs = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    _totalAdditionalRRs = (ushort) IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region DNS头成员

        /// <summary>
        ///     16位标识
        /// </summary>
        private readonly ushort _identification;

        /// <summary>
        ///     16位标志
        /// </summary>
        private readonly ushort _flags;

        /// <summary>
        ///     16位问题列表中的条目数量
        /// </summary>
        private readonly ushort _totalQuestions;

        /// <summary>
        ///     16位应答资源记录列表中的条目数目
        /// </summary>
        private readonly ushort _totalAnswerRRs;

        /// <summary>
        ///     16位授权资源记录列表中的条目数目
        /// </summary>
        private readonly ushort _totalAuthorityRRs;

        /// <summary>
        ///     附加资源记录列表中的条目数目
        /// </summary>
        private readonly ushort _totalAdditionalRRs;

        #endregion

        #region 公开成员

        /// <summary>
        ///     标识
        /// </summary>
        public string Identification
        {
            get { return string.Format("0x{0:x2}", _identification); }
        }

        /// <summary>
        ///     标志
        /// </summary>
        public string Flags
        {
            get { return string.Format("0x{0:x2}", _flags); }
        }

        /// <summary>
        ///     问题列表中的条目数量
        /// </summary>
        public string TotalQuestions
        {
            get { return _totalQuestions.ToString(); }
        }

        /// <summary>
        ///     应答资源记录列表中的条目数量
        /// </summary>
        public string TotalAnswerRRs
        {
            get { return _totalAnswerRRs.ToString(); }
        }

        /// <summary>
        ///     授权资源记录列表中的条目数量
        /// </summary>
        public string TotalAuthorityRRs
        {
            get { return _totalAuthorityRRs.ToString(); }
        }

        /// <summary>
        ///     附加资源记录列表中的条目数量
        /// </summary>
        public string TotalAdditionalRRs
        {
            get { return _totalAdditionalRRs.ToString(); }
        }

        #endregion
    }

    #endregion

    #region 全局枚举

    public enum Protocol
    {
        Tcp = 6,
        Udp = 17,
        Unknown = -1
    }

    #endregion
}