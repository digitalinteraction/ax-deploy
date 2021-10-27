// https://github.com/digitalinteraction/openmovement/blob/master/Downloads/Deploy/Deploy.zip?raw=true

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeployLib;
using SyncLib;

namespace Deploy
{
    public partial class MainForm : Form
    {
        private static string DOCUMENTATION_URL = "https://openlab.ncl.ac.uk/gitlab/public-pages/ax3-deploy/blob/master/README.md";
        private static string DIRECTORY_BASENAME = "AX3";
        private static string CONFIG_FILE = "config.ini";
        private static string WATCH_EXTENSION = "*.cwa";
        private static bool DEFAULT_SCAN_DEVICES = false;           // Require scan of configured devices once disconnnected
        private static bool DEFAULT_AUTO_DOWNLOAD = false;          // Automatically download devices
        private static bool DEFAULT_DELETE_AFTER_SYNC = false;      // Delete local copy if data successfully uploaded to sync server
        
        #region AutoPlay
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint RegisterWindowMessage(string lpString);

        public static uint queryCancelAutoPlayID = 0;

        protected override void WndProc(ref Message msg)
        {
            base.WndProc(ref msg);
            if (msg.Msg == MainForm.queryCancelAutoPlayID && MainForm.queryCancelAutoPlayID != 0)
            {
                msg.Result = (IntPtr)1;    // Cancel autoplay
            }
        }
        #endregion

        public class StateGroup
        {
            public Device.DeviceState DeviceState { get; protected set; }
            public string Label { get; protected set; }
            public int[] Leds { get; protected set; }
            public string Status { get; protected set; }
            public StateGroup(Device.DeviceState deviceState, string label, string status, int[] leds)
            {
                this.DeviceState = deviceState;
                this.Label = label;
                this.Leds = leds;
                this.Status = status;
            }
        }

        IDictionary<string, string> ReadConfiguration()
        {
            IDictionary<string, string> configuration = new Dictionary<string, string>();
            ISet<string> loadedFiles = new HashSet<string>();
            for (uint loops = 0; loops <= 10; loops++)
            {
                string configFile = Path.Combine(WorkingDirectory, CONFIG_FILE);
                if (loadedFiles.Contains(configFile)) { break; }
                loadedFiles.Add(configFile);
                Console.WriteLine("NOTE: Loading config: " + configFile);
                AddConfiguration(configuration, configFile);
                if (configuration.TryGetValue("workingdirectory", out string newWorkingDirectory))
                {
                    if (newWorkingDirectory.Length > 0)
                    {
                        WorkingDirectory = newWorkingDirectory;
                        Console.WriteLine("NOTE: WorkingDirectory set to: " + WorkingDirectory);
                    }
                }
                if (loops >= 10)
                {
                    Console.WriteLine("NOTE: Config files load limit reached.");
                    break;
                }
            }
            return configuration;
        }

        protected IDictionary<Device.DeviceState, StateGroup> stateGroups = new Dictionary<Device.DeviceState, StateGroup>();

        private Deployer deployer;
        protected IDictionary<string, string> configuration;
        protected bool ScanDevices { get; set; }
        protected bool AutoDownload { get; set; }
        protected string ConfigString { get; set; }
        protected int BatteryChargedLevel { get; set; }
        protected int terminateId = -1;

        protected TextBoxStreamWriter textBoxStreamWriter = null;

