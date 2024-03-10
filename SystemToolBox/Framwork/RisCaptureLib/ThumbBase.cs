using System.Windows.Controls.Primitives;

namespace SystemToolBox.Framwork.RisCaptureLib
{
    class ThumbBase : Thumb
    {
        /// <summary>
        /// 获取可拖动控件被创建时的<see cref="IndicatorObject"/>对象
        /// </summary>
        public IndicatorObject Target { get; private set; }

        public ThumbBase()
        {
            FocusVisualStyle = null;
            DragStarted += OnDragStarted;
            DragDelta += OnDragDelta;
            DragCompleted += OnDragCompleted;
        }

        // 通常，你应该重写这个方法来做一些清理工作。
        // 将推送当前操作添加到撤销重做管理器中
        protected virtual void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {

        }

        // 通常，您应该重写此方法进行拖动
        protected virtual void OnDragDelta(object sender, DragDeltaEventArgs e)
        {

        }

        // 通常情况下，你应该重写这个方法来做一些准备工作来拖动
        protected virtual void OnDragStarted(object sender, DragStartedEventArgs e)
        {

        }

        protected override void OnVisualParentChanged(System.Windows.DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            Target = this.GetAncestor<IndicatorObject>();
        }
    }
}
