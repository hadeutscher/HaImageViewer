using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
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

            var args = Environment.GetCommandLineArgs();
            if ( args.Length > 2)
            {
                new MainWindow(args[1], args.Skip(2).ToList()).ShowDialog();
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
    }
}