        public MainForm(Deployer deployer, string workingDirectory, string configString, int batteryChargedLevel)
        {
            this.deployer = deployer;

            InitializeComponent();

            // Design size seems messed up (probably design/view DPI differences)
            //this.Width = 800;
            //this.Height = 600;
            //this.WindowState = FormWindowState.Maximized;

            // Redirect console to Log view
            textBoxStreamWriter = new TextBoxStreamWriter(textBoxLog);
            textBoxStreamWriter.SetConsoleOut();
            Console.WriteLine("Started.");
            Trace.Listeners.Add(new ConsoleTraceListener());
            Trace.WriteLine("Trace started.");
            Debug.Listeners.Add(new ConsoleTraceListener());
            Debug.WriteLine("Debug trace started.");

            this.Text = this.Text + " [" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + "]";

            ConfigString = configString;

            // Use a default working directory if none is specified
            WorkingDirectory = workingDirectory;
            if (WorkingDirectory == null || WorkingDirectory.Length <= 0)
            {
                string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                WorkingDirectory = Path.Combine(documents, DIRECTORY_BASENAME);
            }
            if (!Directory.Exists(WorkingDirectory))
            {
                try
                {
                    Directory.CreateDirectory(WorkingDirectory);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ERROR: Problem trying to create directory {WorkingDirectory}: {e}");
                }
            }
            if (!Directory.Exists(WorkingDirectory))
            {
                string message = "Working directory does not exist:\r\n\r\n" + WorkingDirectory;
                MessageBox.Show(this, message, $"Error - {Application.ProductName}", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                this.Close();
            }

            // Configuration
            configuration = ReadConfiguration();

            // Title prefix
            if (configuration.TryGetValue("title", out string titlePrefix) && titlePrefix.Length > 0)
            {
                this.Text = titlePrefix + " - " + this.Text;
            }

            // Initial View/Log state from designer
            bool logOpen = !splitContainerLog.Panel2Collapsed;
            if (configuration.TryGetValue("log", out string logString)) { bool.TryParse(logString, out logOpen); }
            logToolStripMenuItem.Checked = logOpen;
            splitContainerLog.Panel2Collapsed = !logOpen;

            // Scan devices?
            bool scanDevices = DEFAULT_SCAN_DEVICES;
            if (configuration.TryGetValue("scandevices", out string scanDevicesString)) { bool.TryParse(scanDevicesString, out scanDevices); }
            ScanDevices = scanDevices;
            Console.WriteLine($"ScanDevices={scanDevices}");

            // Automatically download
            bool autoDownload = DEFAULT_AUTO_DOWNLOAD;
            if (configuration.TryGetValue("autodownload", out string autoDownloadString)) { bool.TryParse(autoDownloadString, out autoDownload); }
            AutoDownload = autoDownload;
            Console.WriteLine($"AutoDownload={autoDownload}");

            // deleteAfterSync
            bool deleteAfterSync = DEFAULT_DELETE_AFTER_SYNC;
            if (configuration.TryGetValue("delete", out string deleteAfterSyncString)) { bool.TryParse(deleteAfterSyncString, out deleteAfterSync); }
            DeleteAfterSync = deleteAfterSync;
            Console.WriteLine($"DeleteAfterSync={deleteAfterSync}");

            // batteryChargedLevel
            if (configuration.TryGetValue("batteryChargedLevel", out string batteryChargedLevelString)) { int.TryParse(batteryChargedLevelString, out batteryChargedLevel); }
            BatteryChargedLevel = batteryChargedLevel;
            Console.WriteLine($"BatteryChargedLevel={batteryChargedLevel}");

            // Can't directly set non-public double-buffer flag
            // listViewDevices.DoubleBuffered = true;
            typeof(System.Windows.Forms.Control)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(listViewDevices, true, null);

            // Cancel Windows auto-play
            queryCancelAutoPlayID = RegisterWindowMessage("QueryCancelAutoPlay");

            // Construct StateGroup map
            StateGroup sg;
            sg = new StateGroup(Device.DeviceState.STATE_CONFIGURED, "Outbox", "Configured for recording: " + (ScanDevices ? "remove, scan, dispatch." : "ready to dispatch."), new int[] { 3, 0 }); // Identify: flash cyan/off
            stateGroups.Add(sg.DeviceState, sg);
            sg = new StateGroup(Device.DeviceState.STATE_ERROR, "Error", "Problem with device communication or file download: F9 to reset; or disconnect, wait, reconnect.", new int[] { 1 }); // Error: blue
            stateGroups.Add(sg.DeviceState, sg);
            sg = new StateGroup(Device.DeviceState.STATE_NONE, "Error", "(Unknown)", new int[] { 1 }); // (Error: blue) internally invalid state
            stateGroups.Add(sg.DeviceState, sg);
            sg = new StateGroup(Device.DeviceState.STATE_PENDING, "Unexpected", "Unexpected device: set to record.", new int[] { 4 }); // Unexpected: red      // has no data and a configuration that has not yet started
            stateGroups.Add(sg.DeviceState, sg);
            sg = new StateGroup(Device.DeviceState.STATE_UNDERWAY, "Unexpected", "Unexpected device: recording in progress.", new int[] { 4 }); // Unexpected: red      // has data or a configuration that has started; and a configuration that has not yet finished (and spare capacity?)
            stateGroups.Add(sg.DeviceState, sg);
            sg = new StateGroup(Device.DeviceState.STATE_CONFIGURING, "Charged", "Configuring...", new int[] { 5 }); // Cleared and fully charged: magenta // setting a new configurtaion in progress
            stateGroups.Add(sg.DeviceState, sg);
            sg = new StateGroup(Device.DeviceState.STATE_CHARGED, "Charged", "Charged.", new int[] { 5 });        // Cleared and fully charged: magenta // cleared (has no data and no configuration) but charging has completed;
            stateGroups.Add(sg.DeviceState, sg);
            sg = new StateGroup(Device.DeviceState.STATE_RECHARGING, "Recharging", "Recharging", new int[] { 3 });     // Cleared and charging: cyan  // cleared (has no data and no configuration) but still recharging
            stateGroups.Add(sg.DeviceState, sg);
            sg = new StateGroup(Device.DeviceState.STATE_CLEARING, "Clearing", "Clearing...", new int[] { 7 });    // Downloading: white // clearing device in progress
            stateGroups.Add(sg.DeviceState, sg);
            sg = new StateGroup(Device.DeviceState.STATE_DOWNLOADED, "Downloading", "Downloaded.", new int[] { 7 });     // Downloading: white // download completed
            stateGroups.Add(sg.DeviceState, sg);
            sg = new StateGroup(Device.DeviceState.STATE_DOWNLOADING, "Downloading", "Downloading...", new int[] { 7 }); // Downloading: white // download in progress
            stateGroups.Add(sg.DeviceState, sg);
            sg = new StateGroup(Device.DeviceState.STATE_COMPLETE, "Inbox", "Recording finished.", null); // has a configuration that has finished or has data and no configuration (or has no spare capacity?)
            stateGroups.Add(sg.DeviceState, sg);
            sg = new StateGroup(Device.DeviceState.STATE_UNKNOWN, "Inbox", "Examining...", null); // device attached but not yet examined
            stateGroups.Add(sg.DeviceState, sg);

            // Start Sync Watcher
            bool syncWatcherStarted = StartSyncWatcher();

            // Start Deployer
            this.deployer.DeviceAdded += Deployer_DeviceAdded;
            this.deployer.DeviceRemoved += Deployer_DeviceRemoved;
            this.deployer.DeviceUpdated += Deployer_DeviceUpdated;
            this.deployer.Start();

            UpdateStatus();

            if (!syncWatcherStarted)
            {
                Console.WriteLine($"WARNING: No file synchronization - check configuration {CONFIG_FILE}");
                //configureControl.SetMessage($"WARNING: File synchronizer not running -- check configuration file {CONFIG_FILE} and restart.", ConfigureControl.MessageType.MESSAGE_TYPE_ERROR);
            }

            // Welcome Banner
            if (configuration.TryGetValue("welcome", out string welcomeBanner))
            {
                ConfigureControl.MessageType messageType = ConfigureControl.MessageType.MESSAGE_TYPE_INFO;
                if (welcomeBanner.StartsWith("!")) { messageType = ConfigureControl.MessageType.MESSAGE_TYPE_ERROR; welcomeBanner = welcomeBanner.Substring(1); }
                if (welcomeBanner.StartsWith("*")) { messageType = ConfigureControl.MessageType.MESSAGE_TYPE_SUCCESS; welcomeBanner = welcomeBanner.Substring(1); }
                configureControl.SetMessage(welcomeBanner, messageType);
            }

            this.ActiveControl = textBoxLog;

            if (ConfigString != null)
            {
                configureControl.Enabled = false;
                configureControl.Submit(ConfigString);
            }
        }

        public string WorkingDirectory { get; protected set; }
        public bool DeleteAfterSync { get; protected set; }

        private void Deployer_DeviceAdded(object sender, DeviceEventArgs e)
        {
            if (InvokeRequired) { this.Invoke((MethodInvoker)delegate { Deployer_DeviceAdded(sender, e); }); return; }
            if (BatteryChargedLevel > 0)
            {
                e.Device.ChargedLevel = BatteryChargedLevel;
            }
            UpdateDeviceListItem(e.Device);
        }

        private void Deployer_DeviceRemoved(object sender, DeviceEventArgs e)
        {
            if (InvokeRequired) { this.Invoke((MethodInvoker)delegate { Deployer_DeviceRemoved(sender, e); }); return; }
            UpdateDeviceListItem(e.Device, true);
        }

        private void Deployer_DeviceUpdated(object sender, DeviceEventArgs e)
        {
            try
            {
                if (this.IsDisposed) { return; }
                if (InvokeRequired) { this.Invoke((MethodInvoker)delegate { Deployer_DeviceUpdated(sender, e); }); return; }
                UpdateDeviceListItem(e.Device);
            }
            catch (ObjectDisposedException)
            {
                ;   // Ok to ignore updates after the form is disposed
            }
        }

        public void StartDownloading(Device device)
        {
            string ext = ".cwa";
            string part = ".part";
            string basename = String.Format("{0:D10}-{1:D5}", device.SessionId, device.Id);
            string filename;

            string unique;
            int uniqueNumber = 0;
            do
            {
                unique = uniqueNumber > 0 ? "_" + uniqueNumber : "";
                filename = Path.Combine(WorkingDirectory, basename + unique + ext);
                uniqueNumber++;
            } while (File.Exists(filename));

            // If it looks like QC test data, rename the file so it won't be uploaded
            if (device.IsQcData)
            {
                ext = ".qcdata" + ext;
            }

            string partFilename = filename + part;
            Console.WriteLine("DOWNLOAD: " + device.Id + ", downloading... " + partFilename + " ...to... " + filename);

            device.BeginDownloading(partFilename, filename);
        }

        // State management
        private IDictionary<int, Device.DeviceState> previousState = new Dictionary<int, Device.DeviceState>();
        int lastDeviceConfigured = -1;
        bool configuredDeviceRemoved = false;
        Configuration awaitingConfiguration = null;

        public void UpdateDeviceListItem(Device device, bool remove = false)
        {
            string key = device.Id.ToString();

            // Delete item?
            if (remove)
            {
                Console.WriteLine("Remove: " + key);
                if (listViewDevices.Items.ContainsKey(key))
                {
                    Console.WriteLine("Removed: " + key);
                    listViewDevices.Items.RemoveByKey(key);
                }

                if (device.Id == lastDeviceConfigured)
                {
                    configuredDeviceRemoved = true;
                    if (ScanDevices)
                    {
                        configureControl.SetMessage("Removed configured device #" + device.Id + ", waiting to scan device.", ConfigureControl.MessageType.MESSAGE_TYPE_INFO);
                        // lastDeviceConfigured = lastDeviceConfigured;
                    }
                    else
                    {
                        configureControl.SetMessage("Removed configured device #" + device.Id + ", ready for dispatch.", ConfigureControl.MessageType.MESSAGE_TYPE_SUCCESS);
                        lastDeviceConfigured = -1;
                        if (ConfigString != null)
                        {
                            terminateId = device.Id;
                        }
                    }
                }
                else if (lastDeviceConfigured >= 0)
                {
                    configureControl.SetMessage("ERROR: Removed device #" + device.Id + ", but was expecting removal of configured device #" + lastDeviceConfigured + ".", ConfigureControl.MessageType.MESSAGE_TYPE_ERROR);
                }
                else if (device.IsDownloading)
                {
                    configureControl.SetMessage("WARNING: Removed device #" + device.Id + " while still downloading (" + device.DownloadProgress + "%).", ConfigureControl.MessageType.MESSAGE_TYPE_ERROR);
                }
                else if (!device.IsCharged)
                {
                    configureControl.SetMessage("WARNING: Removed device #" + device.Id + " while not fully charged (" + device.Battery + "%).", ConfigureControl.MessageType.MESSAGE_TYPE_ERROR);
                }

                if (previousState.ContainsKey(device.Id))
                {
                    previousState.Remove(device.Id);
                }

                UpdateStatus();
                return;
            }

            ListViewItem item = null;
            if (listViewDevices.Items.ContainsKey(key))
            {
                // Update row
                item = listViewDevices.Items[listViewDevices.Items.IndexOfKey(key)];
            }
            else
            {
                // Add row
                item = new ListViewItem(new string[] { "-", "(id)", "(status)", "(battery)" }, listViewDevices.Groups["Unknown"]);
                item.Name = key;
                item.Tag = key;
                listViewDevices.Items.Add(item);
                item.UseItemStyleForSubItems = false;
            }

            // Group
            Device.DeviceState lastState = previousState.ContainsKey(device.Id) ? previousState[device.Id] : Device.DeviceState.STATE_NONE;
            StateGroup currentGroup = stateGroups[device.State];

            string group = currentGroup.Label;

            string status = currentGroup.Status;
            if (device.State == Device.DeviceState.STATE_ERROR && device.DownloadStatus == Device.DownloadState.DOWNLOAD_ERROR)
            {
                status = "Problem while downloading the file. You could retry: disconnect, wait, reconnect.";
            }
            else if (device.State == Device.DeviceState.STATE_DOWNLOADING)
            {
                status = status + $" ({device.DownloadProgress}%)";
            }
            else if (device.State == Device.DeviceState.STATE_PENDING)
            {
                status = status + $" (Session {device.SessionId} starting {device.StartTime:dd MMM HH:mm} for {(int)((device.StopTime - device.StartTime).TotalDays)} day(s).";
            }
            else if (device.State == Device.DeviceState.STATE_UNDERWAY)
            {
                status = status + $" (Session {device.SessionId} started {device.StartTime:dd MMM HH:mm} ends {device.StopTime:dd MMM HH:mm})";
            }


            int led = -2;
            int[] flash = null;
            if (currentGroup.Leds != null)
            {
                if (currentGroup.Leds.Length == 1)
                {
                    led = currentGroup.Leds[0];
                }
                else
                {
                    flash = currentGroup.Leds;
                }
            }

            // Triggers, only check when entering a new state
            if (device.State != lastState)
            {
                Console.WriteLine("STATE: #" + device.Id + " " + lastState + " --> " + device.State + " == " + currentGroup.Label);
                previousState[device.Id] = device.State;

                if (device.State == Device.DeviceState.STATE_ERROR)
                {
                    Console.WriteLine("TRIGGER: Device ERROR " + device.Id + "");
                    if (device.DownloadStatus == Device.DownloadState.DOWNLOAD_ERROR) { Console.WriteLine("-- device has download error"); }
                    if (device.CommsError) { Console.WriteLine("-- device has comms error"); }
                    if (!device.CommsError && device.DownloadStatus != Device.DownloadState.DOWNLOAD_ERROR) { Console.WriteLine("-- device has unknown error (not comms or download)"); }
                }
                else if (device.State == Device.DeviceState.STATE_COMPLETE && device.DownloadStatus != Device.DownloadState.DOWNLOAD_CANCELLED)
                {
                    if (AutoDownload)
                    {
                        Console.WriteLine("TRIGGER: Device COMPLETE " + device.Id + ", downloading...");
                        StartDownloading(device);
                    }
                }
                else if (device.State == Device.DeviceState.STATE_DOWNLOADED)
                {
                    Console.WriteLine("TRIGGER: Device DOWNLOADED, clearing: " + device.Id);
                    device.CommandClear();
                }
                else if (device.State == Device.DeviceState.STATE_CHARGED)
                {
                    Console.WriteLine("TRIGGER: Device CHARGED, available for configuration: " + device.Id);
                    // device.CommandConfigure(configuration);
                }
                else if (device.State == Device.DeviceState.STATE_CONFIGURED)
                {
                    Console.WriteLine("TRIGGER: Device CONFIGURED, remembering for checking scan: " + device.Id);
                    // Record ID to check against next scan
                    lastDeviceConfigured = device.Id;
                    configuredDeviceRemoved = false;
                    if (ScanDevices)
                    {
                        configureControl.SetMessage("Device configured #" + lastDeviceConfigured + " -- take flashing device and scan.", ConfigureControl.MessageType.MESSAGE_TYPE_SUCCESS);
                    }
                    else
                    {
                        configureControl.SetMessage("Device configured #" + lastDeviceConfigured + " -- take flashing device.", ConfigureControl.MessageType.MESSAGE_TYPE_SUCCESS);
                    }

                    // Notify sync
                    if (this.syncWatcher != null)
                    {
                        try
                        {
                            Console.WriteLine($"SYNCWATCHER: Registering session {device.SessionId} on #{device.Id}.");
                            this.syncWatcher.HaveProgrammedDevice(device.SessionId, device.Id);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("ERROR: Problem notifying syncwatcher of device programming: ", e);
                        }
                    }

                    awaitingConfiguration = null;
                }
            }

            // Issue change of LED (internally it won't issue a command if unchanged)
            if (flash != null)
            {
                device.FlashCode = flash;
            }
            else if (led > -2)
            {
                device.CommandLed(led);
            }

            // Determine LED details
            Color ledColor;
            string ledString;
            int ledCol = device.Led;
            switch (ledCol)
            {
                case -1: ledString = "\u26AB"; ledColor = Color.Gray; break;    //
                case 0: ledString = "\u26AB"; ledColor = Color.Black; break;    // solid black
                case 1: ledString = "\u26AB"; ledColor = Color.Blue; break;     // solid blue
                case 2: ledString = "\u26AB"; ledColor = Color.Green; break;    // solid green
                case 3: ledString = "\u26AB"; ledColor = Color.Cyan; break;     // solid cyan
                case 4: ledString = "\u26AB"; ledColor = Color.Red; break;      // solid red
                case 5: ledString = "\u26AB"; ledColor = Color.Magenta; break;  // solid magenta
                case 6: ledString = "\u26AB"; ledColor = Color.Yellow; break;   // solid yellow
                case 7: ledString = "\u26AA"; ledColor = Color.Black; break;    // hollow black =white
                default: ledString = "\u26AB"; ledColor = Color.Gray; break;    //
            }

            // Battery text is right-aligned
            string batteryText = device.IsCharged ? $"\u2713 {device.Battery}%" : (device.Battery < 0 ? "-" : $"{device.Battery}%");

            // Update list entry
            var newGroup = listViewDevices.Groups["listViewGroup" + group];
            if (newGroup != item.Group) { item.Group = newGroup; }
            if (ledString != item.SubItems[0].Text) { item.SubItems[0].Text = ledString; }
            if (item.SubItems[0].ForeColor != ledColor) { item.SubItems[0].ForeColor = ledColor; }
            if (item.SubItems[1].Text != device.Id.ToString()) { item.SubItems[1].Text = device.Id.ToString(); }
            if (item.SubItems[2].Text != batteryText) { item.SubItems[2].Text = batteryText; }
            if (item.SubItems[3].Text != status) { item.SubItems[3].Text = status; }

            UpdateStatus();
        }


        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string assemblyTitle = "";
            object[] titleAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (titleAttributes.Length > 0)
            {
                assemblyTitle = ((AssemblyTitleAttribute)titleAttributes[0]).Title;
            }
            if (assemblyTitle.Length <= 0)
            {
                assemblyTitle = System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }

            string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            string assemblyDescription = "";
            object[] descriptionAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            if (descriptionAttributes.Length > 0)
            {
                assemblyDescription = ((AssemblyDescriptionAttribute)descriptionAttributes[0]).Description;
            }

            string assemblyCopyright = "";
            object[] copyrightAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (copyrightAttributes.Length > 0)
            {
                assemblyCopyright = ((AssemblyCopyrightAttribute)copyrightAttributes[0]).Copyright;
            }

            string assemblyCompany = "";
            object[] companyAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            if (companyAttributes.Length > 0)
            {
                assemblyCompany = ((AssemblyCompanyAttribute)companyAttributes[0]).Company;
            }

            string content = "";
            content += $"{assemblyTitle} V{assemblyVersion}\r\n";
            content += $"{assemblyCompany}\r\n";
            content += $"\r\n";
            content += $"{assemblyDescription}\r\n";
            content += $"\r\n";
            content += $"{assemblyCopyright}\r\n";

            MessageBox.Show(this, content, "About " + assemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainerLog.Panel2Collapsed = !logToolStripMenuItem.Checked;
        }

        private void toolStripButtonTest_Click(object sender, EventArgs e)
        {
            deployer.Test();
        }

        private void documentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.UseWaitCursor = true;
                Process.Start(DOCUMENTATION_URL);
            }
            catch (Exception ex)
            {
                string error = "Error:\r\n\r\n" + ex.Message + "\r\n\r\nWhen opening URL:\r\n\r\n" + DOCUMENTATION_URL;
                MessageBox.Show(this, error, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.UseWaitCursor = false;
            }
        }

