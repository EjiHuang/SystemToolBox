using System;
using System.Diagnostics;

namespace SystemToolBox.Framwork.CmdHelper
{
    public class CmdHelper
    {
        #region cmd执行库

        /// <summary>
        ///     cmd进程的完整路径
        /// </summary>
        private string _cmdPath;

        public CmdHelper(string cmdPath)
        {
            _cmdPath = cmdPath;
        }

        /// <summary>
        ///     执行cmd命令
        /// </summary>
        /// <param name="executeStr">执行语句</param>
        /// <param name="retStr">返回结果</param>
        public static void RunCmd(string executeStr, out string retStr)
        {
            executeStr = executeStr.Trim().TrimEnd('&') + "&exit";
            using (var p = new Process())
            {
                p.StartInfo.FileName = executeStr;
                p.StartInfo.UseShellExecute = false; // 是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true; // 接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true; // 接受来自调用程序的输出信息
                p.StartInfo.RedirectStandardError = true; // 重定向标准错误输出
                p.StartInfo.CreateNoWindow = true; // 不显示调用程序窗口
                p.Start();

                // 向CMD程序写入命令
                p.StandardInput.WriteLine(executeStr);
                p.StandardInput.AutoFlush = true; // 刷新缓冲区到基础流

                // 获取CMD程序的输出信息
                retStr = p.StandardOutput.ReadToEnd();
                p.WaitForExit(); // 等待调用程序执行完毕退出进程
                p.Close();
            }
        }

        /// <summary>
        ///     执行cmd命令
        /// </summary>
        /// <param name="executeStr">执行语句</param>
        public static void RunCmd(string executeStr)
        {
            executeStr = executeStr.Trim().TrimEnd('&') + "&exit";
            using (var p = new Process())
            {
                p.StartInfo.FileName = executeStr;
                p.StartInfo.UseShellExecute = false; // 是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true; // 接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true; // 接受来自调用程序的输出信息
                p.StartInfo.RedirectStandardError = true; // 重定向标准错误输出
                p.StartInfo.CreateNoWindow = true; // 不显示调用程序窗口
                p.Start();

                // 向CMD程序写入命令
                p.StandardInput.WriteLine(executeStr);
                p.StandardInput.AutoFlush = true; // 刷新缓冲区到基础流

                p.WaitForExit(); // 等待调用程序执行完毕退出进程
                p.Close();
            }
        }

        public static bool RunBat(string path)
        {
            try
            {
                using (var p = new Process())
                {
                    p.StartInfo.FileName = path;
                    p.StartInfo.Arguments = "10";
                    p.StartInfo.CreateNoWindow = false;
                    p.Start();
                    p.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace));
            }
            return true;
        }

        #endregion
    }
}