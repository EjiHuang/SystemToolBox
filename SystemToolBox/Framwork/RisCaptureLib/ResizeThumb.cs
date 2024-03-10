using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SystemToolBox.Framwork.RisCaptureLib
{
    class ResizeThumb : ThumbBase
    {
        private RotateTransform _RotateTransform;
        private Point _TransformOrigin;
        private double angle;

        public ResizeThumbPlacement Placement
        {
            get { return (ResizeThumbPlacement)GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }

        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.Register("Placement", typeof(ResizeThumbPlacement),
            typeof(ResizeThumb), new UIPropertyMetadata(ResizeThumbPlacement.None));

        protected override void OnDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (null != Target)
            {
                var canvas = VisualTreeHelper.GetParent(Target) as MaskCanvas;
                if (null != canvas)
                {
                    _TransformOrigin = Target.RenderTransformOrigin;
                    _RotateTransform = Target.GetRenderTransform<RotateTransform>();

                    if (null != _RotateTransform)
                        angle = _RotateTransform.Angle * Math.PI / 180;
                    else
                        angle = 0.0;
                }
            }
        }

        protected override void OnDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (null != Target)
            {
                var delta = new Point(e.HorizontalChange, e.VerticalChange);

                var canvas = Target.Parent as MaskCanvas;
                if (null != canvas)
                {
                    double x = Canvas.GetLeft(Target);
                    double y = Canvas.GetTop(Target);
                    double w = Target.ActualWidth;
                    double h = Target.ActualHeight;

                    // 在正常调整大小时调整delta.X
                    switch (Placement)
                    {
                        case ResizeThumbPlacement.LeftTop:
                        case ResizeThumbPlacement.LeftBottom:
                        case ResizeThumbPlacement.LeftCenter:
                            {
                                if (w - delta.X <= Target.MinWidth && 0 < delta.X)
                                    delta.X = w - Target.MinWidth;

                                if (w - delta.X >= Target.MaxWidth && 0 > delta.X)
                                    delta.X = w - Target.MaxWidth;
                            }
                            break;
                        default:
                            break;
                    }

                    // 在正常调整大小时调整delta.Y
                    switch (Placement)
                    {
                        case ResizeThumbPlacement.LeftTop:
                        case ResizeThumbPlacement.RightTop:
                        case ResizeThumbPlacement.TopCenter:
                            {
                                if (h - delta.Y <= Target.MinHeight && 0 < delta.Y)
                                    delta.Y = h - Target.MinHeight;
                                if (h - delta.Y >= Target.MaxHeight && 0 > delta.Y)
                                    delta.Y = h - Target.MaxHeight;
                            }
                            break;
                        default:
                            break;
                    }

                    // 调整delta.X时，当旋转了后重新调整大小
                    switch (Placement)
                    {
                        case ResizeThumbPlacement.RightTop:
                        case ResizeThumbPlacement.RightBottom:
                        case ResizeThumbPlacement.RightCenter:
                            {
                                if (w + delta.X <= Target.MinWidth && delta.X < 0)
                                    delta.X = Target.MinWidth - w;

                                if (w + delta.X >= Target.MaxWidth && delta.X > 0)
                                    delta.X = Target.MaxWidth - w;
                            }
                            break;
                        default:
                            break;
                    }

                    // 调整delta.Y时，当旋转了后重新调整大小
                    switch (Placement)
                    {
                        case ResizeThumbPlacement.LeftBottom:
                        case ResizeThumbPlacement.RightBottom:
                        case ResizeThumbPlacement.BottomCenter:
                            {
                                if (h + delta.Y <= Target.MinHeight && delta.Y < 0)
                                {
                                    delta.Y = Target.MinHeight - h;
                                }

                                if (h + delta.Y >= Target.MaxHeight && delta.Y > 0)
                                {
                                    delta.Y = Target.MaxHeight - h;
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    switch (Placement)
                    {
                        case ResizeThumbPlacement.LeftTop:
                            {
                                x += delta.Y * Math.Sin(-angle) - _TransformOrigin.Y * delta.Y * Math.Sin(-angle);
                                y += delta.Y * Math.Cos(-angle) + _TransformOrigin.Y * delta.Y * (1 - Math.Cos(-angle));

                                x += delta.X * Math.Cos(angle) + _TransformOrigin.X * delta.X * (1 - Math.Cos(angle));
                                y += delta.X * Math.Sin(angle) - _TransformOrigin.X * delta.X * Math.Sin(angle);

                                w -= delta.X;
                                h -= delta.Y;
                            }
                            break;
                        case ResizeThumbPlacement.TopCenter:
                            {
                                x += delta.Y * Math.Sin(-angle) - _TransformOrigin.Y * delta.Y * Math.Sin(-angle);
                                y += delta.Y * Math.Cos(-angle) + _TransformOrigin.Y * delta.Y * (1 - Math.Cos(-angle));

                                h -= delta.Y;
                            }
                            break;
                        case ResizeThumbPlacement.RightTop:
                            {
                                x += delta.Y * Math.Sin(-angle) - _TransformOrigin.Y * delta.Y * Math.Sin(-angle);
                                y += delta.Y * Math.Cos(-angle) + _TransformOrigin.Y * delta.Y * (1 - Math.Cos(-angle));

                                x -= delta.X * _TransformOrigin.X * (1 - Math.Cos(angle));
                                y += delta.X * _TransformOrigin.X * Math.Sin(angle);

                                w += delta.X;
                                h -= delta.Y;
                            }
                            break;
                        case ResizeThumbPlacement.RightCenter:
                            {
                                x -= delta.X * _TransformOrigin.X * (1 - Math.Cos(angle));
                                y += delta.X * _TransformOrigin.X * Math.Sin(angle);

                                w += delta.X;
                            }
                            break;
                        case ResizeThumbPlacement.RightBottom:
                            {
                                x += delta.Y * _TransformOrigin.Y * Math.Sin(-angle);
                                y -= delta.Y * _TransformOrigin.Y * (1 - Math.Cos(-angle));

                                x -= delta.X * _TransformOrigin.X * (1 - Math.Cos(angle));
                                y += delta.X * _TransformOrigin.X * Math.Sin(angle);

                                w += delta.X;
                                h += delta.Y;
                            }
                            break;
                        case ResizeThumbPlacement.BottomCenter:
                            {
                                x += delta.Y * _TransformOrigin.Y * Math.Sin(-angle);
                                y -= delta.Y * _TransformOrigin.Y * (1 - Math.Cos(-angle));

                                h += delta.Y;
                            }
                            break;
                        case ResizeThumbPlacement.LeftBottom:
                            {
                                x += delta.Y * _TransformOrigin.Y * Math.Sin(-angle);
                                y -= delta.Y * _TransformOrigin.Y * (1 - Math.Cos(-angle));

                                x += delta.X * Math.Cos(angle) + _TransformOrigin.X * delta.X * (1 - Math.Cos(angle));
                                y += delta.X * Math.Sin(angle) - _TransformOrigin.X * delta.X * Math.Sin(angle);

                                w -= delta.X;
                                h += delta.Y;
                            }
                            break;
                        case ResizeThumbPlacement.LeftCenter:
                            {
                                x += delta.X * Math.Cos(angle) + _TransformOrigin.X * delta.X * (1 - Math.Cos(angle));
                                y += delta.X * Math.Sin(angle) - _TransformOrigin.X * delta.X * Math.Sin(angle);

                                w -= delta.X;
                            }
                            break;
                        default:
                            break;
                    }

                    if (x.IsNormalNumber() && y.IsNormalNumber() && w.IsNormalNumber() && h.IsNormalNumber())
                    {
                        w = Math.Min(Target.MaxWidth, Math.Max(Target.MinWidth, w));
                        h = Math.Min(Target.MaxHeight, Math.Max(Target.MinHeight, h));

                        var rect = new Rect(x, y, w, h);

                        Target.Resize(rect);
                    }
                }
            }
        }

        protected override void OnDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

    }
}
