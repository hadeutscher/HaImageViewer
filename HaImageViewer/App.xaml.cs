using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace HaImageViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var args = Environment.GetCommandLineArgs();
            if ( args.Length > 2)
            {
                new MainWindow(args[1], args.Skip(2).ToList()).ShowDialog();
            }
            else if (args.Length == 2 && args[1] == "delete-missing")
            {
                Database.Get().DeleteMissing();
            }
            else if (args.Length == 2)
            {
                new MainWindow(args[1]).ShowDialog();
            }
            else
            {
                var dialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true
                };
                CommonFileDialogResult result = dialog.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    new MainWindow(dialog.FileName).ShowDialog();
                }
            }
            Database.Get().Save();
            this.Shutdown();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            AttachConsole(-1);
            Console.WriteLine(ex?.Message);
            Console.WriteLine(ex?.StackTrace);
        }
        
        [DllImport("Kernel32.dll")]
        public static extern bool AttachConsole(int processId);
    }
}
