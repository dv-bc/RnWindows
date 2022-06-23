using Microsoft.ReactNative.Managed;
using Newtonsoft.Json;
using rnwindowsminimal.Constants;
using rnwindowsminimal.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace rnwindowsminimal.Bluetooth
{
    //dang code
    //BleSensor 

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
        public Action<string> KnownDeviceUpdated { get; set; }

        [ReactEvent]
        public Action<string> ConnectedDevicesUpdated { get; set; }

        [ReactEvent]
        public Action<string> DeviceRemoved { get; set; }

        [ReactEvent]
        public Action<string> DeviceDisconnect { get; set; }

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

        private bool KnownDevicesUpdated;

        private static System.Timers.Timer Timer;









        //CancellationTokenSource source = new CancellationTokenSource();
        //CancellationToken token;

        #endregion Fields

        #region Props

        public int ScanningTimeout { get; set; }

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
        private readonly string requestedAqsFilters = "(System.ItemNameDisplay:~~\"MD\") AND (System.Devices.Aep.Bluetooth.Le.IsConnectable:=System.StructuredQueryType.Boolean#True OR System.Devices.Aep.IsConnected:=System.StructuredQueryType.Boolean#True)";
        //private readonly string requestedAqsFilters = " (System.Devices.Aep.Bluetooth.Le.IsConnectable:=System.StructuredQueryType.Boolean#True OR System.Devices.Aep.IsConnected:=System.StructuredQueryType.Boolean#True)";

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
        private List<BleDevice> ConnectedDevices = new List<BleDevice>();
        private Dictionary<string, GattDeviceServicesResult> DeviceServices = new Dictionary<string, GattDeviceServicesResult>();



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
            var response = new ServiceResponse<BleDevice>();
            IsConnecting = true;
            if (!await ClearBluetoothLEDeviceAsync())
            {
                //ConnectedDevices.First().GetGattServicesAsync
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
                    var device = ConnectedDevices.FirstOrDefault(x => x.Id == bluetoothLeDevice.DeviceInformation.Id);
                    if (device == null)
                    {
                        device = new BleDevice(bluetoothLeDevice.DeviceInformation, bluetoothLeDevice, result);
                        ConnectedDevices.Add(device);
                        UserNotification(String.Format($"Connected to device {device.Name}, found {device.Services.Count} services"), (int)NotifyType.StatusMessage);

                    }
                    else
                    {
                        response.Message.Add($"Device {bluetoothLeDevice.DeviceInformation.Name} already connected");
                        UserNotification(String.Format($"Device {bluetoothLeDevice.DeviceInformation.Name} already connected"), (int)NotifyType.StatusMessage);
                    }
                    response.Content = device;
                }
                else
                {

                    UserNotification("Device unreachable", (int)NotifyType.ErrorMessage);
                }
            }
            IsConnecting = false;
            ConnectedDevicesUpdated(JsonConvert.SerializeObject(ConnectedDevices));
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

            string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\") AND (System.ItemNameDisplay:~~\"MD\")";
            this.IsScanning = true;
            var trequestedAqsFilterss = "(System.ItemNameDisplay:~~\"MD\" OR System.ItemNameDisplay:~~\"B\") AND (System.Devices.Aep.Bluetooth.Le.IsConnectable:=System.StructuredQueryType.Boolean#True )";
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


            // Create a timer with a two second interval.
            Timer = new System.Timers.Timer(2000);
            Timer.Elapsed += OnTimedEvent;
            Timer.AutoReset = true;
            Timer.Enabled = true;



        }
        // we are updating front end every 2s and only when known devices list is updated 
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (this.isScanning && this.KnownDevicesUpdated)
            {
                KnownDeviceUpdated(JsonConvert.SerializeObject(knownDevices));
            }
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
                        // If device has a friendly name display it immediately.
                        knownDevices.Add(bleDevice);
                        KnownDevicesUpdated = true;
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
                        KnownDevicesUpdated = true;
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
                            // If device has a friendly name display it immediately.
                            knownDevices.Add(device);
                            UnknownDevices.Remove(deviceInfo);
                            KnownDevicesUpdated = true;
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

        private async void devicedisconnect(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
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
                        KnownDevicesUpdated = true;
                    }

                    DeviceInformation deviceInfo = FindUnknownDevices(deviceInfoUpdate.Id);
                    if (deviceInfo != null)
                    {
                        UnknownDevices.Remove(deviceInfo);
                    }
                }
            }
        }

        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {
            // Protect against race condition if the task runs after the app stopped the deviceWatcher.
            if (sender == deviceWatcher)
            {
                var message = $"{knownDevices.Count} devices found. Enumeration completed.";
                DeviceEnumerationCompleted(message);
                UserNotification(message,
                    (int)NotifyType.StatusMessage);

                this.IsScanning = false;

                Timer.Stop();
                Timer?.Dispose();

                KnownDeviceUpdated(JsonConvert.SerializeObject(knownDevices));

            }
        }

        private async void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
            // Protect against race condition if the task runs after the app stopped the deviceWatcher.
            if (sender == deviceWatcher)
            {
                UserNotification($"No longer watching for devices.",
                        sender.Status == DeviceWatcherStatus.Aborted ? (int)NotifyType.ErrorMessage : (int)NotifyType.StatusMessage);

                this.IsScanning = false;
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
                //else if (selectedCharacteristic.Uuid.Equals(V6CCharacteristicUuId.from.ResultCharacteristicUuid))
                //{
                //    return BitConverter.ToInt32(data, 0).ToString();
                //}
                //// No guarantees on if a characteristic is registered for notifications.
                //else if (registeredCharacteristic != null)
                //{
                //    // This is our custom calc service Result UUID. Format it like an Int
                //    if (registeredCharacteristic.Uuid.Equals(Codes.ResultCharacteristicUuid))
                //    {
                //        return BitConverter.ToInt32(data, 0).ToString();
                //    }
                //}
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


        [ReactMethod("GetBleCharacteristic")]
        public async Task<string> GetBleCharacteristic(string deviceId, string serviceId)
        {
            var response = new ServiceResponse<List<BleDeviceCharacteristic>>();
            var selectedDevice = ConnectedDevices.Where(x => x.Services.Any(y => y.DeviceId == deviceId)).FirstOrDefault();

            // V6C only
            if (selectedDevice != null)
            {
                var service = selectedDevice.Services.FirstOrDefault(x => x.UUid == serviceId);
                if (service == null)
                {
                    response.Valid = false;
                    response.Message.Add("Cannot find device service");
                }
                try
                {
                    // THIS IS V6C RETURNS hardcoded characteristic
                    if ((selectedDevice.Name.StartsWith(SensorHardwareTypeNames.MDE) ||
                     selectedDevice.Name.StartsWith(SensorHardwareTypeNames.MDD) ||
                     selectedDevice.Name.StartsWith(SensorHardwareTypeNames.MDM)) && service.UUid == V6CServiceUuId.Custom.UUid)
                    {
                        var accessStatus = await service.Service.RequestAccessAsync();
                        if (accessStatus == DeviceAccessStatus.Allowed)
                        {
                            response.Content = V6CCharacteristic.List.Select(x => new BleDeviceCharacteristic
                            {
                                Name = x.Name,
                                Uuid = x.UUid,
                                DeviceId = deviceId,
                                ServiceId = serviceId
                            }).ToList();

                            response.Valid = true;
                            return JsonConvert.SerializeObject(response);
                        }
                    }
                    return JsonConvert.SerializeObject(await service.GetCharacteristicAsync());
                }
                catch (Exception ex)
                {
                    response.Valid = false;
                    response.Message.Add("Cannot find device, device may be disconnected");
                }
            }
            else
            {
                response.Valid = false;
                response.Message.Add("Cannot find device");
            }



            return JsonConvert.SerializeObject(response);

        }


        [ReactMethod("GetCharacteristicDescriptor")]
        public async Task<string> GetCharacteristicDescriptor(string deviceId, string serviceId, string characteristicUuid)
        {
            var resp = new ServiceResponse<List<string>>();
            var selectedDevice = ConnectedDevices.FirstOrDefault(x => x.Id == deviceId);

            if (selectedDevice != null)
            {
                var service = selectedDevice.Services.FirstOrDefault(x => x.UUid == serviceId);
                if (service == null)
                {
                    resp.Valid = false;
                    resp.Message.Add("Cannot find services");
                }
                try
                {
                    if (service.BleCharacteristics == null)
                        await service.GetCharacteristicAsync();
                    var selectedCharacteristic = service.BleCharacteristics.FirstOrDefault(x => x.Uuid.ToString() == characteristicUuid);

                    if (selectedCharacteristic != null)
                    {
                        try
                        {
                            var result = await selectedCharacteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
                            if (result.Status != GattCommunicationStatus.Success)
                            {
                                resp.Message.Add("Descriptor read failure: " + result.Status.ToString());
                                return JsonConvert.SerializeObject(resp);
                            }
                            GattPresentationFormat presentationFormat;
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

                            var characteristicDescriptors = new List<KeyValuePair<string, uint>>();
                            // Enable/disable operations based on the GattCharacteristicProperties.
                            foreach (var item in CharacteristicPropertiesEnum.List)
                            {
                                var gattCharProp = (GattCharacteristicProperties)item.CharacteristicValue;
                                if (selectedCharacteristic.CharacteristicProperties.HasFlag(gattCharProp))
                                {
                                    characteristicDescriptors.Add(new KeyValuePair<string, uint>(item.Name, item.CharacteristicValue));
                                }
                            }
                            if (characteristicDescriptors.Count > 1 && characteristicDescriptors.Any(x => x.Value == CharacteristicPropertiesEnum.None.CharacteristicValue))
                            {
                               characteristicDescriptors.Remove(characteristicDescriptors.FirstOrDefault(x => x.Value == CharacteristicPropertiesEnum.None.CharacteristicValue));
                            }
                            resp.Valid = true;
                            resp.Content = characteristicDescriptors.Select(x=>x.Key).ToList();

                        }
                        catch (Exception ex)
                        {
                            resp.Message.Add($"Error, {ex.Message}");
                            // On error, act as if there are no characteristics.
                            resp.Content = null;
                        }
                        return JsonConvert.SerializeObject(resp);
                    } 
                    resp.Valid = false;
                    resp.Message.Add("Cannot find characteristic");
                }
                catch (Exception ex)
                {
                    resp.Valid = false;
                    resp.Message.Add("Cannot find device");
                }
            }
            else
            {
                resp.Valid = false;
                resp.Message.Add("Cannot find device");
            }



            return JsonConvert.SerializeObject(resp);

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
                        if (!subscribedForNotifications)
                        {
                            registeredCharacteristic = selectedCharacteristic;
                            registeredCharacteristic.ValueChanged += Characteristic_ValueChanged;
                            subscribedForNotifications = true;
                        }
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