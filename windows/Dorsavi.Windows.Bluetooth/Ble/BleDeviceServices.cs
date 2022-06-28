using Dorsavi.Windows.Bluetooth.Common;
using Dorsavi.Windows.Bluetooth.Models;
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
    public class BleDeviceServices : IDisposable
    {
        public BleDeviceServices()
        {
            BleCharacteristics = new List<BleDeviceCharacteristic>();
        }

        [JsonIgnore]
        public GattDeviceService Service;

        [JsonIgnore]
        public List<BleDeviceCharacteristic> BleCharacteristics;

        public BleDeviceServices(GattDeviceService gattDeviceService, string deviceId) : this()
        {
            this.Service = gattDeviceService;
            DeviceId = deviceId;
        }

        public string DeviceId { get; private set; }
        public string UUid => Service.Uuid.ToString();
        public virtual string Name => DisplayHelpers.GetServiceName(Service);

        public async Task<ServiceResponse<List<BleDeviceCharacteristic>>> GetCharacteristicAsync()
        {
            var resp = new ServiceResponse<List<BleDeviceCharacteristic>>();
            try
            {
                var accessStatus = await Service.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    if (BleCharacteristics == null || !BleCharacteristics.Any())
                    {
                        var result = await Service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            foreach (var item in result.Characteristics)
                            {
                                if (UUid == V6CServiceUuId.Custom.UUid)
                                {
                                    BleCharacteristics.Add(new V6cDeviceCharacteristic(item, DeviceId, UUid));
                                }
                                else
                                    BleCharacteristics.Add(new BleDeviceCharacteristic(item, DeviceId, UUid));
                            }

                        }
                    }
                    resp.Valid = true;
                    resp.Content = BleCharacteristics;
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




        public void Dispose()
        {
            Service.Dispose();
        }
    }
}
