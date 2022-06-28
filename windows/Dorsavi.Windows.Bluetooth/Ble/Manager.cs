using Dorsavi.Windows.Bluetooth.Constants;
using Dorsavi.Windows.Bluetooth.Models;
using Dorsavi.Windows.Framework.Infrastructure;
using Dorsavi.Windows.Framework.Model;
using Dorsavi.Windows.Framework.PubSub;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Dorsavi.Windows.Bluetooth.Ble
{
    public class BleManager : IDisposable
    {
        private readonly List<Publisher> _publishers;
        private readonly Subscriber _subscriber;

        #region Fields

        private bool isConnecting;

        #endregion Fields

        //public event PropertyChangedEventHandler PropertyChanged;

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
            _publishers = Singleton<List<Publisher>>.Instance;
            _subscriber = Singleton<Subscriber>.Instance;

            var publisher = new Publisher(this.GetType().Name, PublisherType.ConnectedDevice);
            _subscriber.Subscribe(publisher);
            _publishers.Add(publisher);

            ConnectedDevices = new ObservableCollection<BleDevice>();
            ConnectedDevices.CollectionChanged += ConnectedDevicesChanged;

            _subscriber.NotificationReceived += NotificationReceived;
        }

        private void NotificationReceived(object sender, EventArgs e)
        {
            if (e.GetType() == typeof(NotificationEvent))
            {
                var notificationEvent = (NotificationEvent)e;
                if (notificationEvent.PublisherType == PublisherType.PropertyChanged)
                {
                    var device = ConnectedDevices.FirstOrDefault(x => x.Name == notificationEvent.NotificationMessage);
                    if (device != null || device.IsConnected.HasValue && !device.IsConnected.Value)
                        ConnectedDevices.Remove(device);
                }
            }
        }

        #endregion Props

        #region Public

        public async Task<ServiceResponse<BleDevice>> ConnectToDeviceAsync(string deviceId)
        {
            var response = new ServiceResponse<BleDevice>();
            IsConnecting = true;
            //if (!await ClearBluetoothLEDeviceAsync())
            //{
            //    response.Message.Add("Error: Unable to reset state, try again.");
            //    response.Valid = false;
            //    return response;
            //}
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

        //private async Task<bool> ClearBluetoothLEDeviceAsync()
        //{
        //    //if (subscribedForNotifications)
        //    //{
        //    //    // Need to clear the CCCD from the remote device so we stop receiving notifications
        //    //    var result = await registeredCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
        //    //    if (result != GattCommunicationStatus.Success)
        //    //    {
        //    //        return false;
        //    //    }
        //    //    else
        //    //    {
        //    //        selectedCharacteristic.ValueChanged -= Characteristic_ValueChanged;
        //    //        subscribedForNotifications = false;
        //    //    }
        //    //}
        //    //bluetoothLeDevice?.Dispose();
        //    //bluetoothLeDevice = null;
        //    return true;
        //}

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

        private void ConnectedDevicesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool connectedChanges = false;
            var publisher = _publishers.FirstOrDefault(x => x.PublisherType == PublisherType.ConnectedDevice);

            if (e.NewItems != null || e.OldItems != null)
            {
                connectedChanges = true;
            }
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                //do something
            }

            if (connectedChanges)
                publisher.Publish("Connected device changes");
        }

        public void Dispose()
        {
            var publisher = _publishers.FirstOrDefault(x => x.PublisherType == PublisherType.ConnectedDevice);
            _subscriber.Unsubscribe(publisher);
            _publishers.Remove(publisher);

            foreach (var item in ConnectedDevices)
            {
                item.Dispose();
            }
        }

        #endregion Private

        //public void ConnectedDevicelPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    // device disconnected
        //    if (sender.GetType() == typeof(BleDevice))
        //    {
        //        BleDevice device = (BleDevice)sender;
        //        if (e.PropertyName == "IsConnected" && device.IsConnected.HasValue && !device.IsConnected.Value)
        //            ConnectedDevices.Remove(device);
        //        else
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConnectedDevices"));
        //    }

        //    //This will get called when the property of an object inside the collection changes
        //}
    }
}