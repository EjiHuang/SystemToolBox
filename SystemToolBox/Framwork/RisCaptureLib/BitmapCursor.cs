using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace SystemToolBox.Framwork.RisCaptureLib
{
    internal class BitmapCursor : SafeHandle
    {
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "DestroyIcon")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool DestroyIcon([System.Runtime.InteropServices.InAttribute()] System.IntPtr hIcon);

        #region 构造函数

        public static Cursor CreateBmpCursor(Bitmap cursorBitmap)
        {
            var bitmapCursor = new BitmapCursor(cursorBitmap);

            return CursorInteropHelper.Create(cursorHandle: bitmapCursor);
        }

        protected BitmapCursor(Bitmap cursorBitmap)
            : base((IntPtr)(-1), true)
        {
            // 返回图标的句柄
            handle = cursorBitmap.GetHicon();
        }

        #endregion

        #region 内部方法

        public static Cursor CreateCrossCursor()
        {
            // 设置图标的宽高
            const int w = 25;
            const int h = 25;
            // 创建位图
            var bmp = new Bitmap(w, h);
            // 使用GDI+对位图进行操作，初始化GDI+
            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.Default;
            g.InterpolationMode = InterpolationMode.High;

            // 创建一个画笔，绘制鼠标样式
            var pen = new Pen(Brushes.Black, 2);
            g.DrawLine(pen, new Point(12, 0), new Point(12, 8));
            g.DrawLine(pen, new Point(12, 17), new Point(12, 25));
            g.DrawLine(pen, new Point(0, 12), new Point(8, 12));
            g.DrawLine(pen, new Point(16, 12), new Point(24, 12));
            g.DrawLine(pen, new Point(12, 12), new Point(12, 13));

            g.Flush();
            g.Dispose();
            pen.Dispose();
            // 根据位图创建鼠标
            var cursor = CreateBmpCursor(bmp);
            bmp.Dispose();
            return cursor;
        }

        #endregion

        #region 重载基类方法

        // 重载SafeHandle的IsInvalid
        public override bool IsInvalid
        {
            get { return handle == (IntPtr)(-1); }
        }

        // 重载SafeHnadle的ReleaseHandle
        protected override bool ReleaseHandle()
        {
            bool result = DestroyIcon(handle);

            handle = (IntPtr)(-1);

            return result;
        }

        #endregion
    }
}
