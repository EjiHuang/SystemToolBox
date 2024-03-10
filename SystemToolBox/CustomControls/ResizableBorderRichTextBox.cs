using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

#region 自定义富文本框控件说明

/*
 * 感谢以下开源项目，为我提供了技术参考
 * How To Make a Simple Draggable-Resizable Text Box - CodeProject
 * https://www.codeproject.com/Tips/793304/How-To-Make-a-Simple-Draggable-Resizable-Text-Box
 * WPF Draggable Label - CodeProject
 * https://www.codeproject.com/Articles/71792/WPF-Draggable-Label
 *
 * 本控件为WPF下的可拖动，可调整大小的富文本框控件，本来想单纯写一个Border的，由于Border的Child属性比较坑，
 * 所以暂且将他往下扩展成富文本框控件了
 * 
 * 类似于画图工具或PowerPoint里面的插入自定义文本框那种
 * 
 * 代码不复杂，可以结合上面的2个源码项目进行学习或完善
 * 
 * 目前控件为最初版，并不完善，肯定会存在BUG
 */

#endregion

namespace SystemToolBox.CustomControls
{
    #region wpf自定义控件使用说明

    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SystemToolBox.CustomControls"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SystemToolBox.CustomControls;assembly=SystemToolBox.CustomControls"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:ResizableTextBox/>
    ///
    /// </summary>
    /// 

    #endregion

    #region 事件委托

    public delegate void DragEventHandler(object sender, DragEventArgs e);
    public delegate void ResizeEventHandler(object sender, ResizeEventArgs e);

    #endregion

    public sealed class ResizableBorderRichTextBox : Border
    {
        #region 事件声明

        public event DragEventHandler Drag;
        public event ResizeEventHandler Resize;

        #endregion

        #region 内部成员

        /// <summary>
        /// 前一个坐标
        /// </summary>
        private Point _previousLocation;

        /// <summary>
        /// 前一个转换
        /// </summary>
        private Transform _previousTransform;

        /// <summary>
        /// 调整大小的小方格
        /// </summary>
        private Label lbNW = new Label();
        private Label lbN = new Label();
        private Label lbNE = new Label();
        private Label lbE = new Label();
        private Label lbSE = new Label();
        private Label lbS = new Label();
        private Label lbSW = new Label();
        private Label lbW = new Label();

        /// <summary>
        /// 主容器
        /// </summary>
        private Canvas _MainCanvas = new Canvas();

        /// <summary>
        /// 主容器下的子控件
        /// </summary>
        private RichTextBox rtb = new RichTextBox();

        #endregion

