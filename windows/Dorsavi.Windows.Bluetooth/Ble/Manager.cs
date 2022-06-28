using Dorsavi.Windows.Bluetooth.Constants;
using Dorsavi.Windows.Bluetooth.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace Dorsavi.Windows.Bluetooth.Ble
{
    public class BleManager : INotifyPropertyChanged, IDisposable
    {
        #region Fields

        private bool isConnecting;

        #endregion Fields


        public event PropertyChangedEventHandler PropertyChanged;

        #region Props

        public bool IsConnecting
        {
            get => this.isConnecting;
            private set
            {
                this.isConnecting = value;
                //IsConnectingEvent(value);
            }
        }

        public static ObservableCollection<BleDevice> ConnectedDevices;

        public BleManager()
        {
            ConnectedDevices = new ObservableCollection<BleDevice>();
            ConnectedDevices.CollectionChanged += ConnectedDevicesChanged;
        }

        #endregion Props

        #region Public

        public async Task<ServiceResponse<BleDevice>> ConnectToDeviceAsync(string deviceId)
        {
            var response = new ServiceResponse<BleDevice>();
            IsConnecting = true;
            if (!await ClearBluetoothLEDeviceAsync())
            {
                response.Message.Add("Error: Unable to reset state, try again.");
                response.Valid = false;
                return response;
            }
            var device = ConnectedDevices.FirstOrDefault(x => x.Id == deviceId);
            if (device == null)
            {
                device = new BleDevice();
                var connectResponse = await device.ConnectAsync(deviceId);
                if (connectResponse.Valid)
                    ConnectedDevices.Add(device);

                response.CopyFrom(connectResponse);
                //UserNotification(String.Format($"Connected to device {device.Name}, found {device.Services.Count} services"), (int)NotifyType.StatusMessage);
            }
            else
            {
                response.Message.Add($"Device {device.Name} already connected");
                response.Valid = false;
                //UserNotification(String.Format($"Device {bluetoothLeDevice.DeviceInformation.Name} already connected"), (int)NotifyType.StatusMessage);
            }
            IsConnecting = false;
            return response;
        }

        public async Task<ServiceResponse<List<BleDeviceCharacteristic>>> GetBleCharacteristicAsync(string deviceId, string serviceId)
        {
            var response = new ServiceResponse<List<BleDeviceCharacteristic>>();

            var deviceValid = ValidateConnectedDevice(deviceId);
            var deviceServiceValid = ValidateConnectedDeviceService(deviceId, serviceId);

            if (!deviceServiceValid.Valid)
                return response;

            var selectedDevice = deviceValid.Content;
            var service = deviceServiceValid.Content;

            try
            {
                // THIS IS V6C RETURNS hardcoded characteristic
                //if ((selectedDevice.Name.StartsWith(SensorHardwareTypeNames.MDE) ||
                // selectedDevice.Name.StartsWith(SensorHardwareTypeNames.MDD) ||
                // selectedDevice.Name.StartsWith(SensorHardwareTypeNames.MDM)) && service.UUid == V6CServiceUuId.Custom.UUid)
                //{
                //    var accessStatus = await service.Service.RequestAccessAsync();
                //    if (accessStatus == DeviceAccessStatus.Allowed)
                //    {
                //        response.Content = V6CCharacteristic.List.Select(x => new BleDeviceCharacteristic
                //        {
                //            Name = x.Name,
                //            Uuid = x.UUid,
                //            DeviceId = deviceId,
                //            ServiceId = serviceId
                //        }).ToList();

                //        response.Valid = true;
                //        return response; ;
                //    }
                //}

                return await service.GetCharacteristicAsync();
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Message.Add("Cannot find device, device may be disconnected");
            }

            return response;
        }

        public async Task<ServiceResponse<List<BleDeviceCharacteristicDescriptor>>> GetCharacteristicDescriptor(string deviceId, string serviceId, string characteristicUuid)
        {
            var resp = new ServiceResponse<List<BleDeviceCharacteristicDescriptor>>();
            var deviceServiceValid = ValidateConnectedDeviceService(deviceId, serviceId);
            if (!deviceServiceValid.Valid)
            {
                return resp.CopyFrom(deviceServiceValid);
            }
            var service = deviceServiceValid.Content;

            try
            {
                if (service.BleCharacteristics == null)
                    await service.GetCharacteristicAsync();
                var selectedCharacteristic = service.BleCharacteristics.FirstOrDefault(x => x.Uuid.ToString() == characteristicUuid);

                if (selectedCharacteristic != null)
                {
                    return await selectedCharacteristic.GetCharacteristicDescriptorsAsync();

                }
                resp.Valid = false;
                resp.Message.Add("Cannot find characteristic");
            }
            catch (Exception ex)
            {
                resp.Valid = false;
                resp.Message.Add($"invalid operation err: {ex.Message}");
            }

            return resp;
        }

        public async Task<ServiceResponse<string>> SendCommand(string deviceId, string serviceId, string characteristicUuid, CharacteristicPropertiesEnum descriptor, string commandValue = "", bool sendAsInt = false)
        {
            var resp = new ServiceResponse<string>();
            var deviceServiceValid = ValidateConnectedDeviceService(deviceId, serviceId);
            if (!deviceServiceValid.Valid)
            {
                resp.Content = "";// resp.CopyFrom(deviceServiceValid);
            }
            var service = deviceServiceValid.Content;

            try
            {
                if (service.BleCharacteristics == null)
                    await service.GetCharacteristicAsync();
                var selectedCharacteristic = service.BleCharacteristics.FirstOrDefault(x => x.Uuid.ToString() == characteristicUuid);
                if (selectedCharacteristic != null)
                {
                    if (descriptor == CharacteristicPropertiesEnum.Read)
                    {
                        return await selectedCharacteristic.CharacteristicRead();
                    }
                    else if (descriptor == CharacteristicPropertiesEnum.Write)
                    {
                        return await selectedCharacteristic.CharacteristicWrite(commandValue, sendAsInt);
                    }
                    else if (descriptor == CharacteristicPropertiesEnum.Notify)
                    {
                        return resp.CopyFrom(await selectedCharacteristic.ToogleSubscribe());
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Valid = false;
                resp.Message.Add($"Exception in running command, exception: {ex.Message}");
            }

            return resp;
        }

        #endregion Public

        #region Private





        private async Task<bool> ClearBluetoothLEDeviceAsync()
        {
            //if (subscribedForNotifications)
            //{
            //    // Need to clear the CCCD from the remote device so we stop receiving notifications
            //    var result = await registeredCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
            //    if (result != GattCommunicationStatus.Success)
            //    {
            //        return false;
            //    }
            //    else
            //    {
            //        selectedCharacteristic.ValueChanged -= Characteristic_ValueChanged;
            //        subscribedForNotifications = false;
            //    }
            //}
            //bluetoothLeDevice?.Dispose();
            //bluetoothLeDevice = null;
            return true;
        }

        private ServiceResponse<BleDevice> ValidateConnectedDevice(string deviceId)
        {
            var resp = new ServiceResponse<BleDevice>();
            var device = ConnectedDevices.FirstOrDefault(x => x.Id == deviceId);
            if (device != null)
            {
                resp.Valid = true;
                resp.Content = device;
            }
            else
            {
                resp.Message.Add($"Device with id {deviceId} not found.");
            }
            return resp;
        }

        private ServiceResponse<BleDeviceServices> ValidateConnectedDeviceService(string deviceId, string serviceId)

        {
            var resp = new ServiceResponse<BleDeviceServices>();
            var device = ConnectedDevices.FirstOrDefault(x => x.Services.Any(y => y.DeviceId == deviceId));

            if (device != null)
            {
                var service = device.Services.FirstOrDefault(x => x.UUid == serviceId);
                if (service != null)
                {
                    resp.Valid = true;
                    resp.Content = service;
                    return resp;
                }
                resp.Message.Add($"Device with id {deviceId} and service {serviceId} is not found.");
            }
            else
                resp.Message.Add($"Device with id {deviceId} is not found.");

            return resp;
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
            //else if (data != null)
            //{
            //    // We don't know what format to use. Let's try some well-known profiles, or default back to UTF-8.
            //    if (selectedCharacteristic.Uuid.Equals(GattCharacteristicUuids.HeartRateMeasurement))
            //    {
            //        try
            //        {
            //            return "Heart Rate: " + ParseHeartRateValue(data).ToString();
            //        }
            //        catch (ArgumentException)
            //        {
            //            return "Heart Rate: (unable to parse)";
            //        }
            //    }
            //    else if (selectedCharacteristic.Uuid.Equals(GattCharacteristicUuids.BatteryLevel))
            //    {
            //        try
            //        {
            //            // battery level is encoded as a percentage value in the first byte according to
            //            // https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.battery_level.xml
            //            return "Battery Level: " + data[0].ToString() + "%";
            //        }
            //        catch (ArgumentException)
            //        {
            //            return "Battery Level: (unable to parse)";
            //        }
            //    }
            //    // This is our custom calc service Result UUID. Format it like an Int
            //    else if (selectedCharacteristic.Uuid.Equals(Constants.ResultCharacteristicUuid))
            //    {
            //        return BitConverter.ToInt32(data, 0).ToString();
            //    }
            //    // No guarantees on if a characteristic is registered for notifications.
            //    else if (registeredCharacteristic != null)
            //    {
            //        // This is our custom calc service Result UUID. Format it like an Int
            //        if (registeredCharacteristic.Uuid.Equals(Constants.ResultCharacteristicUuid))
            //        {
            //            return BitConverter.ToInt32(data, 0).ToString();
            //        }
            //    }
            //    else
            //    {
            //        return CryptographicBuffer.EncodeToHexString(buffer);

            //        try
            //        {
            //            return "Unknown format: " + Encoding.UTF8.GetString(data);
            //        }
            //        catch (ArgumentException)
            //        {
            //            return "Unknown format";
            //        }
            //    }
            //}
            else
            {
                return "Empty data received";
            }
            return "Unknown format";
        }

        private void ConnectedDevicesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool connectedChanges = false;
            if (e.NewItems != null)
            {
                foreach (BleDevice item in e.NewItems)
                {
                    item.PropertyChanged += ConnectedDevicelPropertyChanged;
                }
                connectedChanges = true;
            }
            if (e.OldItems != null)
            {
                foreach (var y in e.OldItems)
                {
                    // device removed
                }
                connectedChanges = true;
            }
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                //do something
            }

            if (connectedChanges)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConnectedDevices"));
        }

        public void ConnectedDevicelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // device disconnected
            if (sender.GetType() == typeof(BleDevice))
            {
                BleDevice device = (BleDevice)sender;
                if (e.PropertyName == "IsConnected" && device.IsConnected.HasValue && !device.IsConnected.Value)
                    ConnectedDevices.Remove(device);
                else
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConnectedDevices"));
            }

            //This will get called when the property of an object inside the collection changes
        }

        public void Dispose()
        {
            ConnectedDevices.CollectionChanged -= ConnectedDevicesChanged;
            foreach (var item in ConnectedDevices)
            {
                item.Dispose();
            }
        }

        #endregion Private
    }
}