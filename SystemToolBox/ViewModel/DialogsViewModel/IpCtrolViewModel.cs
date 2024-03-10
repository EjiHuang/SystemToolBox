using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;
using SystemToolBox.Framwork;

namespace SystemToolBox.ViewModel.DialogsViewModel
{
    public class IpCtrolViewModel : BaseNotifyPropertyChanged
    {
        #region 静态成员与常量

        /// <summary>
        ///     此处为GetExtendedTcpTable函数的第4个参数
        ///     IP类型，AF_INET——IPv4 AF_INET6——IPv6
        ///     此处为IPv4
        /// </summary>
        private const int AfInet = 2;

        /// <summary>
        ///     TCP连接列表
        /// </summary>
        private static List<TcpProcessRecord> tcpActiveConnections;

        private static List<UdpProcessRecord> udpActiveConnections;

        #endregion

        #region 内部成员

        /// <summary>
        ///     本地地址
        /// </summary>
        private IPAddress _localAddress;

        public IPAddress LocalAddress
        {
            get { return _localAddress; }
            set
            {
                _localAddress = value;
                FirePropertyChanged(() => LocalAddress);
            }
        }

        /// <summary>
        ///     远程地址
        /// </summary>
        private IPAddress _remoteAddress;

        public IPAddress RemoteAddress
        {
            get { return _remoteAddress; }
            set
            {
                _remoteAddress = value;
                FirePropertyChanged(() => RemoteAddress);
            }
        }

        /// <summary>
        ///     本地端口
        /// </summary>
        private ushort _localPort;

        public ushort LocalPort
        {
            get { return _localPort; }
            set
            {
                _localPort = value;
                FirePropertyChanged(() => LocalPort);
            }
        }

        /// <summary>
        ///     远程端口
        /// </summary>
        private ushort _remotePort;

        public ushort RemotePort
        {
            get { return _remotePort; }
            set
            {
                _remotePort = value;
                FirePropertyChanged(() => RemotePort);
            }
        }

        /// <summary>
        ///     进程ID
        /// </summary>
        private int _pid;

        public int Pid
        {
            get { return _pid; }
            set
            {
                _pid = value;
                FirePropertyChanged(() => Pid);
            }
        }

        /// <summary>
        ///     状态
        /// </summary>
        private MibTcpState _state;

        public MibTcpState State
        {
            get { return _state; }
            set
            {
                _state = value;
                FirePropertyChanged(() => State);
            }
        }

        /// <summary>
        /// 进程名
        /// </summary>
        private string _processName;

        public string ProcessName
        {
            get { return _processName; }
            set
            {
                _processName = value; 
                FirePropertyChanged(() => ProcessName);
            }
        }

        #endregion

        #region 构造函数

        private IpCtrolViewModel()
        {
        }

        public IpCtrolViewModel(ref ObservableCollection<IpCtrolViewModel> targetCollection, bool isUDP)
        {
            if (!isUDP)
            {
                tcpActiveConnections = GetAllTcpConnections();
                foreach (var record in tcpActiveConnections)
                {
                    targetCollection.Add(new IpCtrolViewModel
                    {
                        _localAddress = record.LocalAddress,
                        _remoteAddress = record.RemoteAddress,
                        _localPort = record.LocalPort,
                        _remotePort = record.RemotePort,
                        _state = record.State,
                        _pid = record.ProcessId,
                        _processName = record.ProcessName
                    });
                }
            }
            else
            {
                udpActiveConnections = GetAllUdpConnections();
                foreach (var record in udpActiveConnections)
                {
                    targetCollection.Add(new IpCtrolViewModel
                    {
                        _localAddress = record.LocalAddress,
                        _localPort = (ushort) record.LocalPort,
                        _pid = record.ProcessId,
                        _processName = record.ProcessName
                    });
                }
            }
        }

        #endregion

        #region 静态方法

        /// <summary>
        ///     获取所有的TCP连接记录
        /// </summary>
        /// <returns></returns>
        private static List<TcpProcessRecord> GetAllTcpConnections()
        {
            var bufferSize = 0;
            var tcpTableRecords = new List<TcpProcessRecord>();

            // 第一次调用GetExtendedTcpTable来获取TCP表的大小
            var result = GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AfInet,
                TcpTableClass.TcpTableOwnerPidAll);

            // 根据bufferSize分配足够的内存空间
            var tcpTableRecordsPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                // 继续调用GetExtendedTcpTable
                result = GetExtendedTcpTable(tcpTableRecordsPtr, ref bufferSize, true, AfInet,
                    TcpTableClass.TcpTableOwnerPidAll);

