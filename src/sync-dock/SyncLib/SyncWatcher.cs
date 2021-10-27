using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncLib
{
    public class SyncWatcher
    {
        private const double INITIAL_TIMEOUT = 15;
        private const double GROWTH_TIMEOUT = 2.0;
        private const double MAX_TIMEOUT = 64 * 60;

        FileSystemWatcher watcher;
        RestSharp.RestClient restclient;

        public string AWS_KEY { get; }
        public string AWS_SECRET { get; }
        public string AWS_BUCKET { get; }
        public RegionEndpoint AWS_REGION { get; }
        public string PRE_SHARED_KEY { get; }

        #region Recycle
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto, Pack=1)]
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

        [DllImport("shell32.dll", CharSet=CharSet.Auto)]
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
        bool deleteAfterSync = false;
        #endregion

        public SyncWatcher(string dirToWatch, string fileExtensions, string AWS_KEY, string AWS_SECRET, string AWS_BUCKET, string AWS_REGION, string API_URL, string PRE_SHARED_KEY, bool deleteAfterSync)
        {
            FailCount = 0;
            this.dirToWatch = dirToWatch;
            this.fileExtensions = fileExtensions;
            this.AWS_KEY = AWS_KEY;
            this.AWS_SECRET = AWS_SECRET;
            this.AWS_BUCKET = AWS_BUCKET;
            this.AWS_REGION = RegionEndpoint.GetBySystemName(AWS_REGION);
            this.PRE_SHARED_KEY = PRE_SHARED_KEY;
            this.deleteAfterSync = deleteAfterSync;

            Console.WriteLine($"SyncWatcher: {dirToWatch}");
            Directory.CreateDirectory(Path.Combine(dirToWatch, "metadata"));

            //upload watcher
            watcher = new FileSystemWatcher(dirToWatch, fileExtensions);
            watcher.Changed += Watcher_Changed;
            watcher.Renamed += Watcher_Renamed;

            restclient = new RestSharp.RestClient(API_URL);
        }

        public class ProgramMetadata
        {
            public string sessionid { get; set; }
            public string deviceid { get; set; }
            public DateTime programmedat { get; set; }
        }

        public void HaveProgrammedDevice(uint sessionid, int deviceid)
        {
            //dump meta in queue for upload to DB
            var meta = new ProgramMetadata()
            {
                sessionid = sessionid.ToString(),
                deviceid = deviceid.ToString(),
                programmedat = DateTime.Now
            };

            string basename = String.Format("{0:D10}-{1:D5}", sessionid, deviceid);
            string filename = Path.Combine(dirToWatch, "metadata", $"{basename}.json");
            Console.WriteLine("HaveProgrammedDevice #" + deviceid + ": session " + sessionid + " -- " + filename + " -- " + meta.deviceid + " / " + meta.sessionid + " / " + meta.programmedat + ".");
            File.WriteAllText(filename, SimpleJson.SimpleJson.SerializeObject(meta));
            metadataQueue.Enqueue(new FileInfo(filename));
            ProcessMeta();
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            var ff = new FileInfo(e.FullPath);
            //try to access the file (fails if still writing)
            if (!IsFileLocked(ff))
            {
                Console.WriteLine($"File processing {e.FullPath}");
                //add to upload queue
                uploadQueue.Enqueue(ff);
                ProcessFile();
            }
            else
            {
                Console.WriteLine($"File locked for read {e.FullPath}");
            }
        }

        public int UploadQueueLength
        {
            get
            {
                return uploadQueue.Count;
            }
        }

        public FileInfo CurrentUpload
        {
            get
            {
                return currentUpload;
            }
        }

        public event Action<FileInfo> OnFileStartedUpload;
        public event Action<FileInfo> OnFileFinishedUpload;

        private bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                var ff = new FileInfo(e.FullPath);
                //try to access the file (fails if still writing)
                if (!IsFileLocked(ff))
                {
                    Console.WriteLine($"File processing {e.FullPath}");
                    //add to upload queue
                    uploadQueue.Enqueue(ff);
                    ProcessFile();
                }
                else
                {
                    Console.WriteLine($"File locked for read {e.FullPath}");
                }
            }
        }

        CancellationTokenSource cancelToken;
        private string dirToWatch;
        private string fileExtensions;

        private Queue<FileInfo> uploadQueue = new Queue<FileInfo>();
        private Queue<FileInfo> metadataQueue = new Queue<FileInfo>();

        private FileInfo currentUpload;

        private async void ProcessMeta()
        {
            if (metadataQueue.Count > 0)
            {
                var currentMeta = metadataQueue.Dequeue();
                try
                {
                    //load metadata for file (json etc)
                    var file_contents = File.ReadAllText(currentMeta.FullName);
                    var metadata = SimpleJson.SimpleJson.DeserializeObject<ProgramMetadata>(file_contents);
                    Console.WriteLine("META: " + currentMeta.FullName + " = " + metadata.deviceid + " / " + metadata.sessionid + " / " + metadata.programmedat);

                    //if this file has been uploaded successfully, then trigger database update
                    var request = new RestSharp.RestRequest($"/data/trigger?psk={this.PRE_SHARED_KEY}", RestSharp.Method.POST);
                    request.AddJsonBody(metadata);
                    var result = await restclient.ExecutePostTaskAsync(request);
                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Console.WriteLine($"DB Updated {currentUpload}");
                        var moveTo = Path.Combine(dirToWatch, "archive", currentMeta.Name);
                        Directory.CreateDirectory(Path.Combine(dirToWatch, "archive"));
                        if (File.Exists(moveTo))
                        {
                            File.Move(moveTo, moveTo + "_" + Guid.NewGuid().ToString());
                        }
                        currentMeta.MoveTo(moveTo);
                    }
                    else
                        Console.WriteLine($"DB Update Failed {currentUpload}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                ProcessMeta();
            }
            
        }

        public int FailCount { get; protected set; }

        public static int TimeoutForFailCount(int failCount)
        {
            return (int)Math.Min(INITIAL_TIMEOUT * Math.Pow(GROWTH_TIMEOUT, failCount), MAX_TIMEOUT);
        }

        private async void ProcessFile()
        {
            if (uploadQueue.Count > 0 && currentUpload == null)
            {
                try
                {
                    //take the next:
                    currentUpload = uploadQueue.Dequeue();
                    cancelToken = new CancellationTokenSource();

                    OnFileStartedUpload?.Invoke(currentUpload);

                    // Wait for delay time
                    int timeout = TimeoutForFailCount(FailCount);
                    if (FailCount > 0)
                    {
                        Console.WriteLine($"Upload delay #{FailCount} for {timeout} seconds...");
                    }
                    await Task.Delay(timeout * 1000, cancelToken.Token);

                    //add to upload queue
                    TransferUtility transfer = new TransferUtility(AWS_KEY, AWS_SECRET, AWS_REGION);
                    
                    string destinationName = Path.GetFileNameWithoutExtension(currentUpload.Name) + "_" + Guid.NewGuid().ToString() + ".cwa";

                    Console.WriteLine($"Starting upload {currentUpload.Name}");
                    Task task = transfer.UploadAsync(currentUpload.FullName, AWS_BUCKET, destinationName, cancelToken.Token);
                    await task;

                    Console.WriteLine($"Finished upload {currentUpload.Name}");
                    FileInfo finishedUpload = currentUpload;
                    this.currentUpload = null;
                    FailCount = 0;  // reset after a successful upload

                    /*
                    try
                    {
                        //load metadata for file (json etc)
                        var file_contents = File.ReadAllText(Path.ChangeExtension(finishedUpload.FullName, "json"));
                        var metadata = SimpleJson.SimpleJson.DeserializeObject<Metadata>(file_contents);

                        //if this file has been uploaded successfully, then trigger database update
                        var request = new RestSharp.RestRequest($"/data/trigger?psk={this.PRE_SHARED_KEY}", RestSharp.Method.POST);
                        request.AddJsonBody(metadata);
                        var result = await restclient.ExecutePostTaskAsync(request);
                        if (result.StatusCode == System.Net.HttpStatusCode.OK)
                            Console.WriteLine($"DB Updated {finishedUpload}");
                        else
                            Console.WriteLine($"DB Update Failed {finishedUpload}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    */

                    if (deleteAfterSync)
                    {
                        RecycleFile(finishedUpload.FullName);
                        Console.WriteLine($"Recycled {finishedUpload.FullName}");
                    }
                    else
                    {
                        //on success, move to archive dir
                        var moveTo = Path.Combine(dirToWatch, "archive", destinationName);
                        Directory.CreateDirectory(Path.Combine(dirToWatch, "archive"));
                        finishedUpload.MoveTo(moveTo);
                        Console.WriteLine($"Moved to {moveTo}");
                    }

                    OnFileFinishedUpload?.Invoke(finishedUpload);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                    // Append current to end of queue
                    if (currentUpload != null)
                    {
                        uploadQueue.Enqueue(currentUpload);
                    }

                    // Failure (will reset after success)
                    FailCount++;
                }
                finally
                {
                    currentUpload = null;
                }

                // Tail-recurse to continue through queue
                ProcessFile();
            }
        }

        /*
        public class Metadata
        {
            public string sessionid { get; set; }
            public DateTime uploadedat { get; set; }
        }

        public String MetadataSidecarContents(string sessionid, string deviceid)
        {
            //dump meta in queue for upload to DB
            var meta = new ProgramMetadata()
            {
                sessionid = sessionid,
                deviceid = deviceid,
                programmedat = DateTime.Now
            };

            string basename = String.Format("{0:D10}-{1:D5}", sessionid, deviceid);
            string filename = Path.Combine(dirToWatch, "metadata", $"{basename}.json");
            File.WriteAllText(filename, SimpleJson.SimpleJson.SerializeObject(meta));
            metadataQueue.Enqueue(new FileInfo(filename));
            ProcessMeta();
        }
        */

        public void Start()
        {
            watcher.EnableRaisingEvents = true;
            //process offline:
            var files = Directory.GetFiles(dirToWatch, this.fileExtensions);
            foreach (var file in files)
            {
                uploadQueue.Enqueue(new FileInfo(Path.Combine(dirToWatch, file)));
            }

            ProcessFile();

            var metadata = Directory.GetFiles(Path.Combine(dirToWatch, "metadata"), "*.json");
            foreach (var meta in metadata)
            {
                metadataQueue.Enqueue(new FileInfo(Path.Combine(dirToWatch, "metadata", meta)));
            }

            ProcessMeta();
        }

        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
            if (cancelToken != null)
            {
                cancelToken.Cancel();
            }
        }
    }
}
