using System;
using System.Windows;

namespace krasotkaDBLevonTEST
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            Application app = new Application();
            app.Run(new MainWindow());
        }
    }
}