                // 结果不等于0代表失败
                if (0 != result)
                    return new List<TcpProcessRecord>();

                // 将非托管内存转化为结构体
                var tcpRecordsTable =
                    (MibTcptableOwnerPid) Marshal.PtrToStructure(tcpTableRecordsPtr, typeof(MibTcptableOwnerPid));
                // 内存指针指向下一个
                var tableRowPtr =
                    (IntPtr) ((long) tcpTableRecordsPtr + Marshal.SizeOf(tcpRecordsTable.dwNumEntries));

                // 从表中逐个读取并且解析TCP记录，将他们存储到TcpProcessRecord列表中
                for (var row = 0; row < tcpRecordsTable.dwNumEntries; row++)
                {
                    var tcpRow =
                        (MibTcprowOwnerPid) Marshal.PtrToStructure(tableRowPtr, typeof(MibTcprowOwnerPid));
                    tcpTableRecords.Add(new TcpProcessRecord(new IPAddress(tcpRow.localAddr),
                        new IPAddress(tcpRow.remoteAddr),
                        BitConverter.ToUInt16(new[] {tcpRow.localPort[1], tcpRow.localPort[0]}, 0),
                        BitConverter.ToUInt16(new[] {tcpRow.remotePort[1], tcpRow.remotePort[0]}, 0),
                        tcpRow.owningPid, tcpRow.state));
                    tableRowPtr = (IntPtr) ((long) tableRowPtr + Marshal.SizeOf(tcpRow));
                }
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                MessageBox.Show(outOfMemoryException.Message, "Out Of Memory", MessageBoxButton.OK,
                    MessageBoxImage.Stop);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception", MessageBoxButton.OK,
                    MessageBoxImage.Stop);
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTableRecordsPtr);
            }

            return tcpTableRecords.Distinct().ToList();
        }

        /// <summary>
        ///     获取所有的UDP连接记录
        /// </summary>
        /// <returns></returns>
        private static List<UdpProcessRecord> GetAllUdpConnections()
        {
            var bufferSize = 0;
            var udpTableRecords = new List<UdpProcessRecord>();

            // 第一次调用GetExtendedUdpTable来获取Udp表的大小
            var result =
                GetExtendedUdpTable(IntPtr.Zero, ref bufferSize, true, AfInet, UdpTableClass.UdpTableOwnerPid);

            // 根据bufferSize分配足够的内存空间
            var udpTableRecordPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                // 继续调用函数
                result = GetExtendedUdpTable(udpTableRecordPtr, ref bufferSize, true, AfInet,
                    UdpTableClass.UdpTableOwnerPid);

                // 结果不等于0代表失败
                if (0 != result)
                    return new List<UdpProcessRecord>();

                var udpRecordsTable =
                    (MibUdptableOwnerPid) Marshal.PtrToStructure(udpTableRecordPtr, typeof(MibUdptableOwnerPid));
                var tableRowPtr = (IntPtr) ((long) udpTableRecordPtr + Marshal.SizeOf(udpRecordsTable.dwNumEntries));

                for (var i = 0; i < udpRecordsTable.dwNumEntries; i++)
                {
                    var udpRow =
                        (MibUdprowOwnerPid) Marshal.PtrToStructure(tableRowPtr, typeof(MibUdprowOwnerPid));

                    udpTableRecords.Add(new UdpProcessRecord(new IPAddress(udpRow.localAddr),
                        BitConverter.ToUInt16(new[] {udpRow.localPort[1], udpRow.localPort[0]}, 0),
                        udpRow.owningPid));
                    tableRowPtr = (IntPtr) ((long) tableRowPtr + Marshal.SizeOf(udpRow));
                }
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                MessageBox.Show(outOfMemoryException.Message, "Out Of Memory", MessageBoxButton.OK,
                    MessageBoxImage.Stop);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception", MessageBoxButton.OK,
                    MessageBoxImage.Stop);
            }
            finally
            {
                Marshal.FreeHGlobal(udpTableRecordPtr);
            }

            return udpTableRecords.Distinct().ToList();
        }

        #endregion


        #region NativeMethod

        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize,
            bool bOrder, int ulAf, TcpTableClass tableClass, uint reserved = 0);

        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int pdwSize,
            bool bOrder, int ulAf, UdpTableClass tableClass, uint reserved = 0);

        #endregion
    }

    #region TCP记录类

    /// <summary>
    ///     根据TCP连接，保存其TCP连接地址、端口，还有调用该连接的进程名和进程ID
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TcpProcessRecord
    {
        public TcpProcessRecord(IPAddress localIp, IPAddress remoteIp, ushort localPort, ushort remotePort, int pid,
            MibTcpState state)
        {
            LocalAddress = localIp;
            RemoteAddress = remoteIp;
            LocalPort = localPort;
            RemotePort = remotePort;
            State = state;
            ProcessId = pid;

            // 通过进程ID获取进程名
            if (Process.GetProcesses().Any(process => process.Id == pid))
                ProcessName = Process.GetProcessById(ProcessId).ProcessName;
        }

        [DisplayName("Local Address")]
        public IPAddress LocalAddress { get; set; }

        [DisplayName("Local Port")]
        public ushort LocalPort { get; set; }

        [DisplayName("Remote Address")]
        public IPAddress RemoteAddress { get; set; }

        [DisplayName("Remote Port")]
        public ushort RemotePort { get; set; }

        [DisplayName("State")]
        public MibTcpState State { get; set; }

        [DisplayName("Process ID")]
        public int ProcessId { get; set; }

        [DisplayName("Process Name")]
        public string ProcessName { get; set; }
    }

    #endregion

    #region UDP记录类

    /// <summary>
    ///     根据UDP连接，保存其UDP连接地址、端口，还有调用该连接的进程名和进程ID
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class UdpProcessRecord
    {
        public UdpProcessRecord(IPAddress localAddress, uint localPort, int pid)
        {
            LocalAddress = localAddress;
            LocalPort = localPort;
            ProcessId = pid;
            // 根据进程ID获取进程名
            if (Process.GetProcesses().Any(process => process.Id == pid))
                ProcessName = Process.GetProcessById(ProcessId).ProcessName;
        }

        [DisplayName("Local Address")]
        public IPAddress LocalAddress { get; set; }

        [DisplayName("Local Port")]
        public uint LocalPort { get; set; }

        [DisplayName("Process ID")]
        public int ProcessId { get; set; }

        [DisplayName("Process Name")]
        public string ProcessName { get; set; }
    }

    #endregion

    #region 枚举变量与结构体

    /// <summary>
    ///     TCP连接的不同状态
    /// </summary>
    public enum MibTcpState
    {
        Closed = 1,
        Listening = 2,
        SynSent = 3,
        SynRcvd = 4,
        Established = 5,
        FinWait1 = 6,
        FinWait2 = 7,
        CloseWait = 8,
        Closing = 9,
        LastAck = 10,
        TimeWait = 11,
        DeleteTcb = 12,
        None = 0
    }

    /// <summary>
    ///     枚举定义用于指示返回的表的类型的值的集合
    ///     用于调用函数GetExtendedTcpTable
    /// </summary>
    public enum TcpTableClass
    {
        TcpTableBasicListener,
        TcpTableBasicConnections,
        TcpTableBasicAll,
        TcpTableOwnerPidListener,
        TcpTableOwnerPidConnections,
        TcpTableOwnerPidAll,
        TcpTableOwnerModuleListener,
        TcpTableOwnerModuleConnections,
        TcpTableOwnerModuleAll
    }

    /// <summary>
    ///     枚举定义用于指示由调用返回的表的类型的值的集合
    ///     用于调用函数GetExtendedUdpTable
    /// </summary>
    public enum UdpTableClass
    {
        UdpTableBasic,
        UdpTableOwnerPid,
        UdpTableOwnerModule
    }

    /// <summary>
    ///     该结构包含描述IPv4 TCP连接的信息
    ///     IPv4地址、TCP连接使用的端口以及与之关联的进程ID
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MibTcprowOwnerPid
    {
        public MibTcpState state;
        public uint localAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] localPort;
        public uint remoteAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] remotePort;
        public int owningPid;
    }

    /// <summary>
    ///     该结构包含进程ID
    ///     IPv4 TCP链接作为上下文绑定到这些进程中
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MibTcptableOwnerPid
    {
        public uint dwNumEntries;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct,
            SizeConst = 1)] public MibTcprowOwnerPid[] table;
    }

    /// <summary>
    ///     IPv4的UDP监听器表
    ///     在本地计算机上，该表还包含为每个UDP端点发出对绑定函数的调用的进程ID（PID）
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MibUdptableOwnerPid
    {
        public uint dwNumEntries;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct,
            SizeConst = 1)] public UdpProcessRecord[] table;
    }

    /// <summary>
    ///     该结构包含来自用户数据报协议（UDP）侦听器的条目的IPv4表
    ///     该条目还包括进程ID以及它发出对UDP端点的绑定函数的调用
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MibUdprowOwnerPid
    {
        public uint localAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] localPort;
        public int owningPid;
    }

    #endregion
}