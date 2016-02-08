using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Diagnostics;
using System.Collections;
using System.Net;
using System.IO;
using System.Reflection;
using JLibrary.PortableExecutable;
using InjectionLibrary;

namespace PigLoader2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public partial class MainWindow : Window
    {

        [System.Runtime.InteropServices.ComVisibleAttribute(true)]
        public class ScriptManager
        {

            public void Run(string processName, string friendlyName, string externalDllPath)
            {
                Console.Write("{0} {1} {2}", processName, friendlyName, externalDllPath);
                Process process;
                switch (processName)
                {
                    case "swtor":
                        process = Process.GetProcessesByName(processName).FirstOrDefault(p => string.IsNullOrWhiteSpace(p.MainWindowTitle));
                        break;
                    default:
                        process = Process.GetProcessesByName(processName).FirstOrDefault();
                        break;
                }

                string dllLocation = String.Format("http://www.pighack.com{0}", externalDllPath);

                WebClient myWebClient = new WebClient();
                byte[] dllbytes = myWebClient.DownloadData(dllLocation);
                //byte[] dllbytes = File.ReadAllBytes(Path.Combine("C:\\", "PigDll.dll"));

                if (process == null)
                {
                    MessageBox.Show(String.Format("{0} could not be found, ensure it is running and your running launcher as admin!", friendlyName));
                }
                else
                {

                    /*
                    var injector = InjectionMethod.Create(InjectionMethodType.Standard);
                    var processId = process.Id;
                    var hModule = IntPtr.Zero;

                    using (PortableExecutable img = new PortableExecutable(dllbytes))
                        hModule = injector.Inject(img, processId);

                    if (hModule != IntPtr.Zero)
                    {
                        // injection was successful
                        MessageBox.Show("Good job");
                    }
                    else
                    {
                        // injection failed
                        if (injector.GetLastError() != null)
                            MessageBox.Show(injector.GetLastError().Message);
                    }
                    */

                    //InjectionMethod method = InjectionMethod.Create(InjectionMethodType.Standard); //InjectionMethodType.Standard //InjectionMethodType.ManualMap //InjectionMethodType.ThreadHijack
                    InjectionMethod injectionMethod = InjectionMethod.Create(InjectionMethodType.Standard);

                    IntPtr zero = IntPtr.Zero;
                    using (PortableExecutable executable = new PortableExecutable(dllbytes))
                    {
                        zero = injectionMethod.Inject(executable, process.Id);
                    }

                    if (zero != IntPtr.Zero)
                    {
                        MessageBox.Show(String.Format("{0} found!", friendlyName));
                    }
                    else if (injectionMethod.GetLastError() != null)
                    {
                        MessageBox.Show(injectionMethod.GetLastError().Message);
                    }

                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            webBrowser.ObjectForScripting = new ScriptManager();
            HideScriptErrors(webBrowser, true);
        }

        public void HideScriptErrors(WebBrowser wb, bool Hide)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser)
                .GetField("_axIWebBrowser2",
                          BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            object objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null) return;
            objComWebBrowser.GetType().InvokeMember(
                "Silent", BindingFlags.SetProperty, null, objComWebBrowser,
                new object[] { Hide });
        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}

