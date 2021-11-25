using Arctis;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ArctisBatteryPercentage;

class MyApplicationContext : ApplicationContext
{
    private NotifyIcon TrayIcon;
    private System.Threading.Timer _timer;
    private bool _balloonShown = false;
    private ArctisLibrary _arctis;
    private int _oldBatteryLevel = 0;

    public MyApplicationContext()
    {
        Application.ApplicationExit += new EventHandler(OnApplicationExit);
        InitializeComponent();
        TrayIcon.Visible = true;
    }

    private void InitializeComponent()
    {
        TrayIcon = new NotifyIcon
        {
            BalloonTipIcon = ToolTipIcon.Info,
            BalloonTipTitle = "Battery Level Low",
            BalloonTipText = "Battery Level is at 20%. Recharge Soon.",
        };
        CreateTextIcon(-1);


        var TrayIconConextMenu = new ContextMenuStrip();
        var CloseMenuItem = new ToolStripMenuItem();
        TrayIconConextMenu.SuspendLayout();

        TrayIconConextMenu.Items.AddRange(new ToolStripMenuItem[] { CloseMenuItem });
        TrayIconConextMenu.Name = "TrayIconContextMenu";
        TrayIconConextMenu.Size = new Size(153, 70);

        CloseMenuItem.Name = "CloseMenuItem";
        CloseMenuItem.Size = new Size(152, 22);
        CloseMenuItem.Text = "Close";
        CloseMenuItem.Click += new EventHandler(CloseMenuItem_Click);

        TrayIconConextMenu.ResumeLayout();


        TrayIcon.ContextMenuStrip = TrayIconConextMenu;

        _arctis = new ArctisLibrary();

        _timer = new System.Threading.Timer(Timer_Elapsed, null, 0, 10000);
    }

    private void Timer_Elapsed(object? state)
    {

        var batteryLevel = _arctis.CheckBattery();
        if (batteryLevel != _oldBatteryLevel)
        {
            _oldBatteryLevel = batteryLevel;
            UpdateIcon(batteryLevel);
        }
    }

    private void UpdateIcon(int batteryLevel)
    {
        CreateTextIcon(batteryLevel);

        if (batteryLevel == 0)
        {
            TrayIcon.Text = "Device not powered on.";
        }
        else
        {
            TrayIcon.Text = "Device powered on.";
        }

        if (!_balloonShown && batteryLevel != 0 && batteryLevel <= 20)
        {
            TrayIcon.ShowBalloonTip(1000);
            _balloonShown = true;
        }
    }

    private void CloseMenuItem_Click(object? sender, EventArgs e)
    {
        Application.Exit();
    }

    private void OnApplicationExit(object? sender, EventArgs e)
    {
        TrayIcon.Visible = false;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    extern static bool DestroyIcon(IntPtr handle);

    public void CreateTextIcon(int value)
    {
        var color = value switch
        {
            <= 0 => Color.White,
            _ => Color.FromArgb(CalculateRed(value), CalculateGreen(value), 0)
        };

        using var fontToUse = new Font("Microsoft Sans Serif", value == 100 ? 10 : 16, FontStyle.Regular, GraphicsUnit.Pixel);
        using var brushToUse = new SolidBrush(color);
        using var bitmapText = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bitmapText);
        g.Clear(Color.Transparent);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
        g.DrawString(value <= 0 ? "?" : value.ToString(), fontToUse, brushToUse, -4, -2);

        var hIcon = bitmapText.GetHicon();
        using var icon = Icon.FromHandle(hIcon);
        var oldIcon = TrayIcon.Icon;

        TrayIcon.Icon = icon;
        if (oldIcon != null)
        {
            DestroyIcon(oldIcon.Handle);
        }

        static int CalculateRed(int value)
        {
            return (int)((value > 50 ? 1 - 2 * (value - 50) / 100.0 : 1.0) * 255);
        }

        static int CalculateGreen(int value)
        {
            return (int)((value > 50 ? 1.0 : 2 * value / 100.0) * 255);
        }
    }
}
