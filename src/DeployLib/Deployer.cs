using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmApiNet;

namespace DeployLib
{

    public class DeviceEventArgs : EventArgs
    {
        public Device Device { get; protected set; }
        public string PropertyName { get; protected set; }
        public DeviceEventArgs(Device device, string propertyName = null)
        {
            Device = device;
            PropertyName = propertyName;
        }
    }



    public class Deployer : IDisposable
    {
        private static Deployer instance;
        public static Deployer Instance
        {
            get
            {
                if (instance == null) { instance = new Deployer(); }
                return instance;
            }
        }

        // Must keep a reference to callbacks to prevent the garbage collector removing the delegate
        private volatile OmApi.OmLogCallback logCallbackDelegate;
        private volatile OmApi.OmDeviceCallback deviceCallbackDelegate;
        private volatile OmApi.OmDownloadCallback downloadCallbackDelegate;

        /** Private constructor (singleton class) */
        private Deployer()
        {
            // Register our log callback handler first (before startup)
            logCallbackDelegate = new OmApi.OmLogCallback(LogCallback);
            GC.SuppressFinalize(logCallbackDelegate);
            OmApi.OmSetLogCallback(logCallbackDelegate, IntPtr.Zero);

            // Register our device callback handler (before startup, so we get the initial set of devices)
            deviceCallbackDelegate = new OmApi.OmDeviceCallback(DeviceCallback);
            GC.SuppressFinalize(deviceCallbackDelegate);
            OmApi.OmSetDeviceCallback(deviceCallbackDelegate, IntPtr.Zero);

            // Startup the API
            int result = OmApi.OmStartup(OmApi.OM_VERSION);
            if (OmApi.OM_FAILED(result))
            {
                throw new Exception("OmStartup failed");
            }

            downloadCallbackDelegate = new OmApi.OmDownloadCallback(DownloadCallback);
            GC.SuppressFinalize(downloadCallbackDelegate);
            OmApi.OmSetDownloadCallback(downloadCallbackDelegate, IntPtr.Zero);
        }

        /** Destructor */
        ~Deployer()
        {
            Dispose(false);
        }

        /** Overridden dispose handler */
        public void Dispose()
        {
            Dispose(true);
        }

        /** Disposed flag */
        private bool disposed;

        /** Dispose handler */
        public void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ;
                }

                foreach (Device device in devices.Values)
                {
                    device.Dispose();
                }

                // Clean up unmanaged resources
                OmApi.OmShutdown();

                // Allow finalizing
                if (logCallbackDelegate != null) { GC.ReRegisterForFinalize(logCallbackDelegate); }
                if (deviceCallbackDelegate != null) { GC.ReRegisterForFinalize(deviceCallbackDelegate); }

