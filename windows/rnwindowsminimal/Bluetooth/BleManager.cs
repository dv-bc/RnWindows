using Microsoft.ReactNative.Managed;
using Newtonsoft.Json;
using rnwindowsminimal.Constants;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using rnwindowsminimal.Constants;
namespace rnwindowsminimal.Bluetooth
{
    [ReactModule]
    public class BleManager
    {
        /// <summary>
        /// Tells the Watcher what properties we want to access from Bluetooth Devices if available.
        /// </summary>
        private readonly string[] requestedProperties =
        {
            "System.Devices.Aep.Bluetooth.Le.IsConnectable",
            "System.Devices.Aep.IsConnected",
            "System.Devices.Aep.ContainerId",
            "System.Devices.Aep.SignalStrength",
            "System.Devices.Aep.ModelName",
            "System.Devices.Aep.ModelId",
            "System.Devices.AepContainer.ModelName"
        };

        #region Error Codes
        readonly int E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED = unchecked((int)0x80650003);
        readonly int E_BLUETOOTH_ATT_INVALID_PDU = unchecked((int)0x80650004);
        readonly int E_ACCESSDENIED = unchecked((int)0x80070005);
        readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)
        #endregion

        /// <summary>
        /// Additional filters on discovered blue tooth devices.
        /// System.ItemNameDisplay will make sure only device containing MD in the name will be returned.
        /// </summary>

        //private readonly string requestedAqsFilters = "(System.ItemNameDisplay:~~\"MD\") AND (System.Devices.Aep.Bluetooth.Le.IsConnectable:=System.StructuredQueryType.Boolean#True OR System.Devices.Aep.IsConnected:=System.StructuredQueryType.Boolean#True)";
        private readonly string requestedAqsFilters = " (System.Devices.Aep.Bluetooth.Le.IsConnectable:=System.StructuredQueryType.Boolean#True OR System.Devices.Aep.IsConnected:=System.StructuredQueryType.Boolean#True)";

        private ObservableCollection<BleDevice> knownDevices = new ObservableCollection<BleDevice>();
        private List<DeviceInformation> UnknownDevices = new List<DeviceInformation>();
        private BluetoothLEDevice bluetoothLeDevice = null;
        private DeviceWatcher deviceWatcher;
        private GattCharacteristic selectedCharacteristic;

        // Only one registered characteristic at a time.
        private GattCharacteristic registeredCharacteristic;
        private GattPresentationFormat presentationFormat;
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
        public Action<bool> IsConnecting { get; set; }

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
            string[] crequestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

            // BT_Code: Example showing paired and non-paired in a single query.
            string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

            deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        aqsAllBluetoothLEDevices,
                        crequestedProperties,
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



        private bool subscribedForNotifications = false;

        #endregion

        #region Enumerating Services
        private async Task<bool> ClearBluetoothLEDeviceAsync()
        {
            if (subscribedForNotifications)
            {
                // Need to clear the CCCD from the remote device so we stop receiving notifications
                var result = await registeredCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                if (result != GattCommunicationStatus.Success)
                {
                    return false;
                }
                else
                {
                    selectedCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                    subscribedForNotifications = false;
                }
            }
            bluetoothLeDevice?.Dispose();
            bluetoothLeDevice = null;
            return true;
        }

        [ReactMethod("ConnectDevice")]
        public async void ConnectDevice(string deviceId)
        {
            IsConnecting(true);

            if (!await ClearBluetoothLEDeviceAsync())
            {
                UserNotification("Error: Unable to reset state, try again.", NotifyType.ErrorMessage.ToString());
                IsConnecting(false);
                return;
            }

            try
            {
                // BT_Code: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
                bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceId);

                if (bluetoothLeDevice == null)
                {
                    UserNotification("Failed to connect to device.", NotifyType.ErrorMessage.ToString());
                }
            }
            catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
            {
                UserNotification("Bluetooth radio is not on.", NotifyType.ErrorMessage.ToString());
            }