        #region 构造函数

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static ResizableBorderRichTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizableBorderRichTextBox), new FrameworkPropertyMetadata(typeof(ResizableBorderRichTextBox)));
        }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public ResizableBorderRichTextBox()
        {
            // 将当前Border控件的儿子设置为Canvas容器
            Child = _MainCanvas;
            // Canvas容器进行数据绑定
            _MainCanvas.SetBinding(WidthProperty, new Binding("Width") { Source = this });
            _MainCanvas.SetBinding(HeightProperty, new Binding("Height") { Source = this });

            // 添加调整大小的小方格
            InitResizeHandles();

            // 配置子控件
            rtb.Foreground = Brushes.Red;
            rtb.BorderBrush = null;
            rtb.BorderThickness = new Thickness(0);
            rtb.Margin = new Thickness(0);
            rtb.SetValue(Block.LineHeightProperty, 1.0);  // 行高
            rtb.Background = null;
            // RichTextBox失去焦点时，隐藏小方格
            rtb.LostFocus += (sender, e) => { HideResizeHandles(); };
            // RichTextBox获得焦点时，显示小方格
            rtb.GotFocus += (sender, e) =>
            {
                ShowResizeHandles();
                ((RichTextBox) sender).SelectAll();
            };

            // 将子控件添加到Canvas儿子集合中
            _MainCanvas.Children.Add(rtb);
        }

        #endregion

        #region 自定义方法

        /// <summary>
        /// 拖动
        /// </summary>
        /// <param name="e"></param>
        private void OnDrag(DragEventArgs e)
        {
            DragEventHandler handler = Drag;
            if (null != handler)
                handler(this, e);
        }

        /// <summary>
        /// 调整大小
        /// </summary>
        /// <param name="e"></param>
        private void OnResize(ResizeEventArgs e)
        {
            ResizeEventHandler handler = Resize;
            if (null != handler)
                handler(this, e);
        }

        /// <summary>
        /// 初始化调整大小的小方格组
        /// </summary>
        private void InitResizeHandles()
        {
            InitHandles(lbNW);
            lbNW.Name = "lbNW";

            InitHandles(lbN);
            lbN.Name = "lbN";

            InitHandles(lbNE);
            lbNE.Name = "lbNE";

            InitHandles(lbE);
            lbE.Name = "lbE";

            InitHandles(lbSE);
            lbSE.Name = "lbSE";

            InitHandles(lbS);
            lbS.Name = "lbS";

            InitHandles(lbSW);
            lbSW.Name = "lbSW";

            InitHandles(lbW);
            lbW.Name = "lbW";
        }

        /// <summary>
        /// 初始化单个小方格
        /// </summary>
        /// <param name="label"></param>
        private void InitHandles(Label label)
        {
            label.Width = label.Height = 6;
            label.Padding = new Thickness(1);
            label.Background = Brushes.White;
            label.BorderBrush = Brushes.Gray;
            label.BorderThickness = new Thickness(1);
            label.Visibility = Visibility.Hidden;

            label.MouseMove += label_MouseMove;
            label.MouseEnter += label_MouseEnter;
            label.MouseLeave += label_MouseLeave;
            label.MouseDown += label_MouseDown;
            label.MouseUp += label_MouseUp;

            _MainCanvas.Children.Add(label);
        }

        /// <summary>
        /// 显示小方格
        /// </summary>
        public void ShowResizeHandles()
        {
            lbNW.Visibility = Visibility.Visible;
            lbN.Visibility = Visibility.Visible;
            lbNE.Visibility = Visibility.Visible;
            lbE.Visibility = Visibility.Visible;
            lbSE.Visibility = Visibility.Visible;
            lbS.Visibility = Visibility.Visible;
            lbSW.Visibility = Visibility.Visible;
            lbW.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 隐藏小方格
        /// </summary>
        public void HideResizeHandles()
        {
            lbNW.Visibility = Visibility.Hidden;
            lbN.Visibility = Visibility.Hidden;
            lbNE.Visibility = Visibility.Hidden;
            lbE.Visibility = Visibility.Hidden;
            lbSE.Visibility = Visibility.Hidden;
            lbS.Visibility = Visibility.Hidden;
            lbSW.Visibility = Visibility.Hidden;
            lbW.Visibility = Visibility.Hidden;
        }

        #endregion

        #region 重载Border的方法

        /// <summary>
        /// UI尺寸改变事件
        /// </summary>
        /// <param name="sizeInfo"></param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            // 子控件动态改变
            rtb.Width = rtb.MaxWidth = ActualWidth - 5 < 0 ? 5 : ActualWidth - 5;
            rtb.Height = rtb.MaxHeight = ActualHeight - 5 < 0 ? 5 : ActualHeight - 5;
            rtb.MinWidth = 5;
            rtb.MinHeight = 5;

            #region 调整大小的小方格位置

            // 西北
            lbNW.SetValue(Canvas.TopProperty, BorderThickness.Top * -2);
            lbNW.SetValue(Canvas.LeftProperty, BorderThickness.Left * -2);
            // 北
            lbN.SetValue(Canvas.TopProperty, BorderThickness.Top * -2);
            lbN.SetValue(Canvas.LeftProperty, ActualWidth * 0.5 - BorderThickness.Left * 2 - 1);
            // 东北
            lbNE.SetValue(Canvas.TopProperty, BorderThickness.Top * -2);
            lbNE.SetValue(Canvas.LeftProperty, ActualWidth * 1 - BorderThickness.Left * 3);
            // 东
            lbE.SetValue(Canvas.TopProperty, ActualHeight * 0.5 + BorderThickness.Top * -2);
            lbE.SetValue(Canvas.LeftProperty, ActualWidth * 1 - BorderThickness.Left * 3);
            // 东南
            lbSE.SetValue(Canvas.TopProperty, ActualHeight * 1 + BorderThickness.Top * -2 - 1);
            lbSE.SetValue(Canvas.LeftProperty, ActualWidth * 1 - BorderThickness.Left * 3);
            // 南
            lbS.SetValue(Canvas.TopProperty, ActualHeight * 1 + BorderThickness.Top * -2 - 1);
            lbS.SetValue(Canvas.LeftProperty, ActualWidth * 0.5 - BorderThickness.Left * 2 - 1);
            // 西南
            lbSW.SetValue(Canvas.TopProperty, ActualHeight * 1 + BorderThickness.Top * -2 - 1);
            lbSW.SetValue(Canvas.LeftProperty, BorderThickness.Left * -2);
            // 西
            lbW.SetValue(Canvas.TopProperty, ActualHeight * 0.5 + BorderThickness.Top * -2);
            lbW.SetValue(Canvas.LeftProperty, BorderThickness.Left * -2);

            #endregion
        }

        /// <summary>
        /// 鼠标离开事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            ReleaseMouseCapture();

            base.OnMouseLeave(e);
        }

        /// <summary>
        /// 鼠标左键松开事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();

            base.OnMouseLeftButtonUp(e);
        }

        /// <summary>
        /// 鼠标左键按下事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // 显示小方格组
            ShowResizeHandles();

            if (GetElementUnderMouse<Label>() == null)
                CaptureMouse();

            base.OnMouseLeftButtonDown(e);
        }

        /// <summary>
        /// 鼠标移动控件UI事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // 如果鼠标下方对象为Label或RichTextBox，则退出移动Border事件
            if (GetElementUnderMouse<Label>() != null || GetElementUnderMouse<RichTextBox>() != null)
                return;

            // 获取当前窗体坐标
            Point currentLocation = e.MouseDevice.GetPosition(Window.GetWindow(this));

            // 2D范畴内平移对象
            TranslateTransform move = new TranslateTransform(currentLocation.X - _previousLocation.X,
                currentLocation.Y - _previousLocation.Y);

            // 判断鼠标左键按下的情况
            if (MouseButtonState.Pressed == e.LeftButton && GetElementUnderMouse<Border>() != null)
            {
                // 移动Border控件
                if (Cursors.SizeAll == Cursor)
                {
                    var group = new TransformGroup();
                    if (null != _previousTransform)
                        group.Children.Add(_previousTransform);

                    group.Children.Add(move);

                    RenderTransform = group;

                    OnDrag(new DragEventArgs(currentLocation));
                }
            }

            // 当鼠标下方对象为Border时才改变鼠标样式
            if (null != GetElementUnderMouse<Border>())
                Cursor = Cursors.SizeAll;

            // 更新上一个坐标值
            _previousLocation = currentLocation;
            _previousTransform = RenderTransform;

            base.OnMouseMove(e);
        }

        #endregion

        #region 小方块鼠标事件

        /// <summary>
        /// 鼠标拖动小方块事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label_MouseMove(object sender, MouseEventArgs e)
        {
            // 根据当前窗口获取当前鼠标坐标值
            Point currentLocation = e.MouseDevice.GetPosition(Window.GetWindow(this));

            // 获取当前border的宽和高
            double width = double.IsNaN(Width) ? ActualWidth : Width;
            double height = double.IsNaN(Height) ? ActualHeight : Height;

            // 当鼠标在小方格上处于按下状态时，触发相应小方格的移动事件
            if (MouseButtonState.Pressed == e.LeftButton)
            {
                switch (((Label) sender).Name)
                {
                    case "lbW":
                        if (width + _previousLocation.X - currentLocation.X > MinWidth)
                        {
                            Width = width + _previousLocation.X - currentLocation.X;
                        }
                        OnResize(new ResizeEventArgs(new Size(Width, Height)));
                        break;
                    case "lbE":
                        if (width + currentLocation.X - _previousLocation.X > MinWidth)
                        {
                            Width = width + currentLocation.X - _previousLocation.X;
                        }
                        OnResize(new ResizeEventArgs(new Size(Width, Height)));
                        break;
                    case "lbN":
                        if (height + _previousLocation.Y - currentLocation.Y > MinHeight)
                        {
                            Height = height + _previousLocation.Y - currentLocation.Y;
                        }
                        OnResize(new ResizeEventArgs(new Size(Width, Height)));
                        break;
                    case "lbS":
                        if (height + currentLocation.Y - _previousLocation.Y > MinHeight)
                        {
                            Height = height + currentLocation.Y - _previousLocation.Y;
                        }
                        OnResize(new ResizeEventArgs(new Size(Width, Height)));
                        break;
                    case "lbNW":
                        if (width + _previousLocation.X - currentLocation.X > MinWidth &&
                            height + _previousLocation.Y - currentLocation.Y > MinHeight)
                        {
                            Width = width + _previousLocation.X - currentLocation.X;
                            Height = height + _previousLocation.Y - currentLocation.Y;
                        }
                        OnResize(new ResizeEventArgs(new Size(Width, Height)));
                        break;
                    case "lbNE":
                        if (width + currentLocation.X - _previousLocation.X > MinWidth &&
                            height + _previousLocation.Y - currentLocation.Y > MinHeight)
                        {
                            Width = width + currentLocation.X - _previousLocation.X;
                            Height = height + _previousLocation.Y - currentLocation.Y;
                        }
                        OnResize(new ResizeEventArgs(new Size(Width, Height)));
                        break;
                    case "lbSW":
                        if (width + _previousLocation.X - currentLocation.X > MinWidth &&
                            height + currentLocation.Y - _previousLocation.Y > MinHeight)
                        {
                            Width = width + _previousLocation.X - currentLocation.X;
                            Height = height + currentLocation.Y - _previousLocation.Y;
                        }
                        OnResize(new ResizeEventArgs(new Size(Width, Height)));
                        break;
                    case "lbSE":
                        if (width + currentLocation.X - _previousLocation.X > MinWidth &&
                            height + currentLocation.Y - _previousLocation.Y > MinHeight)
                        {
                            Width = width + currentLocation.X - _previousLocation.X;
                            Height = height + currentLocation.Y - _previousLocation.Y;
                        }
                        OnResize(new ResizeEventArgs(new Size(Width, Height)));
                        break;
                }

            }

            // 更新上一个坐标值
            _previousLocation = currentLocation;

            base.OnMouseMove(e);
        }

        /// <summary>
        /// 鼠标离开小方格事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label_MouseLeave(object sender, MouseEventArgs e)
        {
            // 释放鼠标
            ((Label) sender).ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// 鼠标进入小方块事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label_MouseEnter(object sender, MouseEventArgs e)
        {
            // 根据不同的方格位置，为其设置不同的鼠标样式
            var lbName = ((Label) sender).Name;
            switch (lbName)
            {
                case "lbNW":
                    Cursor = Cursors.SizeNWSE;
                    break;
                case "lbN":
                    Cursor = Cursors.SizeNS;
                    break;
                case "lbNE":
                    Cursor = Cursors.SizeNESW;
                    break;
                case "lbE":
                    Cursor = Cursors.SizeWE;
                    break;
                case "lbSE":
                    Cursor = Cursors.SizeNWSE;
                    break;
                case "lbS":
                    Cursor = Cursors.SizeNS;
                    break;
                case "lbSW":
                    Cursor = Cursors.SizeNESW;
                    break;
                case "lbW":
                    Cursor = Cursors.SizeWE;
                    break;
            }
        }

        /// <summary>
        /// 鼠标进入小方块按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 捕获鼠标
            ((Label) sender).CaptureMouse();
        }

        /// <summary>
        /// 鼠标按键松开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((Label) sender).ReleaseMouseCapture();
        }

        #endregion

        #region 其他方法

        /// <summary>
        /// 遍历父节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        private T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (null != parent)
            {
                var correctlyTyped = parent as T;
                if (null != correctlyTyped)
                    return correctlyTyped;

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return null;
        }

        /// <summary>
        /// 获取鼠标下的元素对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T GetElementUnderMouse<T>() where T : UIElement
        {
            return FindVisualParent<T>(Mouse.DirectlyOver as UIElement);
        }

        #endregion

    }

    #region 自定义参数类

    /// <summary>
    /// 自定义拖动参数类，包含坐标值，便于使用
    /// </summary>
    public class DragEventArgs : EventArgs
    {
        private Point _location;

        public DragEventArgs(Point location) { _location = location; }

        public int X { get { return (int)_location.X; } }

        public int Y { get { return (int)_location.Y; } }
    }

    /// <summary>
    /// 自定义调整大小参数类，包含长宽值，便于使用
    /// </summary>
    public class ResizeEventArgs : EventArgs
    {
        private Size _size;

        public ResizeEventArgs(Size size) { _size = size; }

        public int Width { get { return (int)_size.Width; } }

        public int Height { get { return (int)_size.Height; } }
    }

    #endregion
}