                // Disposing complete
                disposed = true;
            }
        }

        protected void LogCallback(IntPtr reference, string message)
        {
            Console.WriteLine("LOG: " + message);
        }




        protected IDictionary<int, Device> devices = new Dictionary<int, Device>();
        public IDictionary<int, Device> Devices
        {
            get
            {
                return devices;
            }
        }
        

        public delegate void DeviceEventHandler(object sender, DeviceEventArgs e);
        public event DeviceEventHandler DeviceAdded;
        public event DeviceEventHandler DeviceRemoved;
        public event DeviceEventHandler DeviceUpdated;

        protected void DeviceCallback(IntPtr reference, int deviceId, OmApi.OM_DEVICE_STATUS status)
        {
            Device device = null;
            lock (devices)
            {
                if (devices.ContainsKey(deviceId))
                {
                    device = devices[deviceId];
                }
            }

            Console.WriteLine("DEPLOYER: " + status + " - " + deviceId);
            if (status == OmApi.OM_DEVICE_STATUS.OM_DEVICE_CONNECTED)
            {
                if (device == null)
                {
                    device = new Device(deviceId);
                    lock (devices)
                    {
                        devices.Add(device.Id, device);
                    }
                    device.PropertyChanged += Device_PropertyChanged;
                    DeviceAdded?.Invoke(this, new DeviceEventArgs(device));
                }
                else
                {
                    Console.WriteLine("WARNING: Device added but already exists: " + deviceId);
                }
            }
            else if (status == OmApi.OM_DEVICE_STATUS.OM_DEVICE_REMOVED)
            {
                if (device != null)
                {
                    DeviceRemoved?.Invoke(this, new DeviceEventArgs(device));
                    device.PropertyChanged -= Device_PropertyChanged;
                    device.Dispose();
                    lock (devices)
                    {
                        devices.Remove(device.Id);
                    }
                }
                else
                {
                    Console.WriteLine("WARNING: Device removed but didn't already exist: " + deviceId);
                }
            }
            else
            {
                Console.WriteLine("WARNING: Unhandled device status: " + status);
            }
        }

        private void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Device device = (Device)sender;
            DeviceUpdated?.Invoke(this, new DeviceEventArgs(device, e.PropertyName));
        }

        protected void DownloadCallback(IntPtr reference, int deviceId, OmApi.OM_DOWNLOAD_STATUS status, int value)
        {
            Device device;
            lock (devices)
            {
                if (!devices.ContainsKey(deviceId))
                {
                    Console.WriteLine("WARNING: Device download progress but didn't already exist: " + deviceId);
                }
                device = devices[deviceId];
            }
            device.UpdateDownloadProgress(status, value);
        }


        public void Start()
        {
            Console.WriteLine("Deployer.Start()...");
            // Initially-connected devices
            foreach (Device device in devices.Values.ToArray()) // duplicate before iteration
            {
                DeviceAdded(this, new DeviceEventArgs(device));
            }
        }

        private bool stopped = false;

        public void Stop()
        {
            Console.WriteLine("Deployer.Stop()...");
            this.stopped = true;
            this.Dispose();
        }

        // Periodic update task trigger (e.g. battery, LED flash)
        public void Update()
        {
            if (this.stopped) { return; }

            Device[] deviceArray;
            lock (devices)
            {
                deviceArray = devices.Values.ToArray();
            }
            foreach (Device device in deviceArray.ToArray()) 
            {
                device.Update();
            }
        }

        /*
        private void OmDeviceRemoved(object sender, OmDeviceEventArgs e)
        {
            Console.WriteLine("DeviceRemoved: " + e.Device.DeviceId);
            if (devices.ContainsKey(e.Device.DeviceId))
            {
                Device device = devices[e.Device.DeviceId];
                DeviceRemoved?.Invoke(this, new DeviceEventArgs(device));
                devices.Remove(e.Device.DeviceId);
                device.Dispose();
            }
        }

        private void OmDeviceAttached(object sender, OmDeviceEventArgs e)
        {
            Console.WriteLine("DeviceAttached: " + e.Device.DeviceId);
            if (devices.ContainsKey(e.Device.DeviceId))
            {
                Console.WriteLine("...force removal of existing...");
                OmDeviceRemoved(sender, e);
            }
            Device device = new Device(e.Device.DeviceId);
            devices.Add(device.Id, device);
            DeviceAdded?.Invoke(this, new DeviceEventArgs(device));
        }

        private void OmDeviceChanged(object sender, OmDeviceEventArgs e)
        {
            Console.WriteLine("DeviceChanged: " + e.Device.DeviceId);
            if (devices.ContainsKey(e.Device.DeviceId))
            {
                Device device = new Device(e.Device);
                bool changed = device.UpdateFromOmEvent(e);
                if (changed)
                {
                    DeviceUpdated?.Invoke(this, new DeviceEventArgs(device));
                }
            }
        }
        */


        public void Test()
        {
            /*
            Random rand = new Random();
            int color = rand.Next(6) + 1;
            Console.WriteLine("Test(): " + color);
            foreach (Device device in devices.Values)
            {
                Console.WriteLine("... " + device.Id + "=" + color + "");
                device.CommandLed(color);
            }
            */
        }
    }


}
