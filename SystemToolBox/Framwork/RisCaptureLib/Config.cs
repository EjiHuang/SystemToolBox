using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace SystemToolBox.Framwork.RisCaptureLib
{
    internal class Config
    {
        /// <summary>
        /// 选择区域的边框颜色
        /// </summary>
        public static Brush SelectionBorderBrush = new SolidColorBrush(Color.FromArgb(255, 49, 106, 196));

        /// <summary>
        /// 选择区域的边框厚度
        /// </summary>
        public static Thickness SelectionBorderThickness = new Thickness(2.0);

        /// <summary>
        /// 选择区域的背景色
        /// </summary>
        public static Brush MaskWindowBackground = new SolidColorBrush(Color.FromArgb(120, 255, 255, 255));
    }
}
