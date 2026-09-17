using System.Windows.Forms;

namespace CClean.UI;

public static class NotificationService
{
    public static void ShowBalloon(string title, string message)
    {
        using var icon = new NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Information,
            Visible = true,
            BalloonTipTitle = title,
            BalloonTipText = message,
            BalloonTipIcon = ToolTipIcon.Info
        };

        using var closeTimer = new System.Windows.Forms.Timer { Interval = 6000 };
        closeTimer.Tick += (_, _) =>
        {
            closeTimer.Stop();
            icon.Visible = false;
            Application.ExitThread();
        };

        icon.ShowBalloonTip(5000);
        closeTimer.Start();
        Application.Run();
    }
}