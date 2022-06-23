using Newtonsoft.Json;
using rnwindowsminimal.Bluetooth.Helpers;
using rnwindowsminimal.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

public class BleDevice : INotifyPropertyChanged
{
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
            Services.Add(new BleDeviceServices(service));
        }
    }

    public DeviceInformation DeviceInformation { get; private set; }

    [JsonIgnore]
    public BluetoothLEDevice BluetoothLEDevice { get; private set; }

    public List<BleDeviceServices> Services { get; set; }

    public string Id => DeviceInformation.Id;
    public string Name => DeviceInformation.Name;
    public bool IsPaired => DeviceInformation.Pairing.IsPaired;

    public bool? IsConnected
    {
        get
        {
            try
            {
                return (bool?)DeviceInformation.Properties["System.Devices.Aep.IsConnected"] == true;
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
                return (bool?)DeviceInformation.Properties["System.Devices.Aep.Bluetooth.Le.IsConnectable"] == true;
            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }

    public IReadOnlyDictionary<string, object> Properties => DeviceInformation.Properties;

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
    [JsonIgnore]
    public GattDeviceService Service;

    [JsonIgnore]
    public IReadOnlyList<GattCharacteristic> BleCharacteristics;

    public BleDeviceServices(GattDeviceService gattDeviceService)
    {
        this.Service = gattDeviceService;
        Name = DisplayHelpers.GetServiceName(Service);


        var charList = GetCharacteristic().Result;

    }

    public string DeviceId => Service.DeviceId;
    public string UUid => Service.Uuid.ToString();
    public string Name { get; private set; }

    public async Task<ServiceResponse<List<BleDeviceCharacteristic>>> GetCharacteristic()
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
                        BleCharacteristics = result.Characteristics.ToList();
                    }
                }

                var characteristics = new List<BleDeviceCharacteristic>();
                foreach (var item in BleCharacteristics)
                {
                    characteristics.Add(new BleDeviceCharacteristic()
                    {
                        Characteristic = item,
                        Name = DisplayHelpers.GetCharacteristicName(item),
                        Uuid = item.Uuid.ToString()
                    });
                }
                resp.Content = characteristics;
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
}