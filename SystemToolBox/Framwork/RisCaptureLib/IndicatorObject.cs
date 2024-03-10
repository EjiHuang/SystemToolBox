using System.Windows;
using System.Windows.Controls;

namespace SystemToolBox.Framwork.RisCaptureLib
{
    internal class IndicatorObject : ContentControl
    {
        private MaskCanvas _CanvasOwner;

        public IndicatorObject(MaskCanvas canvasOwner)
        {
            this._CanvasOwner = canvasOwner;
        }

        static IndicatorObject()
        {
            var ownerType = typeof(IndicatorObject);

            FocusVisualStyleProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(null));
            DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
            MinWidthProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(5.0));
            MinHeightProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(5.0));
        }

        public void Resize(Rect region)
        {
            Canvas.SetLeft(this, region.X);
            Canvas.SetTop(this, region.Y);

            Width = region.Width;
            Height = region.Height;

            _CanvasOwner.UpdateSelectionRegion(region);
        }

        public void Move(Point offset)
        {
            var x = Canvas.GetLeft(this) + offset.X;
            var y = Canvas.GetTop(this) + offset.Y;
            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);

            _CanvasOwner.UpdateSelectionRegion(new Rect(x, y, Width, Height));
        }
    }
}
