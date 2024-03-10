using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SystemToolBox.CustomControls;
using SystemToolBox.Framwork;
using SystemToolBox.Framwork.RisCaptureLib;
using SystemToolBox.Properties;

namespace SystemToolBox.ViewModel
{
    public class CaptureScreenViewModel : BaseNotifyPropertyChanged
    {
        // 主窗口
        private static SysToolBoxMain _targetAcceptor;
        // 截图控件
        private readonly ScreenCaputre _screenCaputre = new ScreenCaputre();
        // 最终尺寸
        private static Size? _lastSize;


        #region 绑定数据(单例模式)

        //public static readonly DependencyProperty CaptureScreenImageProperty =
        //    DependencyProperty.Register("CaptureScreenImage", typeof(ImageSource),
        //    typeof(CaptureScreenViewModel), new UIPropertyMetadata("no version!"));
        //public ImageSource CaptureScreenImage
        //{
        //    get { return (ImageSource)GetValue(CaptureScreenImageProperty); }
        //    set { SetValue(CaptureScreenImageProperty, value); }
        //}

        #endregion
        #region 数据绑定

        /// <summary>
        /// 是否进行绘图功能
        /// </summary>
        private int _brushThickness = 2;
        public int BrushThickness
        {
            get { return _brushThickness; }
            set { _brushThickness = value; FirePropertyChanged(() => BrushThickness); }
        }

        /// <summary>
        /// 文本输入框的坐标
        /// </summary>
        private static Point _textBoxPoint;
        public Point TextBoxPoint
        {
            get { return _textBoxPoint; }
            set { _textBoxPoint = value; FirePropertyChanged(() => TextBoxPoint); }
        }

        #endregion

        #region 构造函数&实例

        //public static CaptureScreenViewModel Instace { get; private set; }

        //static CaptureScreenViewModel() { Instace = new CaptureScreenViewModel(); }

        public CaptureScreenViewModel() { }

        public CaptureScreenViewModel(object oTargetAcceptor)
        {
            // 事件添加
            _screenCaputre.ScreenCaputred += OnScreenCaputred;

            // 获取主窗口，并且设置数据上下文
            _targetAcceptor = ((SysToolBoxMain)oTargetAcceptor);
        }

        #endregion

        #region 截图事件

        /// <summary>
        /// 取消截屏事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnScreenCaputreCancelled(object sender, EventArgs e)
        {
            _targetAcceptor.Show();
            _targetAcceptor.Focus();
        }

        /// <summary>
        /// 屏幕截图完毕事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void OnScreenCaputred(object sender, ScreenCaputredEventArgs e)
        {
            // 显示主窗口
            _targetAcceptor.Show();
            // 显示到InkCanvas控件中
            _targetAcceptor.CaptureScreen_InkCanvas.Width =
                _targetAcceptor.CaptureScreen_InkCanvas.MaxWidth = e.Bmp.Width;
            _targetAcceptor.CaptureScreen_InkCanvas.Height =
                _targetAcceptor.CaptureScreen_InkCanvas.MaxHeight = e.Bmp.Height;
            _targetAcceptor.CaptureScreen_InkCanvas.Background = new ImageBrush
            {
                ImageSource = e.Bmp
            };
            _targetAcceptor.CaptureScreen_InkCanvas.DefaultDrawingAttributes =
                new DrawingAttributes
                {
                    Color = Colors.Red,
                    FitToCurve = true,
                    Height = 2,
                    Width = 2
                };
            _targetAcceptor.CaptureScreen_InkCanvas.EditingMode = InkCanvasEditingMode.None;
            // 添加到剪贴板中
            Clipboard.SetImage(e.Bmp);
        }

        #endregion

        #region 命令

        /// <summary>
        /// 执行截图操作
        /// </summary>
        public void CaptureScreenExecuteCommand()
        {
            var captureScreenTabItem =
                ((MahApps.Metro.Controls.MetroTabItem) _targetAcceptor.SystemTool_TabControl.Items[2]);

            if (!captureScreenTabItem.IsSelected)
            {
                captureScreenTabItem.IsSelected = true;
            }

            _targetAcceptor.Hide();
            Thread.Sleep(300);

            _lastSize = default(Size);
            _screenCaputre.StartCaputre(30, _lastSize);
        }

        /// <summary>
        /// 检测是否使用画笔
        /// </summary>
        public void SelectBrushFeature(RadioButton radio)
        {
            if (radio.IsChecked != null && radio.IsChecked.Value)
            {
                switch ((string)radio.Content)
                {
                    case "无画刷":
                        _targetAcceptor.CaptureScreen_InkCanvas.EditingMode = InkCanvasEditingMode.None;
                        break;
                    case "圆形画刷":
                        _targetAcceptor.CaptureScreen_InkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                        _targetAcceptor.CaptureScreen_InkCanvas.DefaultDrawingAttributes.StylusTip = StylusTip.Ellipse;
                        _targetAcceptor.CaptureScreen_InkCanvas.DefaultDrawingAttributes.Height =
                            _targetAcceptor.CaptureScreen_InkCanvas.DefaultDrawingAttributes.Width =
                            BrushThickness;
                        break;
                    case "按点擦除":
                        _targetAcceptor.CaptureScreen_InkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                        break;
                    case "按线擦除":
                        _targetAcceptor.CaptureScreen_InkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                        break;
                    case "选中墨迹":
                        _targetAcceptor.CaptureScreen_InkCanvas.EditingMode = InkCanvasEditingMode.Select;
                        break;
                }
            }
        }

