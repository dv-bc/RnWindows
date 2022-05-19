//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace rnwindowsminimal.Bluetooth
//{
//    /// <summary>
//    /// Class for accessing windows bluetooth device characteristics.
//    /// </summary>
//    public class BleCharacteristic : IBleCharacteristic
//    {
//        private readonly string streamingDataAccessCodeString = "00000010-0008-a8ba-e311-f48c90364d99";
//        private readonly IGattCharacteristic characteristic;

//        private List<IBleDescriptor> cache = new List<IBleDescriptor>();

//        /// <summary>
//        /// Initializes a new instance of the <see cref="BleCharacteristic"/> class.
//        /// </summary>
//        /// <param name="characteristic">The Gatt Characteristics.</param>
//        public BleCharacteristic(IGattCharacteristic characteristic)
//        {
//            this.characteristic = characteristic;

//            // Make sure to only set the Streaming Characteristic to Notify
//            if (this.CanUpdate && this.IsStreaming())
//            {
//                this.characteristic.ValueChanged += this.Characteristic_ValueChanged;
//                this.SetupNotification();
//            }
//        }

//        private async void SetupNotification()
//        {
//            await this.characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
//                       GattClientCharacteristicConfigurationDescriptorValue.None);
//        }

//        private bool IsStreaming() => this.Id == Guid.Parse(this.streamingDataAccessCodeString);

//        /// <inheritdoc/>
//        public event EventHandler<byte[]> ValueUpdated;

//        /// <inheritdoc/>
//        public event EventHandler<byte[]> ValueChanged;

//        /// <inheritdoc/>
//        public Guid Id => this.characteristic.Uuid;

//        /// <inheritdoc/>
//        public bool CanUpdate => this.characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify);

//        /// <inheritdoc/>
//        public bool CanWrite => this.characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write);

//        /// <inheritdoc/>
//        public bool CanRead => this.characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read);

//        /// <inheritdoc/>
//        public byte[] Value { get; set; } = new byte[0];

//        /// <summary>
//        /// UUID Of the Characteristic, which will be consumed by the clients
//        /// </summary>
//        public string uuid => this.characteristic.Uuid.ToString();

//        /// <inheritdoc/>
//        public async Task<IBleDescriptor> GetDescriptorAsync(Guid id)
//        {
//            // Check the cache
//            var descriptor = this.cache.FirstOrDefault(_ => _.Id == id);
//            if (descriptor == null)
//            {
//                // Get the descriptor from the characteristic
//                var result = await this.characteristic.GetDescriptorsForUuidAsync(id);
//                if (result.Status == GattCommunicationStatus.Success)
//                {
//                    var gattDescriptor = result.Descriptors.FirstOrDefault(_ => _.Uuid == id);
//                    descriptor = new BleDescriptor(gattDescriptor);
//                    this.cache.Add(descriptor);
//                }
//            }

//            return descriptor;
//        }

//        /// <inheritdoc/>
//        public async Task<IReadOnlyList<IBleDescriptor>> GetDescriptorsAsync()
//        {
//            var results = new List<IBleDescriptor>();

//            // Retrieve all descriptors for this characteristic from the device
//            var result = await this.characteristic.GetDescriptorsAsync();
//            if (result.Status == GattCommunicationStatus.Success)
//            {
//                foreach (var item in result.Descriptors)
//                {
//                    // Check if descriptor already present
//                    var descriptor = this.cache.FirstOrDefault(_ => _.Id == item.Uuid);
//                    if (descriptor == null)
//                    {
//                        // Descriptor not currently present
//                        results.Add(new BleDescriptor(item));
//                    }
//                    else
//                    {
//                        // Descriptor already present
//                        results.Add(descriptor);
//                    }
//                }
//            }

//            // Assign the results
//            this.cache = results;
//            return this.cache;
//        }

//        public async Task<byte[]> ReadAsync()
//        {
//            var result = await this.characteristic.ReadValueAsync();
//            if (result.Status == GattCommunicationStatus.Success)
//            {
//                this.Value = result.Value.ToArray();
//                this.ValueChanged?.Invoke(this, this.Value);
//            }

//            return this.Value;
//        }

//        /// <inheritdoc/>
//        public async Task<bool> StartUpdateAsync()
//        {
//            return (await this.characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
//                         GattClientCharacteristicConfigurationDescriptorValue.Notify)) == GattCommunicationStatus.Success;
//        }

//        /// <inheritdoc/>
//        public async Task<bool> StopUpdateAsync()
//        {
//            return (await this.characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
//                         GattClientCharacteristicConfigurationDescriptorValue.None)) == GattCommunicationStatus.Success;
//        }

//        /// <inheritdoc/>
//        public async Task<bool> WriteAsync(byte[] data)
//        {
//            try
//            {
//                var result = await this.characteristic.WriteValueWithoutResultAsync(CryptographicBuffer.CreateFromByteArray(data));
//                if (result == GattCommunicationStatus.Success)
//                {
//                    this.ValueChanged?.Invoke(this, this.Value);
//                }

//                return result == GattCommunicationStatus.Success;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        private void Characteristic_ValueChanged(IGattCharacteristic sender, IGattValueChangedEventArgs e)
//        {
//            this.Value = e.CharacteristicValue.ToArray();
//            this.ValueUpdated?.Invoke(this, this.Value);
//        }

//        public bool isValueUpdatedSubscribed()
//        {
//            return this.ValueUpdated != null;
//        }

//        public async Task<bool> WriteLargePayloadAsync(byte[] data)
//        {
//            try
//            {
//                int offset = 0;
//                int attributeDataLen = 20;
//                while (offset < data.Length)
//                {
//                    int length = data.Length - offset;
//                    if (length > attributeDataLen)
//                    {
//                        length = attributeDataLen;
//                    }

//                    byte[] subset = new byte[length];

//                    Array.Copy(data, offset, subset, 0, length);
//                    offset += length;

//                    var result = await this.characteristic.WriteValueWithoutResultAsync(CryptographicBuffer.CreateFromByteArray(data));
//                    if (result != GattCommunicationStatus.Success)
//                    {
//                        throw new ArgumentException("Failed to write large payload to sensor");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                ex.LogException();
//                return false;
//            }

//            return true;
//        }

//        public async Task<bool> WriteAsyncWithoutResponse(byte[] data)
//        {
//            await this.characteristic.WriteValueWithoutResultResponseAsync(CryptographicBuffer.CreateFromByteArray(data));

//            return true;
//        }
//    }
//}
