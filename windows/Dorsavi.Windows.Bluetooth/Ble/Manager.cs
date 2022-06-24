using Dorsavi.Windows.Bluetooth.Constants;
using Dorsavi.Windows.Bluetooth.Models;
using Dorsavi.Windows.Bluetooth.Sensor.Models;
using Dorsavi.Windows.Bluetooth.Sensor.V6c;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace Dorsavi.Windows.Bluetooth.Ble
{
    public class BleManager
    {
        #region Fields

        private bool isConnecting;

        #endregion
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

        public static List<BleDevice> ConnectedDevices;

        public BleManager()
        {
            ConnectedDevices = new List<BleDevice>();
        }

        #endregion


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
            BluetoothLEDevice bluetoothLeDevice = null;
            try
            {
                // BT_Code: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
                bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceId);

                if (bluetoothLeDevice == null)
                {
                    response.Message.Add("Failed to connect to device.");
                    response.Valid = false;
                    return response;
                }
            }
            catch (Exception ex) when (ex.HResult == ErrorCodes.E_DEVICE_NOT_AVAILABLE)
            {
                response.Message.Add("Bluetooth radio is not on.");
                response.Valid = false;
                return response;
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
                        //UserNotification(String.Format($"Connected to device {device.Name}, found {device.Services.Count} services"), (int)NotifyType.StatusMessage);
                    }
                    else
                    {
                        response.Message.Add($"Device {bluetoothLeDevice.DeviceInformation.Name} already connected");
                        //UserNotification(String.Format($"Device {bluetoothLeDevice.DeviceInformation.Name} already connected"), (int)NotifyType.StatusMessage);
                    }
                    response.Content = device;
                }
                else
                {
                    response.Message.Add("Device unreachable");
                    response.Valid = false;

                }
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
                        return response; ;
                    }
                }
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
                    try
                    {
                        var result = await selectedCharacteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
                        if (result.Status != GattCommunicationStatus.Success)
                        {
                            resp.Message.Add("Descriptor read failure: " + result.Status.ToString());
                            return resp;
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

                        var characteristicDescriptors = new List<BleDeviceCharacteristicDescriptor>();
                        // Enable/disable operations based on the GattCharacteristicProperties.
                        foreach (var item in CharacteristicPropertiesEnum.List)
                        {
                            var gattCharProp = (GattCharacteristicProperties)item.CharacteristicValue;
                            if (selectedCharacteristic.CharacteristicProperties.HasFlag(gattCharProp))
                            {
                                characteristicDescriptors.Add(new BleDeviceCharacteristicDescriptor()
                                {
                                    CharacteristicUuid = characteristicUuid,
                                    DeviceId = deviceId,
                                    ServiceId = serviceId,
                                    Value = item.Name
                                });
                            }
                        }
                        if (characteristicDescriptors.Count > 1 && characteristicDescriptors.Any(x => x.Value == CharacteristicPropertiesEnum.None.CharacteristicValue.ToString()))
                        {
                            characteristicDescriptors.Remove(characteristicDescriptors.FirstOrDefault(x => x.Value == CharacteristicPropertiesEnum.None.CharacteristicValue.ToString()));
                        }
                        resp.Valid = true;
                        resp.Content = characteristicDescriptors;
                    }
                    catch (Exception ex)
                    {
                        resp.Message.Add($"Error, {ex.Message}");
                        // On error, act as if there are no characteristics.
                        resp.Content = null;
                    }
                    return resp;
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

        public async Task<string> SendCommand(string deviceId, string serviceId, string characteristicUuid, CharacteristicPropertiesEnum descriptor)
        {
            var deviceValid = ValidateConnectedDevice(deviceId);
            if (!deviceValid.Valid)
                return JsonConvert.SerializeObject(deviceValid);

            return JsonConvert.SerializeObject(deviceValid);
        }

        #endregion

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



        #endregion
    }
}
