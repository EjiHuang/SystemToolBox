using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace SystemToolBox.Framwork.FileHelper
{
    public class FileHelper
    {
        /// <summary>
        ///     粉碎文件
        /// </summary>
        /// <param name="fileName">文件路径</param>
        /// <param name="deleteCount">覆盖删除次数</param>
        /// <param name="randomData">是否随机数据覆盖，默认是</param>
        /// <param name="blanks">是否空白覆盖，默认否</param>
        /// <returns></returns>
        public static bool SmashFile(string fileName, int deleteCount,
            bool randomData = true, bool blanks = false)
        {
            const int iBufferLength = 1024000;
            var bRet = true;

            try
            {
                using (var oStream = new FileStream(fileName, FileMode.Open,
                    FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    var oFileInfo = new FileInfo(fileName);
                    var lCount = oFileInfo.Length;
                    long lOffset = 0;
                    var aRowDataBuffer = new byte[iBufferLength];

                    while (0 <= lCount)
                    {
                        var iNumOfDataRead = oStream.Read(aRowDataBuffer, 0, iBufferLength);
                        if (0 == iNumOfDataRead)
                            break;
                        if (randomData)
                        {
                            // 随机数填充数组
                            var oRandomByte = new Random();
                            oRandomByte.NextBytes(aRowDataBuffer);
                        }
                        else if (blanks)
                        {
                            for (var i = 0; i < iNumOfDataRead; i++)
                                aRowDataBuffer[i] = 0;
                        }
                        else
                        {
                            for (var i = 0; i < iNumOfDataRead; i++)
                                aRowDataBuffer[i] = Convert.ToByte(Convert.ToChar(deleteCount));
                        }
                        // 写新内容到文件
                        for (var i = 0; i < deleteCount; i++)
                        {
                            oStream.Seek(lOffset, SeekOrigin.Begin);
                            oStream.Write(aRowDataBuffer, 0, iNumOfDataRead);
                        }
                        lOffset += iNumOfDataRead;
                        lCount -= iNumOfDataRead;
                    }
                }

                // 每个文件名字符代替随机数从0到9
                var szNewName = string.Empty;
                do
                {
                    var oRandom = new Random();
                    var szCleanName = Path.GetFileName(fileName);
                    var szDirName = Path.GetDirectoryName(fileName);
                    var iMoreRandomLetters = oRandom.Next(9);
                    // 为了更安全，不只使用原文件名的大小，添加一些随机字母
                    for (var i = 0; i < szCleanName.Length + iMoreRandomLetters; i++)
                        szNewName += oRandom.Next(9).ToString();
                    szNewName = szDirName + "\\" + szNewName;
                } while (File.Exists(szNewName));
            }
            catch
            {
                // 可能会有其他的原因导致删除失败，这里使用强制方法
                try
                {
                    var oProcess = new Process
                    {
                        StartInfo =
                        {
                            FileName = "handle.exe",
                            Arguments = fileName + " /accepteula",
                            UseShellExecute = false,
                            RedirectStandardOutput = true
                        }
                    };
                    oProcess.Start();
                    oProcess.WaitForExit();
                    var szOutPutTool = oProcess.StandardOutput.ReadToEnd();
                    var szMatchPattern = @"(?<=\s+pid:\s+)\b(\d+)\b(?=\s+)";
                    foreach (Match match in Regex.Matches(szOutPutTool, szMatchPattern))
                        // 终结掉所有正在使用这个文件的程序
                        Process.GetProcessById(int.Parse(match.Value)).Kill();
                    File.Delete(fileName);
                }
                catch (Exception)
                {
                    bRet = false;
                }
            }
            return bRet;
        }
    }
}