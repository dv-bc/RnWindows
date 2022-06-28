using Dorsavi.Windows.Bluetooth.Constants;
using Dorsavi.Windows.Bluetooth.Models;
using Dorsavi.Windows.Bluetooth.Sensor.Models;
using Dorsavi.Windows.Bluetooth.Sensor.V6c;
using Dorsavi.Windows.Framework.Infrastructure;
using Dorsavi.Windows.Framework.PubSub;
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
    public class BleDevice : IDisposable
    {
        private readonly List<Publisher> _publishers;
        private readonly Subscriber _subscriber;

        public BleDevice()
        {
            _publishers = Singleton<List<Publisher>>.Instance;
            _subscriber = Singleton<Subscriber>.Instance;
        }

        public BleDevice(DeviceInformation deviceInfoIn) : this()
        {
            DeviceInformation = deviceInfoIn;
        }

        public DeviceInformation DeviceInformation { get; private set; }

        [JsonIgnore]
        public BluetoothLEDevice BluetoothLEDevice { get; private set; }

        public List<BleDeviceServices> Services { get; set; }

        public string Id => DeviceInformation?.Id ?? "";
        public string Name => DeviceInformation?.Name ?? "";
        public bool? IsPaired => DeviceInformation?.Pairing.IsPaired;

        public int? Rssi
        {
            get
            {
                try
                {
                    if (int.TryParse((string)DeviceInformation?.Properties["System.Devices.Aep.SignalStrength"], out int rsii))
                        return rsii;
                    return null;
                }
                catch (System.Exception)
                {
                    return null;
                }
            }
        }

        public bool? IsConnected
        {
            get
            {
                try
                {
                    return this.BluetoothLEDevice != null ? this.BluetoothLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected : false;
                    //return (bool?)DeviceInformation?.Properties["System.Devices.Aep.IsConnected"] == true;
                }
                catch (System.Exception)
                {
                    return null;
                }
            }
        }

        public bool? IsConnectable
        {
            get
            {
                try
                {
                    return (bool?)DeviceInformation?.Properties["System.Devices.Aep.Bluetooth.Le.IsConnectable"] == true;
                }
                catch (System.Exception)
                {
                    return null;
                }
            }
        }

        public IReadOnlyDictionary<string, object> Properties => DeviceInformation?.Properties;

        public void Update(DeviceInformationUpdate deviceInfoUpdate)
        {
            DeviceInformation.Update(deviceInfoUpdate);

            //OnPropertyChanged("Id");
            //OnPropertyChanged("Name");
            //OnPropertyChanged("DeviceInformation");
            //OnPropertyChanged("IsPaired");
            //OnPropertyChanged("IsConnected");
            //OnPropertyChanged("Properties");
            //OnPropertyChanged("IsConnectable");
            //OnPropertyChanged("Rssi");
        }

        //protected void OnPropertyChanged(string name)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        //}

        public async Task<ServiceResponse> ConnectAsync(string deviceId)
        {
            var response = new ServiceResponse<BleDevice>();
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
                    this.BluetoothLEDevice = bluetoothLeDevice;
                    this.DeviceInformation = bluetoothLeDevice.DeviceInformation;

                    if (Services == null)
                    {
                        Services = new List<BleDeviceServices>();
                    }

                    foreach (var service in result.Services)
                    {
                        if ((Name.StartsWith(SensorHardwareTypeNames.MDE) || Name.StartsWith(SensorHardwareTypeNames.MDD) ||
                                Name.StartsWith(SensorHardwareTypeNames.MDM)))
                        {
                            Services.Add(new V6cDeviceServices(service, Id));
                        }
                        else
                        {
                            Services.Add(new BleDeviceServices(service, Id));
                        }
                    }
                    response.Valid = true;
                    //UserNotification(String.Format($"Connected to device {device.Name}, found {device.Services.Count} services"), (int)NotifyType.StatusMessage);
                    AddConnectionStatusChangedHandler();
                }
                else
                {
                    response.Message.Add("Device unreachable");
                    response.Valid = false;
                }
            }
            return response;
        }

        /// <inheritdoc/>
        public Task<bool> DisconnectAsync()
        {
            RemoveValueChangedHandler();
            this.BluetoothLEDevice?.Dispose();
            this.BluetoothLEDevice = null;
            this.ClearAllSensorCache();

            return Task.FromResult(true);
        }

        public void Dispose()
        {
            RemoveValueChangedHandler();
        }

        #region Private

        private void DeviceConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            var publisher = _publishers.FirstOrDefault(x => x.PublisherName == Name && x.PublisherType == PublisherType.PropertyChanged);
            if (publisher != null)
                publisher.Publish(Name);
        }

        private void ClearAllSensorCache()
        {
            try
            {
                foreach (var service in Services)
                {
                    ((IDisposable)service).Dispose();
                }

                this.Services.Clear();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void RemoveValueChangedHandler()
        {
            var publisher = _publishers.FirstOrDefault(x => x.PublisherName == Name && x.PublisherType == PublisherType.PropertyChanged);
            _subscriber.Unsubscribe(publisher);
            _publishers.Remove(publisher);
            this.BluetoothLEDevice.ConnectionStatusChanged -= this.DeviceConnectionStatusChanged;
        }

        private Publisher AddConnectionStatusChangedHandler()
        {
            var publisher = new Publisher(Name, PublisherType.PropertyChanged);
            _subscriber.Subscribe(publisher);
            _publishers.Add(publisher);
            this.BluetoothLEDevice.ConnectionStatusChanged += this.DeviceConnectionStatusChanged;
            return publisher;
        }

        #endregion Private
    }
}