        // this.KeyPreview = true; - capture input first (barcode reader)
        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (ConfigString != null) return;
            bool handled = false;
            // Cancel waiting for configuration
            if (e.KeyChar == 27 && awaitingConfiguration != null)
            {
                awaitingConfiguration = null;
                configureControl.SetMessage(null);
            }
            handled |= (e.KeyChar >= 'A' && e.KeyChar <= 'Z');
            handled |= (e.KeyChar >= 'a' && e.KeyChar <= 'z');
            handled |= (e.KeyChar >= '0' && e.KeyChar <= '9');
            handled |= (e.KeyChar == 8);    // backspace
            handled |= (e.KeyChar == 13);   // enter
            handled |= (e.KeyChar == 27);   // escape
            if (handled)
            {
                // Console.WriteLine("Form.KeyPress: '" + e.KeyChar.ToString() + "' pressed.");
                configureControl.AddInput(e.KeyChar);
                e.Handled = true;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Stop SyncWatcher
            StopSyncWatcher();
            // Stop Deployer
            this.deployer.Stop();
            textBoxStreamWriter.RestoreConsoleOut();
        }

        private void listViewDevices_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private DateTime lastUpdateAvailable = DateTime.MinValue;
        private DateTime lastCheckConfigure = DateTime.MinValue;
        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            this.deployer.Update();

