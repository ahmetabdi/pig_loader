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
 

            public void Run(string processName, string friendlyName, string externalInjectorPath, string externalDllPath)
            {
                //Console.Write("{0} {1} {2} {3}", processName, friendlyName, externalInjectorPath, externalDllPath);
                string subTmpFolderName = "pig"; // DO NOT CHANGE
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

                // Create a pig folder if it doesn't already exist in tmp folder
                System.IO.Directory.CreateDirectory(Path.GetTempPath() + subTmpFolderName);

                // If no process is found at this point, tell the user and do not continue
                //Console.WriteLine(externalInjectorPath);
                //Console.WriteLine(externalDllPath);
                //Console.WriteLine(Path.GetTempPath());

                if (process == null)
                {
                    MessageBox.Show(String.Format("{0} could not be found, ensure it is running and you running launcher as admin!", friendlyName));
                }
                else
                {
                    // Process found

                    // Download Injector if it doesn't exist
                    string externalInjectorDownloadLocation = String.Format("http://www.pighack.com{0}", externalInjectorPath);
                    string injectorPath = String.Format("{0}/{1}/injector.exe", Path.GetTempPath(), subTmpFolderName);

                    if (File.Exists(injectorPath))
                    {
                        Console.WriteLine("Injector already exists we don't need to download it");
                    }
                    else
                    {
                        Console.WriteLine("Injector does not exist in tmp folder create it");
                        var webClient = new WebClient();
                        webClient.DownloadFile(new Uri(externalInjectorDownloadLocation), injectorPath);
                        webClient.Dispose();
                        Console.WriteLine("Injector download complete");
                    }

                    // Download DLL
                    string externalDllDownloadLocation = String.Format("http://www.pighack.com{0}", externalDllPath);
                    string dllPath = String.Format("{0}/{1}/PigDll.dll", Path.GetTempPath(), subTmpFolderName);

                    Console.WriteLine("Download DLL");
                    var webClientDll = new WebClient();
                    webClientDll.DownloadFile(new Uri(externalDllDownloadLocation), dllPath);
                    webClientDll.Dispose();
                    Console.WriteLine("Finish download DLL");

                    Console.WriteLine("Run injection");
                    // Run injector against found process ID with DLL downloaded
                    Process new_process = new Process();
                    new_process.StartInfo.FileName = injectorPath;
                    new_process.StartInfo.Arguments = String.Format("{0} {1}", process.Id, dllPath);
                    new_process.StartInfo.UseShellExecute = false;
                    new_process.StartInfo.CreateNoWindow = true;
                    new_process.StartInfo.RedirectStandardOutput = true;
                    new_process.StartInfo.Verb = "runas";
                    new_process.Start();

                    StreamReader reader = new_process.StandardOutput;
                    string output = reader.ReadToEnd();
                    // Write the redirected output to this application's window.
                    Console.WriteLine(output);
                    new_process.WaitForExit();
                    new_process.Close();
                    Console.WriteLine("END Run injection");

                    // Finally delete the DLL from the temp folder
                    Console.WriteLine("DELETE DLL");
                    File.Delete(dllPath);
                    MessageBox.Show(String.Format("{0} found!", friendlyName));
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            webBrowser.ObjectForScripting = new ScriptManager();
        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}