        /// <summary>
        /// 颜色选择
        /// </summary>
        public void CaptureScreenColorChangeCommand()
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (System.Windows.Forms.DialogResult.OK == colorDialog.ShowDialog())
            {
                _targetAcceptor.CaptureScreen_InkCanvas.DefaultDrawingAttributes.Color =
                    Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R,
                        colorDialog.Color.G, colorDialog.Color.B);

            }
        }

        #endregion

        #region 实用方法

        /// <summary>
        /// 向InkCanvas插入文本内容
        /// </summary>
        public static void InsertText()
        {
            ResizableBorderRichTextBox rtb = new ResizableBorderRichTextBox
            {
                Width = 100,
                Height = 50
            };

            InkCanvas.SetLeft(rtb, _textBoxPoint.X - 5);
            InkCanvas.SetTop(rtb, _textBoxPoint.Y - 5);
            
            _targetAcceptor.CaptureScreen_InkCanvas.Children.Add(rtb);

            //_TargetAcceptor.CaptureScreen_InkCanvas.Children.OfType<System.Windows.Controls.TextBox>().LastOrDefault().Focus();
            //_TargetAcceptor.CaptureScreen_InkCanvas.Children.OfType<System.Windows.Controls.TextBox>().LastOrDefault().SelectAll();
        }

        /// <summary>
        /// 使用画图工具打开当前截图
        /// </summary>
        public static void UseMspaintToOpen()
        {
            // 将当前截图保存到临时文件夹中
            string path = Environment.CurrentDirectory + "\\Temp";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            else
                ClearFolder(path);  // 清空临时文件

            path = path + "\\" + System.Text.RegularExpressions.Regex.Replace(DateTime.Now.ToString(CultureInfo.InvariantCulture), @"/|:| ", "") + ".jpeg";
            SaveInkCanvas(path);

            using (Process p = new Process())
            {
                p.StartInfo.FileName = Environment.SystemDirectory + "\\mspaint.exe";
                p.StartInfo.Arguments = string.Format(" {0}", path);
                p.StartInfo.CreateNoWindow = false;
                p.Start();
            }
        }

        /// <summary>
        /// 图片另存为
        /// </summary>
        public static void SaveImageTo()
        {
            string fileName = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            fileName = System.Text.RegularExpressions.Regex.Replace(fileName, @"/|:| ", "");

            System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = Resources.CaptureScreenViewModel_SaveImageTo,
                RestoreDirectory = true,
                FilterIndex = 6,
                FileName = fileName,
                Title = @"截图另存为..."
            };

            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveInkCanvas(saveDialog.FileName);
            }
        }

        /// <summary>
        /// 将InkCanvas保存到本地
        /// </summary>
        /// <param name="fileName">文件名，包含全路径</param>
        public static void SaveInkCanvas(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                RenderTargetBitmap rtb =
                    new RenderTargetBitmap((int)_targetAcceptor.CaptureScreen_InkCanvas.Width,
                        (int)_targetAcceptor.CaptureScreen_InkCanvas.Height,
                        96d, 96d,
                        PixelFormats.Default);
                rtb.Render(_targetAcceptor.CaptureScreen_InkCanvas);
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));
                encoder.Save(fs);
                fs.Close();
            }
        }

        /// <summary>
        /// 将InkCanvas转换为WriteableBitmap
        /// WriteableBitmap继承自BitmapSource
        /// </summary>
        /// <param name="surface"></param>
        /// <returns></returns>
        public static WriteableBitmap SaveAsWriteableBitmap(InkCanvas surface)
        {
            if (surface == null) return null;

            // Save current canvas transform
            Transform transform = surface.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            surface.LayoutTransform = null;

            // Get the size of canvas
            Size size = new Size(surface.ActualWidth, surface.ActualHeight);
            // Measure and arrange the surface
            // VERY IMPORTANT
            surface.Measure(size);
            surface.Arrange(new Rect(size));

            // Create a render bitmap and push the surface to it
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
              (int)size.Width,
              (int)size.Height,
              96d,
              96d,
              PixelFormats.Pbgra32);
            renderBitmap.Render(surface);


            //Restore previously saved layout
            surface.LayoutTransform = transform;

            //create and return a new WriteableBitmap using the RenderTargetBitmap
            return new WriteableBitmap(renderBitmap);
        }

        /// <summary>
        /// WriteableBitmap转换为Bitmap
        /// </summary>
        /// <param name="writeBmp"></param>
        /// <returns></returns>
        public static System.Drawing.Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp)
        {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
        }

        /// <summary>
        /// 清空文件夹内
        /// </summary>
        /// <param name="dir"></param>
        private static void ClearFolder(string dir)
        {
            foreach (var d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly", StringComparison.Ordinal) != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(d);
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo(d);
                    if (di.GetFiles().Length != 0)
                        ClearFolder(di.FullName);   // 此处递归

                    Directory.Delete(d);
                }
            }
        }

        #endregion

        /// <summary>
        /// 测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void Test(object sender, MouseButtonEventArgs e)
        {
            ((TextBox) sender).SelectAll();
        }
    }
}
