namespace SystemToolBox.ViewModel
{
    public class TotalDataContext
    {
        // 基本操作
        public BasicCtrolViewModel BasicCtrolViewModel { get; set; }
        // 进程操作
        public ProcessCtrolViewModel ProcessCtrolViewModel { get; set; }
        // 截图操作
        public CaptureScreenViewModel CaptureScreenViewModel { get; set; }
        // 网络工具
        public NetwordToolViewModel NetwordToolViewModel { get; set; }
    }
}
