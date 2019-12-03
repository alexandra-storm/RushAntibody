using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Reflection;
using System.Windows.Threading;

namespace WixSetupUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static public string MsiFile { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            App.DoEvents();
            byte[] msiData = WixSetupUI.Properties.Resources.MATCHITRUSH_Export_UtilitySetup;
            MsiFile = Path.Combine(Path.GetTempPath(), "MATCHITRUSH_Export_UtilitySetup.msi");
            File.WriteAllBytes(MsiFile, msiData);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.Load(WixSetupUI.Properties.Resources.WixSharp_Msi);
        }

        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }
    }
}
