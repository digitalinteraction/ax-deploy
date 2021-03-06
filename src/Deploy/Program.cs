using System;
using System.Security.Permissions;
using System.Windows.Forms;
using DeployLib;
using System.Threading;

namespace Deploy
{
    static class Program
    {
        // Single instance of the application
        static Mutex mutex = new Mutex(true, "OmDeployUi");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        static void Main(string[] args)
        {
            try
            {
                // Exception handling
                Application.ThreadException += Application_ThreadException;                         // UI thread exceptions
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);       // Force UI exceptions through our handler
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;     // non-UI thread exceptions

                string applicationBinary = (new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath;

                // Run the application
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Command-line options
                string workingDirectory = null;
                string configString = null;
                int batteryChargedLevel = -1;
                int positional = 0;
                bool help = false;
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].ToLower() == "--help") { help = true; }
                    else if (args[i].ToLower() == "--config") { configString = args[++i]; }
                    else if (args[i].ToLower() == "--battery") { batteryChargedLevel = int.Parse(args[++i]); }
                    else if (args[i][0] == '-' || args[i][0] == '/')
                    {
                        string error = "ERROR: Ignoring unknown option: " + args[i];
                        Console.Error.WriteLine(error);
                        MessageBox.Show(null, error, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        if (positional == 0) { workingDirectory = args[i]; }
                        else
                        {
                            string error = "ERROR: Ignoring positional parameter #" + (positional + 1) + ": " + args[i];
                            Console.Error.WriteLine(error);
                            MessageBox.Show(null, error, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        positional++;
                    }
                }

                if (help)
                {
                    string info = "[--config config-string] [--battery battery-level] [working-directory]";
                    MessageBox.Show(null, info, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
//MessageBox.Show(null, applicationBinary, "App", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Check this is the only instance
                    if (!mutex.WaitOne(TimeSpan.Zero, true))
                    {
                        if (configString != null)
                        {
                            Environment.Exit(0);
                        }
                        MessageBox.Show(null, "You are already running this program.", "Already Running", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Deployer deployer = Deployer.Instance;
                    MainForm mainForm = new MainForm(deployer, workingDirectory, configString, batteryChargedLevel);
                    Application.Run(mainForm);
                }
            }
            catch (Exception ex)
            {
                string error =
                    "Sorry, a fatal application error occurred (exception in main function).\r\n\r\n" +
                    "Exception: " + ex.ToString() + "\r\n\r\n" +
                    "Stack trace: " + ex.StackTrace + "";
                MessageBox.Show(error, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        // Unhandled UI exceptions (can ignore and resume)
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs t)
        {
            DialogResult result = DialogResult.Abort;
            try
            {
                Exception ex = (Exception)t.Exception;
                string error =
                    "Sorry, an application error occurred (unhandled UI exception).\r\n\r\n" +
                    "Exception: " + ex.ToString() + "\r\n\r\n" +
                    "Stack trace: " + ex.StackTrace + "";
                result = MessageBox.Show(error, Application.ProductName, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
            }
            catch {; }
            finally
            {
                if (result == DialogResult.Abort) { Application.Exit(); }
            }
        }

        // Unhandled non-UI exceptions (cannot prevent the application from terminating)
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;
                string error =
                    "Sorry, a fatal application error occurred (unhandled non-UI exception).\r\n\r\n" +
                    "Exception: " + ex.ToString() + "\r\n\r\n" +
                    "Stack trace: " + ex.StackTrace + "";
                MessageBox.Show(error, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch {; }
            finally
            {
                Environment.Exit(-1);       // Not Application.Exit, this will prevent the Windows error message
            }
        }

    }
}
