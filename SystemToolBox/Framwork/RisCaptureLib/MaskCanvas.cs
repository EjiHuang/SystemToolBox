using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SystemToolBox.Framwork.RisCaptureLib
{
    internal class MaskCanvas : Canvas
    {
        #region 字段和属性

        private IndicatorObject _Indicator;
        private Point? _SelectionStartPoint;
        private Point? _SelectionEndPoint;
        private Rect _SelectionRegion = Rect.Empty;
        private bool _IsMaskDraging;

        private readonly System.Windows.Shapes.Rectangle _SelectionBorder = new System.Windows.Shapes.Rectangle();
        private readonly System.Windows.Shapes.Rectangle _MaskRectLeft = new System.Windows.Shapes.Rectangle();
        private readonly System.Windows.Shapes.Rectangle _MaskRectRight = new System.Windows.Shapes.Rectangle();
        private readonly System.Windows.Shapes.Rectangle _MaskRectTop = new System.Windows.Shapes.Rectangle();
        private readonly System.Windows.Shapes.Rectangle _MaskRectBottom = new System.Windows.Shapes.Rectangle();

        public Size? DefaultSize { get; set; }

        public MaskWindow MaskWindowOwner { get; set; }

        #endregion

        public MaskCanvas()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 使渲染效果与SnapsToDevicePixels相同
            // “SnapsToDevicePixels = true;” 不适用于“OnRender”
            // 但是，这可能会使渲染目标原点位置偏移一些
            // SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            // 初始化
            Cursor = BitmapCursor.CreateCrossCursor();
            Background = Brushes.Transparent;

            // 初始化遮罩矩形
            _MaskRectLeft.Fill = _MaskRectRight.Fill = _MaskRectTop.Fill = _MaskRectBottom.Fill = Config.MaskWindowBackground;

            // 下面这些属性将不改变
            SetLeft(_MaskRectLeft, 0);
            SetTop(_MaskRectLeft, 0);
            SetRight(_MaskRectRight, 0);
            SetTop(_MaskRectRight, 0);
            SetTop(_MaskRectTop, 0);
            SetBottom(_MaskRectBottom, 0);
            _MaskRectLeft.Height = ActualHeight;

            Children.Add(_MaskRectLeft);
            Children.Add(_MaskRectRight);
            Children.Add(_MaskRectTop);
            Children.Add(_MaskRectBottom);

            // 初始化选择边框
            _SelectionBorder.Stroke = Config.SelectionBorderBrush;
            _SelectionBorder.StrokeThickness = Config.SelectionBorderThickness.Left;
            Children.Add(_SelectionBorder);

            // 初始化指示符
            _Indicator = new IndicatorObject(this);
            Children.Add(_Indicator);

            // 应用程序选择界面渲染事件
            CompositionTarget.Rendering += OnCompositionTargetRendering;
        }

        private void UpdateSelectionBorderLayout()
        {
            if (!_SelectionRegion.IsEmpty)
            {
                SetLeft(_SelectionBorder, _SelectionRegion.Left);
                SetTop(_SelectionBorder, _SelectionRegion.Top);
                _SelectionBorder.Width = _SelectionRegion.Width;
                _SelectionBorder.Height = _SelectionRegion.Height;
            }
        }

        private void UpdateMaskRectanglesLayout()
        {
            var actualHeight = ActualHeight;
            var actualWidth = ActualWidth;

            if (_SelectionRegion.IsEmpty)
            {
                SetLeft(_MaskRectLeft, 0);
                SetTop(_MaskRectLeft, 0);
                _MaskRectLeft.Width = actualWidth;
                _MaskRectLeft.Height = actualHeight;

                _MaskRectRight.Width =
                    _MaskRectRight.Height =
                    _MaskRectTop.Width =
                    _MaskRectTop.Height =
                    _MaskRectBottom.Width =
                    _MaskRectBottom.Height = 0;
            }
            else
            {
                var temp = _SelectionRegion.Left;
                if (temp != _MaskRectLeft.Width)
                    _MaskRectLeft.Width = temp < 0 ? 0 : temp;  // Math.Max(0, selectionRegion.Left);

                temp = ActualWidth - _SelectionRegion.Right;
                if (temp != _MaskRectRight.Width)
                    _MaskRectRight.Width = temp < 0 ? 0 : temp; // Math.Max(0, ActualWidth - selectionRegion.Right);

                if (_MaskRectRight.Height != actualHeight)
                    _MaskRectRight.Height = actualHeight;

                SetLeft(_MaskRectTop, _MaskRectLeft.Width);
                SetLeft(_MaskRectBottom, _MaskRectLeft.Width);

                temp = actualWidth - _MaskRectLeft.Width - _MaskRectRight.Width;
                if (temp != _MaskRectTop.Width)
                    _MaskRectTop.Width = temp < 0 ? 0 : temp;   // Math.Max(0, ActualWidth - maskRectLeft.Width - maskRectRight.Width);

                temp = _SelectionRegion.Top;
                if (temp != _MaskRectTop.Height)
                    _MaskRectTop.Height = temp < 0 ? 0 : temp;  // Math.Max(0, selectionRegion.Top);

                _MaskRectBottom.Width = _MaskRectTop.Width;

                temp = actualHeight - _SelectionRegion.Bottom;
                if (temp != _MaskRectBottom.Height)
                    _MaskRectBottom.Height = temp < 0 ? 0 : temp;   // Math.Max(0, ActualHeight - selectionRegion.Bottom);
            }
        }

        #region 鼠标管理

        private bool IsMouseOnThis(RoutedEventArgs e)
        {
            return e.Source.Equals(this) || e.Source.Equals(_MaskRectLeft) ||
                e.Source.Equals(_MaskRectRight) || e.Source.Equals(_MaskRectTop) ||
                e.Source.Equals(_MaskRectBottom);
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            // 鼠标在区域中按下
            if (IsMouseOnThis(e))
                PrepareShowMask(Mouse.GetPosition(this));

            // 鼠标按下标识符
            else if (e.Source.Equals(_Indicator))
            {
                HandleIndicatorMouseDown(e);
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsMouseOnThis(e))
            {
                UpdateSelectionRegion(e, UpdateMaskType.ForMouseMoving);

                e.Handled = true;
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (IsMouseOnThis(e))
            {
                UpdateSelectionRegion(e, UpdateMaskType.ForMouseLeftButtonUp);

                FinishShowMask();
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            _Indicator.Visibility = Visibility.Collapsed;
            _SelectionRegion = Rect.Empty;
            _SelectionBorder.Width = _SelectionBorder.Height = 0;
            ClearSelectionData();
            UpdateMaskRectanglesLayout();

            base.OnMouseRightButtonUp(e);
        }

        internal void HandleIndicatorMouseDown(MouseButtonEventArgs e)
        {
            if (2 <= e.ClickCount)
            {
                if (null != MaskWindowOwner)
                {
                    MaskWindowOwner.ClipSnapshot(GetIndicatorRegion());
                    ClearSelectionData();
                }
            }
        }

        private void PrepareShowMask(Point mouseLoc)
        {
            _Indicator.Visibility = Visibility.Collapsed;
            _SelectionBorder.Visibility = Visibility.Visible;
            _SelectionStartPoint = new Point?(mouseLoc);

            if (!IsMouseCaptured)
                CaptureMouse();
        }

        private void UpdateSelectionRegion(MouseEventArgs e, UpdateMaskType updateType)
        {
            if (UpdateMaskType.ForMouseMoving == updateType && MouseButtonState.Pressed != e.LeftButton)
                _SelectionStartPoint = null;

            if (_SelectionStartPoint.HasValue)
            {
                _SelectionEndPoint = e.GetPosition(this);

                var startPoint = (Point)_SelectionEndPoint;
                var endPoint = (Point)_SelectionStartPoint;
                var sX = startPoint.X;
                var sY = startPoint.Y;
                var eX = endPoint.X;
                var eY = endPoint.Y;

                var deltaX = eX - sX;
                var deltaY = eY - sY;

                if (Math.Abs(deltaX) >= SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(deltaX) >= SystemParameters.MinimumVerticalDragDistance)
                {
                    _IsMaskDraging = true;

                    double x = sX < eX ? sX : eX;   // Math.Min(sX, eX);
                    double y = sY < eY ? sY : eY;   // Math.Min(sY, eY);
                    double w = deltaX < 0 ? -deltaX : deltaX;   // Math.Abs(deltaX);
                    double h = deltaY < 0 ? -deltaY : deltaY;   // Math.Abs(deltaY);

                    _SelectionRegion = new Rect(x, y, w, h);
                }
                else
                {
                    if (DefaultSize.HasValue && updateType == UpdateMaskType.ForMouseLeftButtonUp)
                    {
                        _IsMaskDraging = true;

                        _SelectionRegion = new Rect(startPoint.X, startPoint.Y, DefaultSize.Value.Width, DefaultSize.Value.Height);
                    }
                    else
                        _IsMaskDraging = false;
                }
            }
        }

        internal void UpdateSelectionRegion(Rect region)
        {
            _SelectionRegion = region;
        }

        private void FinishShowMask()
        {
            if (IsMouseCaptured)
                ReleaseMouseCapture();

            if (_IsMaskDraging)
            {
                if (null != MaskWindowOwner)
                    MaskWindowOwner.OnShowMaskFinished(_SelectionRegion);

                UpdateIndicator(_SelectionRegion);
                ClearSelectionData();
            }
        }

        private void ClearSelectionData()
        {
            _IsMaskDraging = false;
            _SelectionBorder.Visibility = Visibility.Collapsed;
            _SelectionStartPoint = null;
            _SelectionEndPoint = null;
        }

        private void UpdateIndicator(Rect region)
        {
            if (region.Width < _Indicator.MinWidth || region.Height < _Indicator.MinHeight)
                return;

            _Indicator.Width = region.Width;
            _Indicator.Height = region.Height;
            SetLeft(_Indicator, region.Left);
            SetTop(_Indicator, region.Top);

            _Indicator.Visibility = Visibility.Visible;
        }

        private Rect GetIndicatorRegion()
        {
            return new Rect(GetLeft(_Indicator), GetTop(_Indicator), _Indicator.ActualWidth, _Indicator.ActualHeight);
        }

        #endregion

        #region Render

        private void OnCompositionTargetRendering(object sender, EventArgs e)
        {
            UpdateSelectionBorderLayout();
            UpdateMaskRectanglesLayout();
        }

        #endregion

        #region inner types

        private enum UpdateMaskType
        {
            ForMouseMoving,
            ForMouseLeftButtonUp
        }

        #endregion
    }
}
