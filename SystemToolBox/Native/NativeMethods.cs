using System;
using System.Runtime.InteropServices;

namespace SystemToolBox.Native
{
    /// <summary>
    ///     方法定义
    /// </summary>
    public class NativeMethods
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool LookupPrivilegeValue(string lpsystemname, string lpname,
            [MarshalAs(UnmanagedType.Struct)] ref NativeStructure.Luid lpLuid);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AdjustTokenPrivileges(IntPtr tokenhandle,
            [MarshalAs(UnmanagedType.Bool)] bool disableAllPrivileges,
            [MarshalAs(UnmanagedType.Struct)] ref NativeStructure.TokenPrivileges newstate,
            uint bufferlength, IntPtr previousState, IntPtr returnlength);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool OpenProcessToken(IntPtr processHandle,
            uint desiredAccesss,
            out IntPtr tokenHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll", EntryPoint = "ExitWindowsEx")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ExitWindowsEx([MarshalAs(UnmanagedType.U4)] NativeConstants.ExitWindowsAction uFlags,
            uint dwReason);
    }

    /// <summary>
    ///     结构体定义
    /// </summary>
    public class NativeStructure
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Luid
        {
            internal int LowPart;
            internal uint HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TokenPrivileges
        {
            internal int PrivilegeCount;
            internal Luid Luid;
            internal int Attributes;
        }
    }
}