            // Update avaiable drive space
            if (lastUpdateAvailable == DateTime.MinValue || (DateTime.UtcNow - lastUpdateAvailable).TotalSeconds > 10)
            {
                lastUpdateAvailable = DateTime.UtcNow;
                UpdateAvailable();
            }

            if (lastCheckConfigure == DateTime.MinValue || (DateTime.UtcNow - lastCheckConfigure).TotalSeconds >= 1)
            {
                lastCheckConfigure = DateTime.UtcNow;
                if (awaitingConfiguration != null)
                {
                    string configMessage = ((awaitingConfiguration.Within == 0) ? "[WARNING: Already after start] " : "") + awaitingConfiguration.ToString();
                    Device deviceToConfigure = FindDeviceToConfigure();
                    if (HaveDevice(Device.DeviceState.STATE_CONFIGURING))
                    {
                        // (still configuring)
                        //configureControl.SetMessage("Configuring: " + configMessage, ConfigureControl.MessageType.MESSAGE_TYPE_INFO);
                    }
                    else if (deviceToConfigure == null)
                    {
                        configureControl.SetMessage("Waiting for a charged, clear device. " + configMessage, ConfigureControl.MessageType.MESSAGE_TYPE_INFO);
                    }
                    else
                    {
                        configureControl.SetMessage("Configuring #" + deviceToConfigure.Id + ": " + configMessage, ConfigureControl.MessageType.MESSAGE_TYPE_INFO);
                        deviceToConfigure.CommandConfigure(awaitingConfiguration);
                    }
                }
            }
            
