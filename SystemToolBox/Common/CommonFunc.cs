using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows;

namespace SystemToolBox.Common
{
    public static class CommonFunc
    {
        /// <summary>
        /// 判断当前主体是否属于具有Adminsitrator的Windows用户组
        /// </summary>
        /// <returns></returns>
        internal static bool IsAdministrator()
        {
            try
            {
                WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
                WindowsPrincipal windowsPrincipal = new WindowsPrincipal(windowsIdentity);
                return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                CheckError();
                return false;
            }
            
        }

        /// <summary>
        /// 检测错误
        /// </summary>
        internal static void CheckError()
        {
            var error = new Win32Exception(Marshal.GetLastWin32Error()).Message;
            MessageBox.Show(error, @"Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
