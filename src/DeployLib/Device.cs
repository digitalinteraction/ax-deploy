using System;
using System.Collections.Concurrent;
using System.Threading;
using OmApiNet;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace DeployLib
{
    public class Device : INotifyPropertyChanged, IDisposable
    {
        public bool DeleteOnFail { get; set; } = true;

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            // On any property change, update the state
            state = DetermineState();
            if (disposed) return;   // ignore property changes once we're disposed
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
        public struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)] public int wFunc;
            public string pFrom;
            public string pTo;
            public short fFlags;
            [MarshalAs(UnmanagedType.Bool)] public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

        const int FO_DELETE = 3;
        const int FOF_ALLOWUNDO = 0x40;
        const int FOF_NOCONFIRMATION = 0x10;    //Don't prompt the user.;

        public static void RecycleFile(string filename)
        {
            try
            {
                SHFILEOPSTRUCT shf = new SHFILEOPSTRUCT();
                shf.wFunc = FO_DELETE;
                shf.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION;
                shf.pFrom = filename;
                SHFileOperation(ref shf);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Problem recycling file -- deleting anyway: " + e);
                try
                {
                    File.Delete(filename);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: Problem deleting file -- deleting anyway: " + ex);
                }

            }
        }


        // DeviceCommand
        public class DeviceCommand
        {
            public enum CommandType
            {
                COMMAND_INITIALIZE,
                COMMAND_BATTERY,
                COMMAND_LED,
                COMMAND_CLEAR,
                COMMAND_CONFIGURE,
                COMMAND_RESET,
            }

            public CommandType Command { get; protected set; }
            public Configuration Configuration { get; protected set; }
            public int Value { get; protected set; }

            internal DeviceCommand(CommandType command)
            {
                this.Command = command;
            }

            internal DeviceCommand(CommandType command, Configuration configuration = null) : this(command)
            {
                this.Configuration = configuration;
            }

            internal DeviceCommand(CommandType command, int value) : this(command)
            {
                this.Value = value;
            }

            public override string ToString()
            {
                return "<CMD:" + Command + ">";
            }
        }

        private int flashIndex = 0;
        private int[] flashCode;
        public int[] FlashCode
        {
            get
            {
                return flashCode;
            }
            set
            {
                flashCode = value;
            }
        }

        // Asynchrounous command queue
        private Thread thread;
        private volatile bool quit;
        private CancellationTokenSource cancellationSource;
        private BlockingCollection<DeviceCommand> commandQueue;

        public Device(int deviceId)
        {
            this.id = deviceId;

            IsInitializing = true;

            // Start command-processing thread
            quit = false;
            commandQueue = new BlockingCollection<DeviceCommand>();
            thread = new Thread(ThreadStart);
thread.IsBackground = true; // should probably be foreground threads
            cancellationSource = new CancellationTokenSource();

            DeviceCommand deviceCommand = new DeviceCommand(DeviceCommand.CommandType.COMMAND_INITIALIZE);
            commandQueue.Add(deviceCommand);

            thread.Start();
        }

        volatile bool disposed;

        ~Device()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            Console.WriteLine("DEVICE: Dispose()...");
            if (disposed) { return; }
            disposed = true;
            this.quit = true;
            cancellationSource.Cancel(true);
            if (!this.thread.Join(5000))
            {
                this.thread.Interrupt();
                if (!this.thread.Join(1000))
                {
                    this.thread.Abort();
                }
            }
        }


        /*
        private static bool ReadCwaInfo(string deviceFileName, out int deviceId, out uint sessionId)
        {
            IntPtr handle = OmApi.OmReaderOpen(deviceFileName);
            if (handle != null)
            {
                OmApi.OmReaderMetadata(handle, out deviceId, out sessionId);
                OmApi.OmReaderClose(handle);
                return true;
            }
            return false;
        }
        */

        private static bool ReadCwaInfo(string deviceFileName, out uint deviceId, out uint sessionId, out uint startTime, out uint endTime)
        {
            deviceId = 0xffffffff;
            sessionId = 0xffffffff;
            startTime = 0xffffffff;
            endTime = 0xffffffff;
            if (deviceFileName == null)
            {
                return false;
            }
            try
            {
                using (FileStream fs = new FileStream(deviceFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] headerBuffer = new byte[1024];
                    try
                    {
                        fs.Seek(0, SeekOrigin.Begin);
                        if (fs.Read(headerBuffer, 0, headerBuffer.Length) != headerBuffer.Length)
                        {
                            Console.WriteLine("WARNING: Unexpectedly read too few bytes (MD).");
                            return false;
                        }
                    }
                    catch (Exception) { ; }
                    if (headerBuffer[0] != 'M' || headerBuffer[1] != 'D')
                    {
                        Console.WriteLine("WARNING: Invalid file header type (MD).");
                        return false;
                    }
                    deviceId = BitConverter.ToUInt16(headerBuffer, 5);
                    uint upperDeviceId = BitConverter.ToUInt16(headerBuffer, 11);
                    if (upperDeviceId != 0xffff)
                    {
                        deviceId = (upperDeviceId << 16) | deviceId;
                    }
                    sessionId = BitConverter.ToUInt32(headerBuffer, 7);
                    startTime = BitConverter.ToUInt32(headerBuffer, 13);
                    endTime = BitConverter.ToUInt32(headerBuffer, 17);
                    return true;
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("INFO: " + ex.Message);
                return false;
            }
        }


        public void ThreadStart()
        {
            while (!quit)
            {
                try
                {
                    DeviceCommand command = commandQueue.Take(cancellationSource.Token);

                    if (command.Command != DeviceCommand.CommandType.COMMAND_BATTERY && command.Command != DeviceCommand.CommandType.COMMAND_LED)
                    {
                        Console.WriteLine("COMMAND: #" + this.id + " " + command.ToString());
                    }

                    switch (command.Command)
                    {
                        case DeviceCommand.CommandType.COMMAND_INITIALIZE:
                            {
                                int res;

                                // int firmwareVersion = -1, hardwareVersion = -1;
                                // res = OmApi.OmGetVersion(this.Id, out firmwareVersion, out hardwareVersion);
                                // if (OmApi.OM_FAILED(res)) { Console.WriteLine("COMMS: " + this.Id + " Get versions"); CommsError = true; break; }

                                uint sessionId = 0;
                                res = OmApi.OmGetSessionId(this.Id, out sessionId);
                                if (OmApi.OM_FAILED(res)) { Console.WriteLine("COMMS: " + this.Id + " Get session id err=" + res); CommsError = true; IsInitializing = false; break; }
                                SessionId = sessionId;

                                this.fileName = "";
                                this.FileSize = -1;
                                this.FileModified = false;
                                StringBuilder filenamesb = new StringBuilder(256);
                                if (OmApi.OmGetDataFilename(this.Id, filenamesb) == OmApi.OM_OK)
                                {
                                    this.fileName = filenamesb.ToString();
                                }

                                if (this.fileName != null && this.fileName.Length > 0)
                                {
                                    if (File.Exists(this.fileName))
                                    {
                                        try
                                        {
                                            FileInfo fileInfo = new FileInfo(this.fileName);
                                            this.FileSize = (int)fileInfo.Length;

                                            FileAttributes fileAttributes = File.GetAttributes(this.fileName);
                                            this.FileModified = ((fileAttributes & FileAttributes.Archive) != 0);

                                            uint fileDeviceId = 0;
                                            uint fileSessionId = 0;
                                            uint fileStartTime = 0;
                                            uint fileEndTime = 0;
                                            bool readOk = ReadCwaInfo(this.fileName, out fileDeviceId, out fileSessionId, out fileStartTime, out fileEndTime);

                                            if (readOk && this.Id != fileDeviceId)
                                            {
                                                Console.WriteLine("COMMS: Mismatched device id from file: expected " + this.Id + ", found " + fileDeviceId + " in " + this.fileName);
                                                CommsError = true;
                                                IsInitializing = false;
                                                break;
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            Console.WriteLine("WARNING: Problem finding file information: " + this.fileName);
                                        }
                                    }
                                }

                                uint startTimeValue = 0, stopTimeValue = 0;
                                res = OmApi.OmGetDelays(this.Id, out startTimeValue, out stopTimeValue);
                                if (OmApi.OM_FAILED(res)) { Console.WriteLine("COMMS: " + this.Id + " Get delays"); CommsError = true; IsInitializing = false; break; }
                                StartTime = OmApi.OmDateTimeUnpack(startTimeValue);
                                StopTime = OmApi.OmDateTimeUnpack(stopTimeValue);

                                // QC Test data is exactly 8 days, starts at midnight, and has a session id that matches the device id
                                if ((StopTime - StartTime).TotalDays == 8 && sessionId == Id && StartTime.Hour == 0 && StartTime.Minute == 0 && StartTime.Second == 0)
                                {
                                    IsQcData = true;
                                }
                                IsInitializing = false;

                                break;
                            }

                        case DeviceCommand.CommandType.COMMAND_LED:
                            {
                                int res = OmApi.OmSetLed(this.Id, command.Value);
                                if (OmApi.OM_FAILED(res)) { Console.WriteLine("COMMS: " + this.Id + " Set LED"); CommsError = true; break; }
                                Led = command.Value;
                                break;
                            }

                        case DeviceCommand.CommandType.COMMAND_RESET:
                            {
                                // Flag as error
                                Console.WriteLine("COMMS: Device reset " + this.Id + "");
                                this.CommsError = true;
                                OmApi.OmCommand((int)this.Id, "\r\nreset\r\n", (StringBuilder)null, 0, "RESET", (uint)500, IntPtr.Zero, 0);
                                break;
                            }

                        case DeviceCommand.CommandType.COMMAND_BATTERY:
                            {
                                int res = OmApi.OmGetBatteryLevel(this.Id);
                                if (OmApi.OM_FAILED(res)) { Console.WriteLine("COMMS: " + this.Id + " Get battery level"); CommsError = true; break; }
                                Battery = res;
                                break;
                            }

                        case DeviceCommand.CommandType.COMMAND_CLEAR:
                            {
                                int res;

                                // Mark as not user-configured
                                this.UserConfigured = false;
                                this.IsQcData = false;

                                res = OmApi.OmSetSessionId(this.Id, 0);
                                if (OmApi.OM_FAILED(res)) { Console.WriteLine("COMMS: " + this.Id + " Set session id"); CommsError = true; IsClearing = false; break; }
                                SessionId = 0;

                                res = OmApi.OmSetMetadata(this.Id, "", 0);
                                if (OmApi.OM_FAILED(res)) { Console.WriteLine("COMMS: " + this.Id + " Set metadata"); CommsError = true; IsClearing = false; break; }

                                res = OmApi.OmSetDelays(this.Id, OmApi.OmDateTimePack(DateTime.MaxValue), OmApi.OmDateTimePack(DateTime.MinValue));
                                res = OmApi.OmSetAccelConfig(this.Id, OmApi.OM_ACCEL_DEFAULT_RATE, OmApi.OM_ACCEL_DEFAULT_RANGE);
                                if (OmApi.OM_FAILED(res)) { Console.WriteLine("COMMS: " + this.Id + " Set delays"); CommsError = true; IsClearing = false; break; }
                                StopTime = DateTime.MaxValue;
                                StartTime = DateTime.MinValue;

                                res = OmApi.OmEraseDataAndCommit(this.Id, OmApi.OM_ERASE_LEVEL.OM_ERASE_WIPE);
                                if (OmApi.OM_FAILED(res)) { Console.WriteLine("COMMS: " + this.Id + " Erase"); IsClearing = false; break; }

                                // Reset file/config info
                                this.SessionId = 0;
                                this.FileSize = 1024;
                                this.StopTime = DateTime.MinValue;
                                this.StartTime = DateTime.MaxValue;
                                this.FileModified = true;

                                // Wait for wipe
                                Thread.Sleep(5000);
                                IsClearing = false;

                                break;
                            }

                        case DeviceCommand.CommandType.COMMAND_CONFIGURE:
                            {
                                int res;

                                this.IsQcData = false;

                                DateTime time = DateTime.Now;   // local system time
                                res = OmApi.OmSetTime(this.Id, OmApi.OmDateTimePack(time));
                                if (OmApi.OM_FAILED(res)) { Console.WriteLine("COMMS: " + this.Id + " Set time"); CommsError = true; IsConfiguring = false; break; }

                                res = OmApi.OmSetSessionId(this.Id, command.Configuration.SessionId);
                                if (OmApi.OM_FAILED(res)) { Console.WriteLine("COMMS: " + this.Id + " Set session id"); IsConfiguring = false; break; }

                                res = OmApi.OmSetMetadata(this.Id, command.Configuration.Metadata, command.Configuration.Metadata.Length);
                                if (OmApi.OM_FAILED(res)) { Console.WriteLine("COMMS: " + this.Id + " Set metadata"); CommsError = true; IsConfiguring = false; break; }

                                res = OmApi.OmSetDelays(this.Id, OmApi.OmDateTimePack(command.Configuration.Start), OmApi.OmDateTimePack(command.Configuration.End));
                                if (OmApi.OM_FAILED(res)) { Console.WriteLine("COMMS: " + this.Id + " Set delays"); CommsError = true; IsConfiguring = false; break; }

                                res = OmApi.OmSetAccelConfig(this.Id, command.Configuration.Rate, command.Configuration.Range);
                                if (OmApi.OM_FAILED(res)) { Console.WriteLine("COMMS: " + this.Id + " Set accel config"); CommsError = true; IsConfiguring = false; break; }

                                res = OmApi.OmEraseDataAndCommit(this.Id, OmApi.OM_ERASE_LEVEL.OM_ERASE_QUICKFORMAT);
                                if (OmApi.OM_FAILED(res)) { Console.WriteLine("COMMS: " + this.Id + " Erase"); CommsError = true; IsConfiguring = false; break; }

                                // Reset file info
                                this.SessionId = command.Configuration.SessionId;
                                this.FileSize = 1024;
                                this.StopTime = command.Configuration.End;
                                this.StartTime = command.Configuration.Start;
                                this.FileModified = true;

                                // Wait for erase
                                Thread.Sleep(1000);

                                // Mark as user-configured
                                this.UserConfigured = true;
                                IsConfiguring = false;
                            }
                            break;

                        default:
                            Console.WriteLine("WARNING: Device unhandled command type: ", command.Command);
                            break;
                    }
                    // Console.WriteLine("COMMAND-END: " + command.ToString());
                }
                catch (ThreadInterruptedException e)
                {
                    if (!quit)
                    {
                        Console.WriteLine("WARNING: Device ThreadInterruptedException but not quitting.", e);
                    }
                }
                catch (OperationCanceledException e)
                {
                    if (!quit)
                    {
                        Console.WriteLine("WARNING: Device OperationCanceledException but not quitting.", e);
                    }
                }
                catch (ThreadAbortException e)
                {
                    if (!quit)
                    {
                        Console.WriteLine("WARNING: Device ThreadAbortException but not quitting.", e);
                    }
                }
            }
            Console.WriteLine("DEVICE: Thread exited");
        }



        private int id;
        public int Id
        {
            get { return id; }
        }


        public enum DownloadState { DOWNLOAD_NONE, DOWNLOAD_PROGRESS, DOWNLOAD_COMPLETE, DOWNLOAD_CANCELLED, DOWNLOAD_ERROR };
        private DownloadState downloadStatus;
        public DownloadState DownloadStatus
        {
            get { return downloadStatus; }
            protected set
            {
                if (value != downloadStatus)
                {
                    downloadStatus = value;
                    OnPropertyChanged();
                }
            }
        }


        private int downloadProgress;
        public int DownloadProgress
        {
            get { return downloadProgress; }
            protected set
            {
                if (value != downloadProgress)
                {
                    downloadProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsQcData { get; protected set; }

        private bool isInitializing;
        public bool IsInitializing
        {
            get { return isInitializing; }
            protected set
            {
                if (value != isInitializing)
                {
                    isInitializing = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool isDownloading;
        public bool IsDownloading
        {
            get { return isDownloading; }
            protected set
            {
                if (value != isDownloading)
                {
                    isDownloading = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool isConfiguring;
        public bool IsConfiguring
        {
            get { return isConfiguring; }
            protected set
            {
                if (value != isConfiguring)
                {
                    isConfiguring = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool isClearing;
        public bool IsClearing
        {
            get { return isClearing; }
            protected set
            {
                if (value != isClearing)
                {
                    isClearing = value;
                    OnPropertyChanged();
                }
            }
        }


        private string fileName = null;

        private int fileSize = -1;
        public int FileSize
        {
            get { return fileSize; }
            protected set
            {
                if (value != fileSize)
                {
                    fileSize = value;
                    OnPropertyChanged();
                    HasData = fileSize > 1024;
                }
            }
        }

        private bool fileModified = false;
        public bool FileModified
        {
            get { return fileModified; }
            protected set
            {
                if (value != fileModified)
                {
                    fileModified = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool hasData = false;
        public bool HasData
        {
            get { return hasData; }
            protected set
            {
                if (value != hasData)
                {
                    hasData = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool userConfigured = false;
        public bool UserConfigured
        {
            get { return userConfigured; }
            protected set
            {
                if (value != userConfigured)
                {
                    userConfigured = value;
                    OnPropertyChanged();
                }
            }
        }


        // Flag indicate communication error
        private bool commsError = false;
        public bool CommsError
        {
            get
            {
                return commsError;
            }
            protected set
            {
                // Cannot clear error flag
                if (!commsError && value)
                {
                    Console.WriteLine("COMMS-ERROR: Now set for device " + this.Id);
                    commsError = true;
                    OnPropertyChanged();
                }
            }
        }

        // Configured session id
        private uint sessionId = uint.MaxValue;
        public uint SessionId
        {
            get { return sessionId; }
            protected set
            {
                if (sessionId != value)
                {
                    sessionId = value;
                    OnPropertyChanged();
                }
            }
        }

        // Battery level
        private DateTime batteryRequested = DateTime.MinValue;
        private DateTime batteryUpdated = DateTime.MinValue;
        private int battery;
        public int Battery
        {
            get { return battery; }
            protected set
            {
                batteryUpdated = DateTime.UtcNow;
                if (battery != value)
                {
                    battery = value;                    
                    OnPropertyChanged();
                }
                IsCharged = Battery >= (IsCharged ? ChargedLevel - 5 : ChargedLevel);
            }
        }

        private int chargedLevel = 82;
        public int ChargedLevel
        {
            get { return chargedLevel; }
            set
            {
                chargedLevel = value;
                Battery = Battery;  // re-evaluate charged flag
            }
        }

        private bool isCharged = false;
        public bool IsCharged
        {
            get { return isCharged; }
            protected set
            {
                if (value != isCharged)
                {
                    isCharged = value;
                    OnPropertyChanged();
                }
            }
        }

        // LED
        private int led = -2;
        public int Led
        {
            get { return led; }
            protected set
            {
                if (led != value)
                {
                    led = value;
                    OnPropertyChanged();
                }
            }
        }

        public void CancelDownload()
        {
            OmApi.OmCancelDownload(this.Id);
        }

        internal void UpdateDownloadProgress(OmApi.OM_DOWNLOAD_STATUS status, int value)
        {
            switch (status)
            {
                case OmApi.OM_DOWNLOAD_STATUS.OM_DOWNLOAD_NONE:
                    {
                        DownloadProgress = value;
                        DownloadStatus = DownloadState.DOWNLOAD_NONE;
                        IsDownloading = false;
                        break;
                    }

                case OmApi.OM_DOWNLOAD_STATUS.OM_DOWNLOAD_PROGRESS:
                    {
                        DownloadProgress = value;
                        DownloadStatus = DownloadState.DOWNLOAD_PROGRESS;
                        break;
                    }

                case OmApi.OM_DOWNLOAD_STATUS.OM_DOWNLOAD_COMPLETE:
                    {
                        string filename = null;
                        if (downloadFilename != null)
                        {
                            filename = downloadFilename;

                            // Check final file size matches
                            if (!File.Exists(filename))
                            {
                                Console.WriteLine("ERROR: Problem finding downloaded file: " + filename);
                            }

                            int downloadedSize = -1;
                            try
                            {
                                FileInfo fileInfo = new FileInfo(filename);
                                downloadedSize = (int)fileInfo.Length;
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("ERROR: Problem finding downloaded file information: " + filename);
                                DownloadStatus = DownloadState.DOWNLOAD_ERROR;
                                break;
                            }

                            if (downloadedSize != fileSize)
                            {
                                Console.WriteLine("ERROR: Mismatched download file size: " + filename + " (" + downloadedSize + ") vs " + this.fileName + " (" + this.fileSize + ").");
                                DownloadStatus = DownloadState.DOWNLOAD_ERROR;
                                if (DeleteOnFail)
                                {
                                    Console.WriteLine("Recycling failed download (file size mismatch): " + downloadFilename);
                                    RecycleFile(downloadFilename);
                                }
                                break;
                            }

                            // Rename after download
                            if (downloadFilenameRename != null && downloadFilename != downloadFilenameRename)
                            {
                                try
                                {
                                    File.Move(downloadFilename, downloadFilenameRename);
                                    filename = downloadFilenameRename;
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("ERROR: Problem moving final file");
                                    DownloadStatus = DownloadState.DOWNLOAD_ERROR;
                                    if (DeleteOnFail)
                                    {
                                        Console.WriteLine("Recycling failed download (problem moving to final destination): " + downloadFilename);
                                        RecycleFile(downloadFilename);
                                    }
                                    break;
                                }
                            }
                        }

                        DownloadProgress = value;
                        DownloadStatus = DownloadState.DOWNLOAD_COMPLETE;
                        IsDownloading = false;
                        break;
                    }

                case OmApi.OM_DOWNLOAD_STATUS.OM_DOWNLOAD_CANCELLED:
                    {
                        DownloadProgress = value;
                        DownloadStatus = DownloadState.DOWNLOAD_CANCELLED;
                        if (DeleteOnFail)
                        {
                            Console.WriteLine("Recycling failed download (cancelled download): " + downloadFilename);
                            RecycleFile(downloadFilename);
                        }
                        IsDownloading = false;
                    }
                    break;

                case OmApi.OM_DOWNLOAD_STATUS.OM_DOWNLOAD_ERROR:
                    {
                        DownloadProgress = value;
                        DownloadStatus = DownloadState.DOWNLOAD_ERROR;
                        IsDownloading = false;
                        if (DeleteOnFail)
                        {
                            Console.WriteLine("Recycling failed download (download error): " + downloadFilename);
                            RecycleFile(downloadFilename);
                        }
                        break;
                    }
            }
        }

        protected string downloadFilename = null;
        protected string downloadFilenameRename = null;
        public void BeginDownloading(string filename, string renameFilename = null)
        {
            this.downloadFilename = filename;
            this.downloadFilenameRename = renameFilename;
            DownloadProgress = 0;
            DownloadStatus = DownloadState.DOWNLOAD_PROGRESS;
            IsDownloading = true;
            int res = OmApi.OM_E_FAIL;

            if (renameFilename != null && File.Exists(renameFilename))
            {
                Console.WriteLine("ERROR: Download final destination file already exists for device " + this.Id + ": " + renameFilename);
            }
            else
            {
                res = OmApi.OmBeginDownloading(this.Id, 0, -1, filename);
            }
            if (OmApi.OM_FAILED(res))
            {
                DownloadStatus = DownloadState.DOWNLOAD_ERROR;
                IsDownloading = false;
            }
        }


        // Configured start time
        private DateTime startTime;
        public DateTime StartTime
        {
            get { return startTime; }
            protected set
            {
                if (startTime != value)
                {
                    startTime = value;
                    OnPropertyChanged();
                }
            }
        }

        // Configured stop time
        private DateTime stopTime;
        public DateTime StopTime
        {
            get { return stopTime; }
            protected set
            {
                if (stopTime != value)
                {
                    stopTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? WithinInterval
        {
            get
            {
                DateTime now = DateTime.Now;
                if (StopTime <= StartTime) { return null; }     // no interval
                else if (StartTime <= DateTime.MinValue && StopTime >= DateTime.MaxValue) { return 0; } // within interval: always recording
                else if (StopTime <= now) { return 1; }         // after interval
                else if (StartTime > now) { return -1; }        // before interval
                else { return 0; }                              // within interval
            }
        }

        public bool IsConfigured
        {
            get
            {
                return WithinInterval.HasValue;
            }
        }


        public bool CommandReset()
        {
            DeviceCommand deviceCommand = new DeviceCommand(DeviceCommand.CommandType.COMMAND_RESET);
            commandQueue.Add(deviceCommand);
            return true;
        }

        public bool CommandLed(int color)
        {
            // Ignore if already set
            if (color == Led)
            {
                return false;
            }
            DeviceCommand deviceCommand = new DeviceCommand(DeviceCommand.CommandType.COMMAND_LED, color);
            commandQueue.Add(deviceCommand);
            return true;
        }

        public bool CommandBattery(bool force = false)
        {
            bool update = force;
            DateTime now = DateTime.UtcNow;
            int interval = this.IsCharged ? 2 * 60 : (this.isDownloading ? 2 * 60 : 30);
            if (batteryRequested == DateTime.MinValue || (now - batteryRequested).TotalSeconds > interval)
            {
                update = true;
            }
            if (update)
            {
                batteryRequested = now;
                DeviceCommand deviceCommand = new DeviceCommand(DeviceCommand.CommandType.COMMAND_BATTERY);
                commandQueue.Add(deviceCommand);
            }
            return update;
        }

        public bool CommandClear()
        {
            IsClearing = true;
            DownloadStatus = DownloadState.DOWNLOAD_NONE;
            DeviceCommand deviceCommand = new DeviceCommand(DeviceCommand.CommandType.COMMAND_CLEAR);
            commandQueue.Add(deviceCommand);
            return true;
        }

        public bool CommandConfigure(Configuration configuration)
        {
            IsConfiguring = true;
            DeviceCommand deviceCommand = new DeviceCommand(DeviceCommand.CommandType.COMMAND_CONFIGURE, configuration);
            commandQueue.Add(deviceCommand);
            return true;
        }



        // Overall state
        public enum DeviceState
        {
            STATE_NONE,         // no state
            STATE_UNKNOWN,      // device attached but not yet examined
            STATE_ERROR,        // a failure has occurred (comms error or download error), reconnect?
            STATE_PENDING,      // has no data and a configuration that has not yet started
            STATE_UNDERWAY,     // has data or a configuration that has started; and a configuration that has not yet finished (and spare capacity?)
            STATE_COMPLETE,     // has a configuration that has finished or has data and no configuration (or has no spare capacity?)
            STATE_DOWNLOADING,  // download in progress
            STATE_DOWNLOADED,   // download completed
            STATE_CLEARING,     // clearing device in progress
            STATE_RECHARGING,   // cleared (has no data and no configuration) but still recharging
            STATE_CHARGED,      // cleared (has no data and no configuration) but charging has completed;
            STATE_CONFIGURING,  // setting a new configurtaion in progress
            STATE_CONFIGURED,   // configurtaion finished
        }
        private DeviceState state = DeviceState.STATE_UNKNOWN;
        public DeviceState State
        {
            get
            {
                return this.state;
            }
        }

        public DeviceState DetermineState() {
            int? interval = this.WithinInterval;

            // Error (Downloading)
            if (this.DownloadStatus == DownloadState.DOWNLOAD_ERROR || this.downloadStatus == DownloadState.DOWNLOAD_CANCELLED){ return DeviceState.STATE_ERROR; }

            // Downloading
            if (this.IsDownloading) { return DeviceState.STATE_DOWNLOADING; }

            // Final state
            if (this.UserConfigured && !HasData && IsConfigured) { return DeviceState.STATE_CONFIGURED; }

            // Error (Comms)
            if (this.CommsError) { return DeviceState.STATE_ERROR; }

            // Startup
            if (this.IsInitializing) { return DeviceState.STATE_UNKNOWN; }

            // Temporary actions
            if (this.IsClearing) { return DeviceState.STATE_CLEARING; }
            if (this.IsConfiguring) { return DeviceState.STATE_CONFIGURING; }

            // Downloaded (needs to be cleared to be in any future state)
            if (DownloadStatus == DownloadState.DOWNLOAD_COMPLETE) { return DeviceState.STATE_DOWNLOADED; }
            
            // Static state
            if (!IsConfigured && !HasData && !IsCharged) { return DeviceState.STATE_RECHARGING; } // Device has no data or configuration (it is charging)
            else if (!IsConfigured && !HasData && IsCharged) { return DeviceState.STATE_CHARGED; } // Device has no data or configuration (it is not charging)
            else if (!HasData && interval < 0) { return DeviceState.STATE_PENDING; } // has no data and a configuration that has not yet started
            else if ((HasData && interval < 0) || interval == 0) { return DeviceState.STATE_UNDERWAY; } // during a recording, or has some data but before another recording
            else { return DeviceState.STATE_COMPLETE; } // after recording, or has data and no configuration
        }

        // Periodic update task trigger (e.g. battery, LED flash)
        public void Update()
        {
            // Update battery (internally rate-limited)
            CommandBattery();

            // LED flash
            flashIndex++;
            if (flashCode != null && flashCode.Length > 0)
            {
                int color = flashCode[flashIndex % flashCode.Length];
                // Internally only issues a serial command if the LED needs to change color
                CommandLed(color);
            }
        }

    }
}