            if (bluetoothLeDevice != null)
            {
                // Note: BluetoothLEDevice.GattServices property will return an empty list for unpaired devices. For all uses we recommend using the GetGattServicesAsync method.
                // BT_Code: GetGattServicesAsync returns a list of all the supported services of the device (even if it's not paired to the system).
                // If the services supported by the device are expected to change during BT usage, subscribe to the GattServicesChanged event.
                GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    var services = result.Services;
                    UserNotification(String.Format("Found {0} services", services.Count), NotifyType.StatusMessage.ToString());
                    foreach (var service in services)
                    {
                        //  ServiceList.Items.Add(new ComboBoxItem { Content = DisplayHelpers.GetServiceName(service), Tag = service });
                    }
                    //ConnectButton.Visibility = Visibility.Collapsed;
                    //ServiceList.Visibility = Visibility.Visible;
                }
                else
                {
                    UserNotification("Device unreachable", NotifyType.ErrorMessage.ToString());
                }
            }
            IsConnecting(false);
        }

        #endregion

        #region Connect and Enumerate Characteristic

        #endregion
        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            // BT_Code: An Indicate or Notify reported that the value has changed.
            // Display the new value with a timestamp.
            var newValue = FormatValueByPresentation(args.CharacteristicValue, presentationFormat);
            var message = $"Value at {DateTime.Now:hh:mm:ss.FFF}: {newValue}";
            //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            //    () => CharacteristicLatestValue.Text = message);
        }


        private string FormatValueByPresentation(IBuffer buffer, GattPresentationFormat format)
        {
            // BT_Code: For the purpose of this sample, this function converts only UInt32 and
            // UTF-8 buffers to readable text. It can be extended to support other formats if your app needs them.
            byte[] data;
            CryptographicBuffer.CopyToByteArray(buffer, out data);
            if (format != null)
            {
                if (format.FormatType == GattPresentationFormatTypes.UInt32 && data.Length >= 4)
                {
                    return BitConverter.ToInt32(data, 0).ToString();
                }
                else if (format.FormatType == GattPresentationFormatTypes.Utf8)
                {
                    try
                    {
                        return Encoding.UTF8.GetString(data);
                    }
                    catch (ArgumentException)
                    {
                        return "(error: Invalid UTF-8 string)";
                    }
                }
                else
                {
                    // Add support for other format types as needed.
                    return "Unsupported format: " + CryptographicBuffer.EncodeToHexString(buffer);
                }
            }
            else if (data != null)
            {
                // We don't know what format to use. Let's try some well-known profiles, or default back to UTF-8.
                if (selectedCharacteristic.Uuid.Equals(GattCharacteristicUuids.HeartRateMeasurement))
                {
                    try
                    {
                        return "Heart Rate: " + ParseHeartRateValue(data).ToString();
                    }
                    catch (ArgumentException)
                    {
                        return "Heart Rate: (unable to parse)";
                    }
                }
                else if (selectedCharacteristic.Uuid.Equals(GattCharacteristicUuids.BatteryLevel))
                {
                    try
                    {
                        // battery level is encoded as a percentage value in the first byte according to
                        // https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.battery_level.xml
                        return "Battery Level: " + data[0].ToString() + "%";
                    }
                    catch (ArgumentException)
                    {
                        return "Battery Level: (unable to parse)";
                    }
                }
                // This is our custom calc service Result UUID. Format it like an Int
                else if (selectedCharacteristic.Uuid.Equals(SystemConstants.ResultCharacteristicUuid))
                {
                    return BitConverter.ToInt32(data, 0).ToString();
                }
                // No guarantees on if a characteristic is registered for notifications.
                else if (registeredCharacteristic != null)
                {
                    // This is our custom calc service Result UUID. Format it like an Int
                    if (registeredCharacteristic.Uuid.Equals(SystemConstants.ResultCharacteristicUuid))
                    {
                        return BitConverter.ToInt32(data, 0).ToString();
                    }
                }
                else
                {
                    try
                    {
                        return "Unknown format: " + Encoding.UTF8.GetString(data);
                    }
                    catch (ArgumentException)
                    {
                        return "Unknown format";
                    }
                }
            }
            else
            {
                return "Empty data received";
            }
            return "Unknown format";
        }

        private static ushort ParseHeartRateValue(byte[] data)
        {
            // Heart Rate profile defined flag values
            const byte heartRateValueFormat = 0x01;

            byte flags = data[0];
            bool isHeartRateValueSizeLong = ((flags & heartRateValueFormat) != 0);

            if (isHeartRateValueSizeLong)
            {
                return BitConverter.ToUInt16(data, 1);
            }
            else
            {
                return data[1];
            }
        }

    }
}