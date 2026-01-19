using System.Windows;

namespace VisionMaster.UI.Views
{
    /// <summary>
    /// MainWindow - VisionMaster风格的主界面窗口
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeComponents();
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponents()
        {
            // 设置初始状态
            StatusText.Text = "就绪";
            CameraStatus.Text = "相机: 未连接";

            // TODO: 初始化插件系统
            // TODO: 加载工作流
            // TODO: 初始化设备连接
        }
    }
}
