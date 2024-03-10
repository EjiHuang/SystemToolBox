using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SystemToolBox.Framwork.RisCaptureLib
{
    internal class MaskWindow : Window
    {
        private MaskCanvas _InnerCanvas;
        private Bitmap _ScreenSnapshot;
        private System.Windows.Forms.Timer _TimeOutTimer;
        private readonly ScreenCaputre _ScreenCaputreOwner;

        public MaskWindow(ScreenCaputre screenCaputreOwner)
        {
            this._ScreenCaputreOwner = screenCaputreOwner;
            Ini();
        }

        private void Ini()
        {
            // 初始化常规属性
            // Topmost = true;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;

            // 设置范围覆盖所有屏幕
            var rect = SystemInformation.VirtualScreen;
            Left = rect.X;
            Top = rect.Y;
            Width = rect.Width;
            Height = rect.Height;

            // 设置背景
            _ScreenSnapshot = HelperMethods.GetScreenSnapshot();
            if (null != _ScreenSnapshot)
            {
                var bmp = _ScreenSnapshot.ToBitmapSource();
                bmp.Freeze();
                Background = new ImageBrush(bmp);
            }

            // 初始化canvas
            _InnerCanvas = new MaskCanvas
            {
                MaskWindowOwner = this
            };

            Content = _InnerCanvas;
        }

        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.RightButton == MouseButtonState.Pressed && e.ClickCount >= 2)
                CancelCaputre();
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (null != _TimeOutTimer && _TimeOutTimer.Enabled)
            {
                _TimeOutTimer.Stop();
                _TimeOutTimer.Start();
            }
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
                CancelCaputre();
        }

        private void CancelCaputre()
        {
            Close();
            _ScreenCaputreOwner.OnScreenCaputredCancelled(null);
        }

        internal void OnShowMaskFinished(Rect maskRegion)
        {

        }

        internal void ClipSnapshot(Rect clipRegion)
        {
            BitmapSource caputredBmp = CopyFromScreenSnapshot(clipRegion);

            if (null != caputredBmp)
                _ScreenCaputreOwner.OnScreenCaputred(null, caputredBmp);

            // 关闭遮罩窗口
            Close();
        }

        internal BitmapSource CopyFromScreenSnapshot(Rect region)
        {
            var sourceRect = region.ToRectangle();
            var destRect = new Rectangle(0, 0, sourceRect.Width, sourceRect.Height);

            if (null != _ScreenSnapshot)
            {
                var bitmap = new Bitmap(sourceRect.Width, sourceRect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(_ScreenSnapshot, destRect, sourceRect, GraphicsUnit.Pixel);
                }

                return bitmap.ToBitmapSource();
            }

            return null;
        }

        public void Show(int timeOutSecond, System.Windows.Size? defaultSize)
        {
            if (timeOutSecond > 0)
            {
                if (null == _TimeOutTimer)
                {
                    _TimeOutTimer = new System.Windows.Forms.Timer();
                    _TimeOutTimer.Tick += OnTimeOutTimerTick;
                }
                _TimeOutTimer.Interval = timeOutSecond * 1000;
                _TimeOutTimer.Start();
            }

            if (null != _InnerCanvas)
                _InnerCanvas.DefaultSize = defaultSize;

            Show();
            Focus();
        }

        private void OnTimeOutTimerTick(object sender, System.EventArgs e)
        {
            _TimeOutTimer.Stop();
            CancelCaputre();
        }
    }
}
