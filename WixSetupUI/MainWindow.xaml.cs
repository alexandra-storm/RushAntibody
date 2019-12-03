using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WixSetupUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MyProductSetup mySetup { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        void Install_Click(object sender, RoutedEventArgs e)
        {
            uxBtInstall.IsEnabled = false;
            mySetup.StartInstall();
        }

        void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            uxBtUninstall.IsEnabled = false;
            mySetup.StartUninstall();
        }

        public void InUiThread(Action action)
        {
            if (this.Dispatcher.CheckAccess())
                action();
            else
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            mySetup.CancelRequested = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mySetup = new MyProductSetup(App.MsiFile);
            mySetup.InUiThread = this.InUiThread;

            DataContext = mySetup;
        }

        private void ShowLog_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(mySetup.LogFile);
        }
    }
}
