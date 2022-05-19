using Microsoft.ReactNative.Managed;
using Newtonsoft.Json;
using rnwindowsminimal.Constants;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace rnwindowsminimal.Bluetooth
{
    [ReactModule]
    public class BleManager
    {
        private ObservableCollection<BleDevice> knownDevices = new ObservableCollection<BleDevice>();
        private List<DeviceInformation> UnknownDevices = new List<DeviceInformation>();

        private DeviceWatcher deviceWatcher;
        public BleManager()
        {
        }
        private bool isScanning;
        public bool IsScanning {
            get => this.isScanning;
            private set
            {
                this.isScanning = value;
                IsScanningEvent(value);
            }
        }


        public int ScanningTimeout { get; set; }
        public CancellationTokenSource TokenSource { get; set; }

        [ReactEvent]
        public Action<string> Event { get; set; }

        [ReactEvent]
        public Action<string, string> UserNotification { get; set; }

        [ReactEvent]
        public Action<bool> IsScanningEvent { get; set; }

        [ReactEvent]
        public Action<string> DeviceAdded { get; set; }

        [ReactEvent]
        public Action<string> DeviceRemoved { get; set; }

        [ReactEvent]
        public Action<string> DeviceUpdated { get; set; }

        [ReactEvent]
        public Action<string> DeviceEnumerationCompleted { get; set; }

        /// <summary>
        /// List of all fetched and known devices
        /// </summary>
        [ReactConstant]
        public List<BleDevice> KnownDevices
        {
            get { return this.knownDevices.ToList(); }
        }

        [ReactConstant]
        public string Connected = "Connected to BleManager";

        /// <summary>
        /// Start scanning for sensor, removing all known sensor and readding all scanned Dorsavi devices
        /// </summary>
        /// <param name="timeoutMilliseconds"></param>
        [ReactMethod("StartScan")]
        public async void StartScanningAsync(int timeoutMilliseconds = 30)
        {            
            if (!this.IsScanning || deviceWatcher == null)
            {
                ////
                //Task.Run(() =>
                //{
                //    WaitHandle.WaitAny(new[] { this.TokenSource.Token.WaitHandle });
                //    if (this.TokenSource.IsCancellationRequested)
                //    {
                //        UserNotification("User canceled scan", NotifyType.StatusMessage.ToString());
                //        this.StopBleDeviceWatcher();
                //        UserNotification($"Device watcher stopped.", NotifyType.StatusMessage.ToString());
                //    }
                //});

                Event("Start enumerating");
                StartBleDeviceWatcher();
                UserNotification($"Device watcher started.", NotifyType.StatusMessage.ToString());
            }
            else
            {
                UserNotification("User canceled scan", NotifyType.StatusMessage.ToString());
                this.StopBleDeviceWatcher();
                UserNotification($"Device watcher stopped.", NotifyType.StatusMessage.ToString());
            }

        }

        

        #region Device discovery

        /// <summary>
        /// Starts a device watcher that looks for all nearby Bluetooth devices (paired or unpaired).
        /// Attaches event handlers to populate the device collection.
        /// </summary>
        private void StartBleDeviceWatcher()
        {
            // Additional properties we would like about the device.
            // Property strings are documented here https://msdn.microsoft.com/en-us/library/windows/desktop/ff521659(v=vs.85).aspx
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

            // BT_Code: Example showing paired and non-paired in a single query.
            string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

            deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        aqsAllBluetoothLEDevices,
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start over with an empty collection.
            knownDevices.Clear();

            // Start the watcher. Active enumeration is limited to approximately 30 seconds.
            // This limits power usage and reduces interference with other Bluetooth activities.
            // To monitor for the presence of Bluetooth LE devices for an extended period,
            // use the BluetoothLEAdvertisementWatcher runtime class. See the BluetoothAdvertisement
            // sample for an example.
            deviceWatcher.Start();

            this.IsScanning = true;
        }

        /// <summary>
        /// Stops watching for all nearby Bluetooth devices.
        /// </summary>
        private void StopBleDeviceWatcher()
        {
            if (deviceWatcher != null)
            {
                // Unregister the event handlers.
                deviceWatcher.Added -= DeviceWatcher_Added;
                deviceWatcher.Updated -= DeviceWatcher_Updated;
                deviceWatcher.Removed -= DeviceWatcher_Removed;
                deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
                deviceWatcher.Stopped -= DeviceWatcher_Stopped;

                // Stop the watcher.
                deviceWatcher.Stop();
                deviceWatcher = null;
            }
        }

        private BleDevice FindBleDevice(string id)
        {
            foreach (var bleDeviceDisplay in knownDevices)
            {
                if (bleDeviceDisplay.Id == id)
                {
                    return bleDeviceDisplay;
                }
            }
            return null;
        }

        private DeviceInformation FindUnknownDevices(string id)
        {
            foreach (DeviceInformation bleDeviceInfo in UnknownDevices)
            {
                if (bleDeviceInfo.Id == id)
                {
                    return bleDeviceInfo;
                }
            }
            return null;
        }

        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            lock (this)
            {
                Debug.WriteLine(String.Format("Added {0}{1}", deviceInfo.Id, deviceInfo.Name));

                //if (deviceInfo.Name.StartsWith(SensorHardwareTypeNames.MDE) ||
                //    deviceInfo.Name.StartsWith(SensorHardwareTypeNames.MDD) ||
                //    deviceInfo.Name.StartsWith(SensorHardwareTypeNames.MDM))
                //{
                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        // Make sure device isn't already present in the list.
                        if (FindBleDevice(deviceInfo.Id) == null)
                        {
                            if (deviceInfo.Name != string.Empty)
                            {
                                var bleDevice = new BleDevice(deviceInfo);
                                DeviceAdded(JsonConvert.SerializeObject(bleDevice));
                                // If device has a friendly name display it immediately.
                                knownDevices.Add(bleDevice);
                            }
                            else
                            {
                                // Add it to a list in case the name gets updated later.
                                UnknownDevices.Add(deviceInfo);
                            }
                        }
                    }
                //}
            }
        }

        private async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            lock (this)
            {
                Debug.WriteLine(String.Format("Updated {0}{1}", deviceInfoUpdate.Id, ""));

                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    BleDevice bleDevice = FindBleDevice(deviceInfoUpdate.Id);
                    if (bleDevice != null)
                    {
                        // Device is already being displayed - update UX.
                        bleDevice.Update(deviceInfoUpdate);
                        DeviceUpdated(JsonConvert.SerializeObject(bleDevice));
                        return;
                    }

                    DeviceInformation deviceInfo = FindUnknownDevices(deviceInfoUpdate.Id);
                    if (deviceInfo != null)
                    {
                        deviceInfo.Update(deviceInfoUpdate);
                        // If device has been updated with a friendly name it's no longer unknown.
                        if (deviceInfo.Name != String.Empty)
                        {
                            var device = new BleDevice(deviceInfo);
                            DeviceAdded(JsonConvert.SerializeObject(bleDevice));

                            // If device has a friendly name display it immediately.
                            knownDevices.Add(device);
                            UnknownDevices.Remove(deviceInfo);
                        }
                    }
                }
            }
        }

        private async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            lock (this)
            {
                Debug.WriteLine(String.Format("Removed {0}{1}", deviceInfoUpdate.Id, ""));

                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    // Find the corresponding DeviceInformation in the collection and remove it.
                    BleDevice bleDevice = FindBleDevice(deviceInfoUpdate.Id);
                    if (bleDevice != null)
                    {
                        knownDevices.Remove(bleDevice);
                        DeviceRemoved(JsonConvert.SerializeObject(bleDevice));
                    }

                    DeviceInformation deviceInfo = FindUnknownDevices(deviceInfoUpdate.Id);
                    if (deviceInfo != null)
                    {
                        UnknownDevices.Remove(deviceInfo);
                    }
                }
            }
        }

        private async void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {
            // Protect against race condition if the task runs after the app stopped the deviceWatcher.
            if (sender == deviceWatcher)
            {
                var message = $"{knownDevices.Count} devices found. Enumeration completed.";
                DeviceEnumerationCompleted(message);
                UserNotification(message,
                    NotifyType.StatusMessage.ToString());
            }
        }

        private async void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
            // Protect against race condition if the task runs after the app stopped the deviceWatcher.
            if (sender == deviceWatcher)
            {
                UserNotification($"No longer watching for devices.",
                        sender.Status == DeviceWatcherStatus.Aborted ? NotifyType.ErrorMessage.ToString() : NotifyType.StatusMessage.ToString());
            }
        }

        #endregion Device discovery

        #region Pairing

        private bool isBusy = false;
        
        [ReactMethod("PairDevice")]
        public async void PairDevice(string deviceId)
        {
            // Do not allow a new Pair operation to start if an existing one is in progress.
            if (isBusy)
            {
                UserNotification("Bluetooth pairing (Busy), please wait until current pairing finished", NotifyType.StatusMessage.ToString());
                return;
            }

            isBusy = true;
            UserNotification("Pairing started. Please wait..", NotifyType.StatusMessage.ToString());

            // For more information about device pairing, including examples of
            // customizing the pairing process, see the DeviceEnumerationAndPairing sample.

            // Capture the current selected item in case the user changes it while we are pairing.            
            var bleDevice = FindBleDevice(deviceId);
            if (bleDevice != null)
            {
                // BT_Code: Pair the currently selected device.
                var result = await bleDevice.DeviceInformation.Pairing.PairAsync();

                UserNotification($"Pairing result = {result.Status}",
                    result.Status == DevicePairingResultStatus.Paired || result.Status == DevicePairingResultStatus.AlreadyPaired
                        ? NotifyType.StatusMessage.ToString()
                        : NotifyType.ErrorMessage.ToString());
            }
            else
            {
                UserNotification("Pairing fail, Could not find device.", NotifyType.ErrorMessage.ToString());
            }

            
            isBusy = false;
        }

        #endregion
    }
}