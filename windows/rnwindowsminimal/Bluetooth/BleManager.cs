using Microsoft.ReactNative.Managed;
using Newtonsoft.Json;
using rnwindowsminimal.Bluetooth.Helpers;
using rnwindowsminimal.Constants;
using rnwindowsminimal.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace rnwindowsminimal.Bluetooth
{
    /// <summary>
    /// React module that consumes bluetooth manager class
    /// </summary>
    [ReactModule]
    public class BleManager
    {
        #region React Events

        [ReactEvent]
        public Action<string> Event { get; set; }

        [ReactEvent]
        public Action<string, int> UserNotification { get; set; }

        [ReactEvent]
        public Action<bool> IsScanningEvent { get; set; }

        [ReactEvent]
        public Action<bool> IsConnectingEvent { get; set; }

        [ReactEvent]
        public Action<string> DeviceAdded { get; set; }

        [ReactEvent]
        public Action<string> DeviceRemoved { get; set; }

        [ReactEvent]
        public Action<string> DeviceUpdated { get; set; }

        [ReactEvent]
        public Action<string> DeviceEnumerationCompleted { get; set; }

        #endregion React Events

        #region React Constants

        [ReactConstant]
        public string Connected = @"C# BLE Module. this module lets you connects to Bluetooth Low Energy devices and communicates with the device.";

        #endregion React Constants

        #region Fields

        private DeviceWatcher deviceWatcher;

        private bool isScanning;

        private bool isConnecting;

        public int ScanningTimeout { get; set; }

        #endregion Fields

        #region Props

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

        /// <summary>
        /// Additional filters on discovered blue tooth devices.
        /// System.ItemNameDisplay will make sure only device containing MD in the name will be returned.
        /// </summary>
        //private readonly string requestedAqsFilters = "(System.ItemNameDisplay:~~\"MD\") AND (System.Devices.Aep.Bluetooth.Le.IsConnectable:=System.StructuredQueryType.Boolean#True OR System.Devices.Aep.IsConnected:=System.StructuredQueryType.Boolean#True)";
        private readonly string requestedAqsFilters = " (System.Devices.Aep.Bluetooth.Le.IsConnectable:=System.StructuredQueryType.Boolean#True OR System.Devices.Aep.IsConnected:=System.StructuredQueryType.Boolean#True)";

        public bool IsScanning
        {
            get => this.isScanning;
            private set
            {
                this.isScanning = value;
                IsScanningEvent(value);
            }
        }

        public bool IsConnecting
        {
            get => this.isConnecting;
            private set
            {
                this.isConnecting = value;
                IsConnectingEvent(value);
            }
        }

        private ObservableCollection<BleDevice> knownDevices = new ObservableCollection<BleDevice>();
        private List<DeviceInformation> UnknownDevices = new List<DeviceInformation>();


        private List<GattDeviceService> GattDeviceServicesList = new List<GattDeviceService>();
        private List<GattCharacteristic> GattCharacteristicList = new List<GattCharacteristic>();
        
        private GattDeviceService SelectedGattDeviceServices;
        private GattCharacteristic SelectedCharacteristic;

        #endregion Props

        #region React Methods

        /// <summary>
        /// Start scanning for sensor, removing all known sensor and readding all scanned Dorsavi devices
        /// </summary>
        /// <param name="timeoutMilliseconds"></param>

        [ReactMethod("StartScan")]
        public async void StartScanningAsync(int timeoutMilliseconds = 30)
        {
            if (!IsScanning)
            {
                Event("Start enumerating");
                StartBleDeviceWatcher();
                UserNotification($"Device watcher started.", (int)(int)NotifyType.StatusMessage);
            }
            else
            {
                UserNotification("User canceled scan", (int)(int)NotifyType.StatusMessage);
                StopBleDeviceWatcher();
                UserNotification($"Device watcher stopped.", (int)(int)NotifyType.StatusMessage);
            }
        }

        [ReactMethod("Connect")]
        public async Task<string> ConnectToDevice(string deviceId)
        {
            var response = new ServiceResponse<List<ServiceName>>();
            IsConnecting = true;
            if (!await ClearBluetoothLEDeviceAsync())
            {
                UserNotification("Error: Unable to reset state, try again.", (int)NotifyType.ErrorMessage);
                return JsonConvert.SerializeObject(response);
            }

            try
            {
                // BT_Code: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
                bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceId);

                if (bluetoothLeDevice == null)
                {
                    var message = "Failed to connect to device.";
                    UserNotification(message, (int)NotifyType.ErrorMessage);
                }
            }
            catch (Exception ex) when (ex.HResult == ErrorCodes.E_DEVICE_NOT_AVAILABLE)
            {
                var message = "Bluetooth radio is not on.";
                UserNotification(message, (int)NotifyType.ErrorMessage);
            }

            if (bluetoothLeDevice != null)
            {
                // Note: BluetoothLEDevice.GattServices property will return an empty list for unpaired devices. For all uses we recommend using the GetGattServicesAsync method.
                // BT_Code: GetGattServicesAsync returns a list of all the supported services of the device (even if it's not paired to the system).
                // If the services supported by the device are expected to change during BT usage, subscribe to the GattServicesChanged event.
                GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    response.Valid = true;

                    var services = result.Services;

                    UserNotification(String.Format("Found {0} services", services.Count), (int)NotifyType.StatusMessage);


                    var serviceNames = new List<ServiceName>();
                    var id = 1;
                    foreach (var service in services)
                    {
                        var serviceName = DisplayHelpers.GetServiceName(service);
                        serviceNames.Add(new ServiceName { Id = id, Name = serviceName });
                        GattDeviceServicesList.Add(service);
                        id +=1 ;
                    }
                    response.Content = serviceNames;
                }
                else
                {
                    UserNotification("Device unreachable", (int)NotifyType.ErrorMessage);
                }
            }
            IsConnecting = false;
            return JsonConvert.SerializeObject(response);
        }

        #endregion React Methods

        #region Methods

        #region Device discovery

        /// <summary>
        /// Starts a device watcher that looks for all nearby Bluetooth devices (paired or unpaired).
        /// Attaches event handlers to populate the device collection.
        /// </summary>
        private void StartBleDeviceWatcher()
        {
            // Additional properties we would like about the device.
            // Property strings are documented here https://msdn.microsoft.com/en-us/library/windows/desktop/ff521659(v=vs.85).aspx
            //string[] crequestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

            // BT_Code: Example showing paired and non-paired in a single query.
            //string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

            this.IsScanning = true;
            deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        requestedAqsFilters,
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

                this.IsScanning = false;
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
                    var bleDevice = FindBleDevice(deviceInfoUpdate.Id);
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
                    (int)NotifyType.StatusMessage);
            }
        }

        private async void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
            // Protect against race condition if the task runs after the app stopped the deviceWatcher.
            if (sender == deviceWatcher)
            {
                UserNotification($"No longer watching for devices.",
                        sender.Status == DeviceWatcherStatus.Aborted ? (int)NotifyType.ErrorMessage : (int)NotifyType.StatusMessage);
            }
        }

        #endregion Device discovery

        #region Enumerating Services

        private bool subscribedForNotifications = false;

        private BluetoothLEDevice bluetoothLeDevice = null;
        private GattCharacteristic selectedCharacteristic;
        private GattCharacteristic registeredCharacteristic;
        private GattPresentationFormat presentationFormat;

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

        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            // BT_Code: An Indicate or Notify reported that the value has changed.
            // Display the new value with a timestamp.
            var newValue = FormatValueByPresentation(args.CharacteristicValue, presentationFormat);
            var message = $"Value at {DateTime.Now:hh:mm:ss.FFF}: {newValue}";
            UserNotification(message, (int)NotifyType.StatusMessage);
            //CharacteristicLatestValue.Text = message;

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
                else if (selectedCharacteristic.Uuid.Equals(Codes.ResultCharacteristicUuid))
                {
                    return BitConverter.ToInt32(data, 0).ToString();
                }
                // No guarantees on if a characteristic is registered for notifications.
                else if (registeredCharacteristic != null)
                {
                    // This is our custom calc service Result UUID. Format it like an Int
                    if (registeredCharacteristic.Uuid.Equals(Codes.ResultCharacteristicUuid))
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

        /// <summary>
        /// Process the raw data received from the device into application usable data,
        /// according the the Bluetooth Heart Rate Profile.
        /// https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.heart_rate_measurement.xml&u=org.bluetooth.characteristic.heart_rate_measurement.xml
        /// This function throws an exception if the data cannot be parsed.
        /// </summary>
        /// <param name="data">Raw data received from the heart rate monitor.</param>
        /// <returns>The heart rate measurement value.</returns>
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

        #endregion Enumerating Services

        [ReactMethod("SelectBatteryService")]
        public async void SelectBattery()
        {
            GattDeviceService service = null;
            foreach (var item in GattDeviceServicesList)
            {
                var serviceName = DisplayHelpers.GetServiceName(item);
                if (serviceName == "Battery")
                {
                    service = item;
                    UserNotification("Battery Service Selected", (int)NotifyType.StatusMessage);
                }
            }


            GattCharacteristicList.Clear();

            IReadOnlyList<GattCharacteristic> characteristics = null;
            try
            {
                // Ensure we have access to the device.
                var accessStatus = await service.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    // BT_Code: Get all the child characteristics of a service. Use the cache mode to specify uncached characterstics only 
                    // and the new Async functions to get the characteristics of unpaired devices as well. 
                    var result = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        characteristics = result.Characteristics;
                    }
                    else
                    {
                        UserNotification("Error accessing service.", (int)NotifyType.ErrorMessage);

                        // On error, act as if there are no characteristics.
                        characteristics = new List<GattCharacteristic>();
                    }
                }
                else
                {
                    // Not granted access
                    UserNotification("Error accessing service.", (int)NotifyType.ErrorMessage);

                    // On error, act as if there are no characteristics.
                    characteristics = new List<GattCharacteristic>();

                }
            }
            catch (Exception ex)
            {
                UserNotification("Restricted service. Can't read characteristics: " + ex.Message,
                    (int)NotifyType.ErrorMessage);
                // On error, act as if there are no characteristics.
                characteristics = new List<GattCharacteristic>();
            }

            foreach (GattCharacteristic c in characteristics)
            {
                GattCharacteristicList.Add(c);
            }
            
        }

        [ReactMethod("SelectBatteryLevel")]
        public async void SelectBatteryLevel()
        {
            //BatteryLevel
            selectedCharacteristic = null;
            foreach (var item in GattCharacteristicList)
            {
                var characteristicName = DisplayHelpers.GetCharacteristicName(item);
                if (characteristicName == "BatteryLevel")
                {
                    selectedCharacteristic = item;
                    UserNotification("Battery Level Characteristic selected", (int)NotifyType.StatusMessage);
                }
            }

            if (selectedCharacteristic == null)
            {
                UserNotification("No characteristic selected", (int)NotifyType.ErrorMessage);
                return;
            }

            // Get all the child descriptors of a characteristics. Use the cache mode to specify uncached descriptors only 
            // and the new Async functions to get the descriptors of unpaired devices as well. 
            var result = await selectedCharacteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
            if (result.Status != GattCommunicationStatus.Success)
            {
                UserNotification("Descriptor read failure: " + result.Status.ToString(), (int)NotifyType.ErrorMessage);
            }

            // BT_Code: There's no need to access presentation format unless there's at least one. 
            presentationFormat = null;
            if (selectedCharacteristic.PresentationFormats.Count > 0)
            {

                if (selectedCharacteristic.PresentationFormats.Count.Equals(1))
                {
                    // Get the presentation format since there's only one way of presenting it
                    presentationFormat = selectedCharacteristic.PresentationFormats[0];
                }
                else
                {
                    // It's difficult to figure out how to split up a characteristic and encode its different parts properly.
                    // In this case, we'll just encode the whole thing to a string to make it easy to print out.
                }
            }
        }

        [ReactMethod("SubscribeChanges")]
        public async void ValueChangedSubscribeToggle_Click()
        {
            if (!subscribedForNotifications)
            {
                // initialize status
                GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
                var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
                if (selectedCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
                {
                    cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
                }

                else if (selectedCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
                }

                try
                {
                    // BT_Code: Must write the CCCD in order for server to send indications.
                    // We receive them in the ValueChanged event handler.
                    status = await selectedCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);

                    if (status == GattCommunicationStatus.Success)
                    {
                        AddValueChangedHandler();
                        UserNotification("Successfully subscribed for value changes", (int)NotifyType.StatusMessage);
                    }
                    else
                    {
                        UserNotification($"Error registering for value changes: {status}", (int)NotifyType.ErrorMessage);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support indicate, but it actually doesn't.
                    UserNotification(ex.Message, (int)NotifyType.ErrorMessage);
                }
            }
            else
            {
                try
                {
                    // BT_Code: Must write the CCCD in order for server to send notifications.
                    // We receive them in the ValueChanged event handler.
                    // Note that this sample configures either Indicate or Notify, but not both.
                    var result = await
                            selectedCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                GattClientCharacteristicConfigurationDescriptorValue.None);
                    if (result == GattCommunicationStatus.Success)
                    {
                        subscribedForNotifications = false;
                        RemoveValueChangedHandler();
                        UserNotification("Successfully un-registered for notifications", (int)NotifyType.StatusMessage);
                    }
                    else
                    {
                        UserNotification($"Error un-registering for notifications: {result}", (int)NotifyType.ErrorMessage);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support notify, but it actually doesn't.
                    UserNotification(ex.Message, (int)NotifyType.ErrorMessage);
                }
            }
        }
        private void AddValueChangedHandler()
        {
            
            if (!subscribedForNotifications)
            {
                registeredCharacteristic = selectedCharacteristic;
                registeredCharacteristic.ValueChanged += Characteristic_ValueChanged;
                subscribedForNotifications = true;
            }
        }

        private void RemoveValueChangedHandler()
        {
            if (subscribedForNotifications)
            {
                registeredCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                registeredCharacteristic = null;
                subscribedForNotifications = false;
            }
        }

        #endregion Methods
    }
}