            if (terminateId >= 0)
            {
                textBoxStreamWriter.RestoreConsoleOut();
                Environment.Exit(terminateId);
            }
        }

        protected bool HaveDevice(Device.DeviceState state)
        {
            lock (deployer.Devices)
            {
                foreach (Device device in deployer.Devices.Values)
                {
                    if (device.State == state)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected Device FindDeviceToConfigure()
        {
            Device bestDevice = null;
            lock (deployer.Devices)
            {
                foreach (Device device in deployer.Devices.Values)
                {
                    if (device.State == Device.DeviceState.STATE_CHARGED)
                    {
                        if (bestDevice == null || device.Battery > bestDevice.Battery)
                        {
                            bestDevice = device;
                        }
                    }
                }
            }
            return bestDevice;
        }

        private void configureControl_ConfigurationScanned(object sender, ConfigureControl.ScanEventArgs e)
        {
            if (awaitingConfiguration != null)
            {
                configureControl.SetMessage("Not expecting another configuration scan, still waiting for a device to configure.", ConfigureControl.MessageType.MESSAGE_TYPE_ERROR);
            }
            else if (lastDeviceConfigured >= 0)
            {
                if (ScanDevices)
                {
                    configureControl.SetMessage("Not expecting a new configuration, was expecting removal and a scan of the last-configured device #" + lastDeviceConfigured + ".", ConfigureControl.MessageType.MESSAGE_TYPE_ERROR);
                }
                else
                {
                    configureControl.SetMessage("Not expecting a new configuration, was expecting removal of the last-configured device #" + lastDeviceConfigured + ".", ConfigureControl.MessageType.MESSAGE_TYPE_ERROR);
                }
            }
            else
            {
                lastDeviceConfigured = -1;  // clear flag of last device configured
                awaitingConfiguration = e.Configuration;
            }
        }

        private void configureControl_DeviceScanned(object sender, ConfigureControl.ScanEventArgs e)
        {
            if (lastDeviceConfigured < 0)
            {
                configureControl.SetMessage("Not expecting a device to be scanned (scanned #" + e.DeviceId + ").", ConfigureControl.MessageType.MESSAGE_TYPE_ERROR);
            }
            else if (e.DeviceId == lastDeviceConfigured && !configuredDeviceRemoved)
            {
                configureControl.SetMessage("Not expecting device #" + lastDeviceConfigured + " to be scanned until after it is removed.", ConfigureControl.MessageType.MESSAGE_TYPE_ERROR);
            }
            else if (e.DeviceId == lastDeviceConfigured)
            {
                configureControl.SetMessage("Device matched #" + lastDeviceConfigured + " -- ready for dispatch.", ConfigureControl.MessageType.MESSAGE_TYPE_SUCCESS);
                lastDeviceConfigured = -1;
                if (ConfigString != null)
                {
                    terminateId = e.DeviceId;
                }
            }
            else
            {
                configureControl.SetMessage("Mismatched devices! Scanned #" + e.DeviceId + ", expected configured device #" + lastDeviceConfigured + ".", ConfigureControl.MessageType.MESSAGE_TYPE_ERROR);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // ScanDevices configuredDeviceRemoved
            if (lastDeviceConfigured >= 0)
            {
                var result = MessageBox.Show("You must first remove device #" + lastDeviceConfigured + ".\r\n\r\nYou cannot exit the setup tool until you remove the last configured device.  If there is a problem and you must forcefully exit, use [Tools | Clear Pending Configuration].", $"Cannot Exit {Application.ProductName}", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                e.Cancel = true;
            }
            else
            {
                var result = MessageBox.Show("Really quit?", $"Exit {Application.ProductName}", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                };
            }
        }

        private void copySelectedEntriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ListViewItem item in listViewDevices.Items)
            {
                if (item.Selected)
                {
                    StringBuilder sbLine = new StringBuilder();
                    for (int i = 0; i < item.SubItems.Count; i++)
                    {
                        if (i == 0) { continue; }  // Don't capture LED column

                        if (i != 1) { continue; }  // only capture id column

                        if (sbLine.Length > 0) sbLine.Append('\t');
                        sbLine.Append(item.SubItems[i].Text);
                    }
                    sbLine.Append("\r\n");
                    sb.Append(sbLine);
                }
            }
            if (sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Bitmap bmp = new Bitmap(this.Width, this.Height))
            {
                this.DrawToBitmap(bmp, new Rectangle(Point.Empty, bmp.Size));
                Clipboard.SetImage(bmp);
                // bmp.Save("screenshot.png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string text = Clipboard.GetText();
            if (text != null) { text = text.Trim(); }
            if (text == null || text.Length <= 0)
            {
                Console.WriteLine("PASTE: Cannot use empty text from clipboard.");
                return;
            }
            if (text.IndexOf("\n") >= 0)
            {
                Console.WriteLine("PASTE: Cannot use multiple lines from clipboard.");
                return;
            }
            if (text.Length > 500)
            {
                Console.WriteLine("PASTE: Cannot use very long text from clipboard.");
                return;
            }
            // Allow duplicates when pasting?
            // configureControl.ForgetLastInput();
            configureControl.Expire();
            foreach (char ch in text)
            {
                configureControl.AddInput(ch);
            }
            configureControl.AddInput((char)13);
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listViewDevices.BeginUpdate();
            foreach (ListViewItem item in listViewDevices.Items)
            {
                item.Selected = true;
            }
            listViewDevices.EndUpdate();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string configFile = Path.Combine(WorkingDirectory, CONFIG_FILE);

            if (!File.Exists(configFile))
            {
                string message = $"Configuration file does not exist:\r\n\r\n{configFile}\r\n\r\nYou must place a configuration file at that location.";
                MessageBox.Show(this, message, "Options", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else
            {
                string message = $"Launch an editor for the configuration file?\r\n\r\n{configFile}\r\n\r\nYou must restart the program afterwards for changes to take affect.";
                DialogResult dr = MessageBox.Show(this, message, "Options", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (dr == DialogResult.OK)
                {
                    Process.Start(configFile);
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", String.Format("/n, /e, {0}", WorkingDirectory));
        }

        public void UpdateStatus()
        {
            if (InvokeRequired) { this.Invoke((MethodInvoker)delegate { UpdateStatus(); }); return; }

            int style = 0;

            StringBuilder text = new StringBuilder();
            text.Append($"{listViewDevices.Items.Count} device(s) - SYNC: ");

            if (syncWatcher == null)
            {
                text.Append("(No file synchronization configured)");
                style = AutoDownload ? 1 : 0;
            }
            else
            {
                if (currentSyncFile == null)
                {
                    text.Append("[idle]");
                }
                else
                {
                    text.Append(currentSyncFile.Name);
                    if (syncWatcher.FailCount > 0)
                    {
                        text.Append(" - " + syncWatcher.FailCount + " failed attempt(s) (" + SyncWatcher.TimeoutForFailCount(syncWatcher.FailCount) + " second back-off) - ");
                        if (syncWatcher.FailCount > 5)
                        {
                            style = 2;
                        }
                        else
                        {
                            style = 1;
                        }
                    }
                }
                text.Append($" - queue {syncWatcher.UploadQueueLength} file(s).");
            }

            if (text.ToString() != toolStripStatusLabel.Text)
            {
                toolStripStatusLabel.Text = text.ToString();
            }

            switch (style)
            {
                case 0:
                    toolStripStatusLabel.ForeColor = SystemColors.ControlText;
                    toolStripStatusLabel.BackColor = SystemColors.Control;
                    break;
                case 1:
                    toolStripStatusLabel.ForeColor = Color.Black;
                    toolStripStatusLabel.BackColor = Color.Yellow;
                    break;
                default:
                    toolStripStatusLabel.ForeColor = Color.White;
                    toolStripStatusLabel.BackColor = Color.Red;
                    break;
            }

        }

        private void toolStripStatusLabel_Click(object sender, EventArgs e)
        {

        }

        bool AddConfiguration(IDictionary<string, string> config, string configFile)
        {
            if (File.Exists(configFile))
            {
                try
                {
                    new List<String>(File.ReadAllText(configFile).Split('\r', '\n')).ForEach(line =>
                    {
                        string[] parts = line.Split(new char[] { '=' }, 2);
                        if (parts.Length > 0 && parts[0].Trim().Length > 0 && !parts[0].Trim().StartsWith("#"))
                        {
                            config[parts[0].Trim().ToLower()] = parts.Length > 1 ? parts[1].Trim() : "";
                        }
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: Problem reading configuration.", e);
                    return false;
                }
            }
            else
            {
                Console.WriteLine("WARNING: Configuration file does not exist: ", configFile);
                return false;
            }
            return true;
        }

        #region SyncWatcher
        private SyncWatcher syncWatcher;
        private FileInfo currentSyncFile;

        protected bool StartSyncWatcher()
        {
            if (this.syncWatcher != null)
            {
                Console.WriteLine("ERROR: SyncWatcher already exists.");
                return false;
            }

            configuration.TryGetValue("aws_key", out string awsKey);
            configuration.TryGetValue("aws_secret", out string awsSecret);
            configuration.TryGetValue("aws_bucket", out string awsBucket);
            configuration.TryGetValue("aws_region", out string awsRegion);
            configuration.TryGetValue("api_url", out string apiUrl);
            configuration.TryGetValue("pre_shared_key", out string apiKey);

            if (awsBucket == null || awsBucket.Length == 0)
            {
                Console.WriteLine("WARNING: SyncWatcher not starting - AWS_BUCKET not specified. Check configuration and restart.");
                return false;
            }

            Console.WriteLine("SYNCWATCHER: Starting: " + WorkingDirectory);
            this.syncWatcher = new SyncWatcher(WorkingDirectory, WATCH_EXTENSION, awsKey, awsSecret, awsBucket, awsRegion, apiUrl, apiKey, DeleteAfterSync);
            this.syncWatcher.OnFileStartedUpload += SyncWatcher_OnFileStartedUpload;
            this.syncWatcher.OnFileFinishedUpload += SyncWatcher_OnFileFinishedUpload;
            this.syncWatcher.Start();
            return true;
        }

        protected void StopSyncWatcher()
        {
            if (this.syncWatcher != null)
            {
                this.syncWatcher.Stop();
                this.syncWatcher = null;
            }
        }

        private void SyncWatcher_OnFileStartedUpload(FileInfo obj)
        {
            currentSyncFile = obj;
            UpdateStatus();
        }

        private void SyncWatcher_OnFileFinishedUpload(FileInfo obj)
        {
            currentSyncFile = null;
            UpdateStatus();
        }
        #endregion

        private void menuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void advancedIgnoreConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message = $"Reset the status waiting for a device to be configured, \r\nwaiting for a configured device to be removed, \r\nand the memory of the last configuration?\r\n\r\nWARNING: This will cause the system to forget it is waiting for a device to configure,\nor a configured device to be removed, or may allow duplicated configurations to exist.";
            var result = MessageBox.Show(message, $"DANGER! - {Application.ProductName}", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                lastDeviceConfigured = -1;
                awaitingConfiguration = null;
                configuredDeviceRemoved = true;
                configureControl.ForgetLastInput();
                configureControl.SetMessage($"Waiting status reset.", ConfigureControl.MessageType.MESSAGE_TYPE_INFO);
            }
        }

        private void advancedSetFullChargeLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var devices = new List<Device>();
            // Find suitable devices
            foreach (ListViewItem item in listViewDevices.SelectedItems)
            {
                int deviceId  = int.Parse(item.Tag.ToString());
                Device device = deployer.Devices[deviceId];
                devices.Add(device);
            }
            if (devices.Count == 0 || devices.Count != listViewDevices.SelectedItems.Count)
            {
                MessageBox.Show($"Sorry, only {devices.Count} of {listViewDevices.SelectedItems.Count} selected device(s) are suitable.", $"{Application.ProductName}", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                return;
            }

            string message = $"Treat {devices.Count} device(s) as fully charged and ready to configure?\r\n\r\nWARNING: Logging duration could be limited.";
            var result = MessageBox.Show(message, $"DANGER! - {Application.ProductName}", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                foreach (var device in devices)
                {
                    device.ChargedLevel = 0;
                }
            }
        }

        private void advancedDownloadAndClearNonCompletedDevicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var devices = new List<Device>();
            // Find suitable devices
            foreach (ListViewItem item in listViewDevices.SelectedItems)
            {
                int deviceId = int.Parse(item.Tag.ToString());
                Device device = deployer.Devices[deviceId];
                if (device.State == Device.DeviceState.STATE_PENDING || device.State == Device.DeviceState.STATE_UNDERWAY || device.State == Device.DeviceState.STATE_COMPLETE)
                {
                    devices.Add(device);
                }
            }
            if (devices.Count == 0 || devices.Count != listViewDevices.SelectedItems.Count)
            {
                MessageBox.Show($"Sorry, only {devices.Count} of {listViewDevices.SelectedItems.Count} selected device(s) are suitable.", $"{Application.ProductName}", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                return;
            }

            string message = $"Download and clear data from {devices.Count} device(s) -- even if the recordings are not yet complete?";
            var result = MessageBox.Show(message, $"DANGER! - {Application.ProductName}", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                foreach (var device in devices)
                {
                    StartDownloading(device);
                }
            }
        }

        private void advancedWipeUnsavedRecordingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var devices = new List<Device>();
            // Find suitable devices
            foreach (ListViewItem item in listViewDevices.SelectedItems)
            {
                int deviceId = int.Parse(item.Tag.ToString());
                Device device = deployer.Devices[deviceId];
                if (device.State == Device.DeviceState.STATE_PENDING || device.State == Device.DeviceState.STATE_UNDERWAY || device.State == Device.DeviceState.STATE_COMPLETE || device.State == Device.DeviceState.STATE_CONFIGURED)
                {
                    devices.Add(device);
                }
            }
            if (devices.Count == 0 || devices.Count != listViewDevices.SelectedItems.Count)
            {
                MessageBox.Show($"Sorry, only {devices.Count} of {listViewDevices.SelectedItems.Count} selected device(s) are suitable.", $"{Application.ProductName}", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                return;
            }

            string message = $"Delete unsaved recordings from {devices.Count} device(s)?\r\n\r\nWARNING: You will delete ALL data made from {devices.Count} recording(s).";
            var result = MessageBox.Show(message, $"DANGER! - {Application.ProductName}", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                foreach (var device in devices)
                {
                    device.CommandClear();
                }
            }
        }

        private void advancedCancelDownloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var devices = new List<Device>();
            // Find suitable devices
            foreach (ListViewItem item in listViewDevices.SelectedItems)
            {
                int deviceId = int.Parse(item.Tag.ToString());
                Device device = deployer.Devices[deviceId];
                if (device.State == Device.DeviceState.STATE_DOWNLOADING)
                {
                    devices.Add(device);
                }
            }
            if (devices.Count == 0 || devices.Count != listViewDevices.SelectedItems.Count)
            {
                MessageBox.Show($"Sorry, only {devices.Count} of {listViewDevices.SelectedItems.Count} selected device(s) are suitable.", $"{Application.ProductName}", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                return;
            }

            string message = $"Cancel the download from {devices.Count} device(s)?\r\n\r\nWARNING: The data will not be downloaded for {devices.Count} recording(s).";
            var result = MessageBox.Show(message, $"DANGER! - {Application.ProductName}", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                foreach (var device in devices)
                {
                    device.CancelDownload();
                }
            }
        }

        private void advancedResetDeviceWithErrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var devices = new List<Device>();
            // Find suitable devices
            foreach (ListViewItem item in listViewDevices.SelectedItems)
            {
                int deviceId = int.Parse(item.Tag.ToString());
                Device device = deployer.Devices[deviceId];
                if (device.State == Device.DeviceState.STATE_ERROR)
                {
                    devices.Add(device);
                }
            }
            if (devices.Count == 0 || devices.Count != listViewDevices.SelectedItems.Count)
            {
                MessageBox.Show($"Sorry, only {devices.Count} of {listViewDevices.SelectedItems.Count} selected device(s) are suitable.", $"{Application.ProductName}", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                return;
            }

            string message = $"Reset {devices.Count} device(s)?\r\n\r\nNOTE: If this doesn't help, disconnect, wait, and reconnect the devices.";
            var result = MessageBox.Show(message, $"{Application.ProductName}", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                foreach (var device in devices)
                {
                    device.CommandReset();
                }
            }
        }

        private void resetDevicesWithCommsErrorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewDevices.Items)
            {
                int deviceId = int.Parse(item.Tag.ToString());
                Device device = deployer.Devices[deviceId];
                item.Selected = (device.State == Device.DeviceState.STATE_ERROR);
            }
            if (listViewDevices.SelectedItems.Count > 0)
            {
                advancedResetDeviceWithErrorToolStripMenuItem_Click(sender, e);
            }
        }

        /*
        // Remove focussed listview item when control looses focus
        private void listViewDevices_Leave(object sender, EventArgs e)
        {
            if (listViewDevices.FocusedItem != null)
            {
                listViewDevices.FocusedItem.Focused = false;
            }
        }
        */

        public static long GetAvailableSpaceFromPathDrive(string path)
        {
            // Drive info only for drive letters
            string root = Path.GetPathRoot(path);
            if (root != null && root.Length >= 3 && root[1] == ':' && root[2] == '\\')
            {
                DriveInfo info = new DriveInfo("" + root[0]);
                return info.AvailableFreeSpace;
            }
            return -1;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

        public static long GetAvailableSpace(string path)
        {
            if (path != null && !path.EndsWith("\\")) { path += '\\'; }
            if (!GetDiskFreeSpaceEx(path, out ulong freeBytesAvailable, out ulong _totalNumberOfBytes, out ulong _totalNumberOfFreeBytes))
            {
                // throw new Win32Exception(Marshal.GetLastWin32Error());
                // return -1;
                return GetAvailableSpaceFromPathDrive(path);
            }
            // Console.WriteLine(path + " - " + freeBytesAvailable + " - " + _totalNumberOfBytes + " - " + _totalNumberOfFreeBytes);
            return (long)freeBytesAvailable;
        }

        public void UpdateAvailable()
        {
            // Don't show available storage if not automatically downloading
            if (!AutoDownload)
            {
                return;
            }

            string path = WorkingDirectory;
            long available = GetAvailableSpace(path);
            int approxDownloads = -1;
            int style = 0;

            // Drive info only for drive letters
            if (available >= 0)
            {
                approxDownloads = (int)(available / (250 * 1024 * 1024));
                if (approxDownloads < 15) style = 2;
                else if (approxDownloads < 30) style = 1;
            }

            if (approxDownloads < 0)
            {
                toolStripStatusLabelAvailable.Text = "-";
            }
            else
            {
                toolStripStatusLabelAvailable.Text = "Available: ~" + approxDownloads + " download(s)";
            }

            if (available >= 0)
            {
                toolStripStatusLabelAvailable.ToolTipText = "Drive " + path + " has " + (int)(available / 1024 / 1024) + " MB free";
            }
            else
            {
                toolStripStatusLabelAvailable.ToolTipText = "-";
            }

            switch (style)
            {
                case 0:
                    toolStripStatusLabelAvailable.ForeColor = SystemColors.ControlText;
                    toolStripStatusLabelAvailable.BackColor = SystemColors.Control;
                    break;
                case 1:
                    toolStripStatusLabelAvailable.ForeColor = Color.Black;
                    toolStripStatusLabelAvailable.BackColor = Color.Yellow;
                    break;
                default:
                    toolStripStatusLabelAvailable.ForeColor = Color.White;
                    toolStripStatusLabelAvailable.BackColor = Color.Red;
                    break;
            }
        }

        private void toolStripStatusLabelAvailable_Click(object sender, EventArgs e)
        {

        }

        private void configureControl_Load(object sender, EventArgs e)
        {

        }
    }
}

