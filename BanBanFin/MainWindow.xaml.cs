using System.Windows;
using BanBanFin.Controllers;

namespace BanBanFin;

public partial class MainWindow : Window
{
    private readonly MpvPlayer _player;
    private readonly Panel     _mpvPanel;

    public MainWindow()
    {
        InitializeComponent();

        // 1. 创建 WinForms Panel（有 HWND）
        _mpvPanel      = new Panel();
        _mpvPanel.Dock = DockStyle.Fill;

        // 2. 将 WinForms Panel 添加到 WindowsFormsHost
        MpvWindow.Child = _mpvPanel;

        // 3. 初始化 Mpv 播放器
        _player = new MpvPlayer();
        InitMpvPlayer();
    }

    private void InitMpvPlayer()
    {
        // 4. 获取 Panel 的句柄作为 mpv 渲染目标
        var hwnd = _mpvPanel.Handle;

        // 5. 初始化播放器，传递 HWND
        _player.Init(hwnd, processCommandLine : false);

        // 6. 加载视频文件示例
        //_player.LoadFiles(new[] { @"D:\Downloads\真盖塔 世界最后之日 E01.mkv" }, loadFolder : false, append : false);
    }

    // 7. 窗口关闭时销毁播放器
    protected override void OnClosed(EventArgs e)
    {
        _player?.Destroy();
        base.OnClosed(e);
    }
}