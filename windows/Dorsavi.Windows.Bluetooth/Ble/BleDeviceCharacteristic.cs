using Dorsavi.Win.Bluetooth.Common;
using Dorsavi.Win.Bluetooth.Constants;
using Dorsavi.Win.Bluetooth.Models;
using Dorsavi.Win.Framework.Model;
using Dorsavi.Win.Framework.PubSub;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace Dorsavi.Win.Bluetooth.Ble
{
    public class BleDeviceCharacteristic : BasePublisher, IDisposable
    {
        public BleDeviceCharacteristic()
        {
        }

        public BleDeviceCharacteristic(GattCharacteristic gattCharacteristic, string deviceId, string serviceId) : this()
        {
            Characteristic = gattCharacteristic;
            DeviceId = deviceId;
            ServiceId = serviceId;
        }

        [JsonIgnore]
        public GattCharacteristic Characteristic;

        public Guid Uuid => Characteristic.Uuid;

        public virtual string Name => DisplayHelpers.GetCharacteristicName(Characteristic);

        public string DeviceId { get; set; }

        public string ServiceId { get; set; }

        public bool SubscribedForNotification => subscribedForNotification;

        private bool subscribedForNotification = false;

        private string PublisherName => $"{DeviceId}|{ServiceId}|{Uuid}|{Name}";

        public async Task<ServiceResponse<List<BleDeviceCharacteristicDescriptor>>> GetCharacteristicDescriptorsAsync()
        {
            var resp = new ServiceResponse<List<BleDeviceCharacteristicDescriptor>>();

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

                var characteristicDescriptors = new List<BleDeviceCharacteristicDescriptor>();
                // Enable/disable operations based on the GattCharacteristicProperties.
                foreach (var item in CharacteristicPropertiesEnum.List)
                {
                    var gattCharProp = (GattCharacteristicProperties)item.CharacteristicValue;
                    if (Characteristic.CharacteristicProperties.HasFlag(gattCharProp))
                    {
                        characteristicDescriptors.Add(new BleDeviceCharacteristicDescriptor()
                        {
                            CharacteristicUuid = Uuid.ToString(),
                            DeviceId = DeviceId,
                            ServiceId = ServiceId,
                            Value = item.Name,
                            SubscriptionState = false
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

        public async Task<ServiceResponse> ToogleSubscribe()
        {
            var resp = new ServiceResponse();
            try
            {
                if (!subscribedForNotification)
                {
                    // initialize status
                    GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
                    var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
                    if (Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
                    {
                        cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
                    }
                    else if (Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                    {
                        cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
                    }

                    try
                    {
                        // BT_Code: Must write the CCCD in order for server to send indications.
                        // We receive them in the ValueChanged event handler.
                        status = await Characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);

                        if (status == GattCommunicationStatus.Success)
                        {
                            AddValueChangedHandler();
                            resp.Valid = true;
                            resp.Message.Add("Successfully subscribed for value changes");
                        }
                        else
                        {
                            resp.Message.Add($"Error registering for value changes: {status}");
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        // This usually happens when a device reports that it support indicate, but it actually doesn't.
                        resp.Message.Add(ex.Message);
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
                                Characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                    GattClientCharacteristicConfigurationDescriptorValue.None);
                        if (result == GattCommunicationStatus.Success)
                        {
                            subscribedForNotification = false;
                            RemoveValueChangedHandler();
                            resp.Message.Add("Successfully un-registered for notifications");
                            resp.Valid = true;
                        }
                        else
                        {
                            resp.Message.Add($"Error un-registering for notifications: {result}");
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        // This usually happens when a device reports that it support notify, but it actually doesn't.
                        resp.Message.Add(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Message.Add($"Error change notifications: {ex.Message}");
            }
            return resp;
        }

        public byte[] GetByteArrayFromHexCode(string hexCode)
        {
            int hexLength = hexCode.Length;
            byte[] bytes = new byte[hexLength / 2];
            for (int i = 0; i < hexLength; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexCode.Substring(i, 2), 16);
            }

            return bytes;
        }

        public async Task<ServiceResponse<string>> CharacteristicWrite(string characteristicWriteValue, bool sendAsInt)
        {
            var resp = new ServiceResponse<string>();

            try
            {
                if (!String.IsNullOrEmpty(characteristicWriteValue))
                {
                    GattWriteResult result = null;
                    if (sendAsInt)
                    {
                        var isValidValue = Int32.TryParse(characteristicWriteValue, out int readValue);
                        if (isValidValue)
                        {
                            var writer = new DataWriter();
                            writer.ByteOrder = ByteOrder.LittleEndian;
                            writer.WriteInt32(readValue);
                            writer.WriteUInt32(5);

                            result = await Characteristic.WriteValueWithResultAsync(writer.DetachBuffer());
                        }
                        else
                        {
                            resp.Message.Add("Data to write has to be an int32");
                            resp.Valid = false;
                        }
                    }
                    else
                    {
                        // BT_Code: Writes the value from the buffer to the characteristic.
                        result = await Characteristic.WriteValueWithResultAsync(CryptographicBuffer.DecodeFromBase64String(characteristicWriteValue));
                    }

                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        resp.Message.Add("Successfully wrote value to device");
                        resp.Valid = true;
                    }
                    else
                    {
                        resp.Message.Add($"Write failed: {result.Status}");
                        resp.Valid = false;
                    }
                }
                else
                {
                    resp.Message.Add($"No data to write to device");
                    resp.Valid = false;
                }
            }
            catch (Exception ex) when (ex.HResult == ErrorCodes.E_BLUETOOTH_ATT_INVALID_PDU)
            {
                resp.Message.Add(ex.Message);
                resp.Valid = false;
            }
            catch (Exception ex) when (ex.HResult == ErrorCodes.E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED || ex.HResult == ErrorCodes.E_ACCESSDENIED)
            {
                // This usually happens when a device reports that it support writing, but it actually doesn't.
                resp.Message.Add(ex.Message);
                resp.Valid = false;
            }
            return resp;
        }

        #region Private

        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            try
            {
                // BT_Code: An Indicate or Notify reported that the value has changed.
                // Display the new value with a timestamp.

                byte[] data;
                CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out data);
                Publish(PublisherName, Convert.ToBase64String(data));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void AddValueChangedHandler()
        {
            if (!subscribedForNotification)
            {
                ModifyPublisher($"{PublisherName}", PublisherType.SubscriptionValue);

                Characteristic.ValueChanged += Characteristic_ValueChanged;
                subscribedForNotification = true;
            }
        }

        private void RemoveValueChangedHandler()
        {
            if (subscribedForNotification)
            {
                ModifyPublisher(PublisherName, PublisherType.SubscriptionValue, true);

                Characteristic.ValueChanged -= Characteristic_ValueChanged;
                subscribedForNotification = false;
            }
        }

        public async Task<ServiceResponse<string>> CharacteristicRead()
        {
            var resp = new ServiceResponse<string>();
            // BT_Code: Read the actual value from the device by using Uncached.
            GattReadResult result = await Characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);

            if (result.Status == GattCommunicationStatus.Success)
            {
                byte[] data;
                CryptographicBuffer.CopyToByteArray(result.Value, out data);

                resp.Content = Convert.ToBase64String(data);
                //resp.Content = BitConverter.ToString(data).Replace("-", string.Empty);
                resp.Valid = true;
            }
            else
            {
                resp.Valid = false;
                resp.Message.Add($"Read failed: {result.Status}");
            }
            return resp;
        }

        public void Dispose()
        {
            Characteristic = null;
        }

        #endregion Private
    }
}