using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SystemToolBox.Framwork.RisCaptureLib
{
    internal static class HelperMethods
    {
        /// <summary>
        /// Rectangle convert to Rect
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        public static Rect ToRect(this Rectangle rectangle)
        {
            return new Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        /// <summary>
        /// Rect convert to Rectangle
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Rectangle ToRectangle(this Rect rect)
        {
            return new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }

        /// <summary>
        /// 获取覆盖系统所有显示屏幕的矩形
        /// </summary>
        /// <returns></returns>
        public static Rect GetRectContainsAllScreens()
        {
            var rect = Rect.Empty;
            // 遍历系统上所有的显示屏幕
            foreach (Screen screen in Screen.AllScreens)
            {
                // 放大矩形，让它刚好覆盖显示器
                rect.Union(screen.Bounds.ToRect());
            }

            return rect;
        }

        /// <summary>
        /// 获取显示屏幕的快照
        /// </summary>
        /// <returns></returns>
        public static Bitmap GetScreenSnapshot()
        {
            try
            {
                // 获取虚拟屏幕的界限
                Rectangle rectangle = SystemInformation.VirtualScreen;
                // 根据界限创建位图
                var bitmap = new Bitmap(rectangle.Width, rectangle.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                // GDI+打开位图并且将屏幕快照保存到位图中去
                using (Graphics memoryGrahics = Graphics.FromImage(bitmap))
                {
                    memoryGrahics.CopyFromScreen(rectangle.X, rectangle.Y, 0, 0, rectangle.Size, CopyPixelOperation.SourceCopy);
                }

                return bitmap;
            }
            catch (Exception)
            {
                
            }

            return null;
        }

        /// <summary>
        /// Bitmap convert to BitmapSource
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            BitmapSource bitmapSource;

            try
            {
                bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                    IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                bitmapSource = null;
            }

            return bitmapSource;
        }

        /// <summary>
        /// 获取对象的父母
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        public static T GetAncestor<T>(this DependencyObject element)
        {
            while (!(null == element || element is T))
                element = VisualTreeHelper.GetParent(element);

            if ((null != element) && element is T)
                return (T)(object)element;

            return default(T);
        }

        /// <summary>
        /// 获取渲染转换器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        public static T GetRenderTransform<T>(this UIElement element) where T : Transform
        {
            // 判断是否为恒等矩阵
            // 然后创建一个效果叠加组，添加到对象渲染器中
            if (element.RenderTransform.Value.IsIdentity)
                element.RenderTransform = CreateSimpleTransformGroup();

            if (element.RenderTransform is T)
                return (T)element.RenderTransform;

            if (element.RenderTransform is TransformGroup)
            {
                var group = (TransformGroup)element.RenderTransform;

                foreach (var t in group.Children)
                {
                    if (t is T)
                        return (T)t;
                }
            }

            throw new NotSupportedException("Can not get instance of " + typeof(T).Name + " from " + element + "'s RenderTransform : " + element.RenderTransform);
        }

        /// <summary>
        /// 创建一个简单的效果叠加器
        /// </summary>
        /// <returns></returns>
        public static TransformGroup CreateSimpleTransformGroup()
        {
            var group = new TransformGroup();

            // 注意：RotateTransform属性必须是第一个加入组
            group.Children.Add(new RotateTransform());
            group.Children.Add(new TranslateTransform());
            group.Children.Add(new ScaleTransform());
            group.Children.Add(new SkewTransform());

            return group;
        }

        /// <summary>
        /// 判断该浮点型数是否为正常的数值
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool IsNormalNumber(this double d)
        {
            return !double.IsInfinity(d) &&
                !double.IsNaN(d) &&
                !double.IsNegativeInfinity(d) &&
                !double.IsPositiveInfinity(d);
        }
    }
}
