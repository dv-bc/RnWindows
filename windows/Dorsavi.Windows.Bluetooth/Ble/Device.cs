using Dorsavi.Windows.Bluetooth.Common;
using Dorsavi.Windows.Bluetooth.Constants;
using Dorsavi.Windows.Bluetooth.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace Dorsavi.Windows.Bluetooth.Ble
{
    public class BleDevice : INotifyPropertyChanged
    {
        public BleDevice()
        {
        }

        public BleDevice(DeviceInformation deviceInfoIn)
        {
            DeviceInformation = deviceInfoIn;
        }

        public BleDevice(DeviceInformation deviceInfoIn, BluetoothLEDevice bluetoothLEDevice, GattDeviceServicesResult deviceServices) : this(deviceInfoIn)
        {
            BluetoothLEDevice = bluetoothLEDevice;
            if (Services == null)
            {
                Services = new List<BleDeviceServices>();
            }
            foreach (var service in deviceServices.Services)
            {
                Services.Add(new BleDeviceServices(service, Id));
            }
        }

        public DeviceInformation DeviceInformation { get; private set; }

        [JsonIgnore]
        public BluetoothLEDevice BluetoothLEDevice { get; private set; }

        public List<BleDeviceServices> Services { get; set; }

        public string Id => DeviceInformation?.Id ?? "";
        public string Name => DeviceInformation?.Name ?? "";
        public bool? IsPaired => DeviceInformation?.Pairing.IsPaired;

        public bool? IsConnected
        {
            get
            {
                try
                {
                    return (bool?)DeviceInformation?.Properties["System.Devices.Aep.IsConnected"] == true;
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void Update(DeviceInformationUpdate deviceInfoUpdate)
        {
            DeviceInformation.Update(deviceInfoUpdate);

            OnPropertyChanged("Id");
            OnPropertyChanged("Name");
            OnPropertyChanged("DeviceInformation");
            OnPropertyChanged("IsPaired");
            OnPropertyChanged("IsConnected");
            OnPropertyChanged("Properties");
            OnPropertyChanged("IsConnectable");

            //        UpdateGlyphBitmapImage();
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class BleDeviceServices
    {
        public BleDeviceServices()
        {
        }

        [JsonIgnore]
        public GattDeviceService Service;

        [JsonIgnore]
        public IReadOnlyList<GattCharacteristic> BleCharacteristics;

        public BleDeviceServices(GattDeviceService gattDeviceService, string deviceId)
        {
            this.Service = gattDeviceService;
            Name = DisplayHelpers.GetServiceName(Service);
            DeviceId = deviceId;
        }

        public string DeviceId { get; private set; }
        public string UUid => Service.Uuid.ToString();
        public string Name { get; private set; }

        public async Task<ServiceResponse<List<BleDeviceCharacteristic>>> GetCharacteristicAsync()
        {
            var resp = new ServiceResponse<List<BleDeviceCharacteristic>>();
            try
            {
                var accessStatus = await Service.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    if (BleCharacteristics == null)
                    {
                        var result = await Service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            BleCharacteristics = result.Characteristics;
                        }
                    }
                    resp.Valid = true;
                    resp.Content = BleCharacteristics.Select(item => new BleDeviceCharacteristic()
                    {
                        Characteristic = item,
                        Name = DisplayHelpers.GetCharacteristicName(item),
                        Uuid = item.Uuid.ToString(),
                        DeviceId = DeviceId,
                        ServiceId = UUid
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Message.Add($"Error, {ex.Message}");
                // On error, act as if there are no characteristics.
                resp.Content = new List<BleDeviceCharacteristic>();
            }
            return resp;
        }
    }

    public class BleDeviceCharacteristic
    {
        [JsonIgnore]
        public GattCharacteristic Characteristic;
        public string Uuid { get; set; }

        public string Name { get; set; }

        public string DeviceId { get; set; }

        public string ServiceId { get; set; }

        public async Task<ServiceResponse<List<KeyValuePair<string, uint>>>> GetCharacteristicDescriptorsAsync()
        {
            var resp = new ServiceResponse<List<KeyValuePair<string, uint>>>();

            try
            {
                var result = await Characteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
                if (result.Status != GattCommunicationStatus.Success)
                {
                    resp.Message.Add("Descriptor read failure: " + result.Status.ToString());
                    return resp;
                }
                GattPresentationFormat presentationFormat;
                // BT_Code: There's no need to access presentation format unless there's at least one. 
                presentationFormat = null;
                if (Characteristic.PresentationFormats.Count > 0)
                {

                    if (Characteristic.PresentationFormats.Count.Equals(1))
                    {
                        // Get the presentation format since there's only one way of presenting it
                        presentationFormat = Characteristic.PresentationFormats[0];
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
                    if (Characteristic.CharacteristicProperties.HasFlag(gattCharProp))
                    {
                        characteristicDescriptors.Add(new KeyValuePair<string, uint>(item.Name, item.CharacteristicValue));
                    }
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
    }
}
