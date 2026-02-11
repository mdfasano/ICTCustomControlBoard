using IctCustomControlBoard;
using System;
using System.Windows.Forms;

namespace IctControlPanel
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new BoardForm();
            Application.Run(form);
        }
    }
}
