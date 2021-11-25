using System;
using System.Windows.Forms;

namespace ArctisBatteryPercentage
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetCompatibleTextRenderingDefault(false);
            Application.EnableVisualStyles();
            var context = new MyApplicationContext();
            Application.Run(context);
        }
    }
}