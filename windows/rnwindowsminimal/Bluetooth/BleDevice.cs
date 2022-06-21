using Microsoft.ReactNative.Managed;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Devices.Enumeration;

public class BleDevice : INotifyPropertyChanged
{
    public BleDevice(DeviceInformation deviceInfoIn)
    {
        DeviceInformation = deviceInfoIn;
        //UpdateGlyphBitmapImage();
    }

    public DeviceInformation DeviceInformation { get; private set; }

    public string Id => DeviceInformation.Id;
    public string Name => DeviceInformation.Name;
    public bool IsPaired => DeviceInformation.Pairing.IsPaired;
    public bool? IsConnected  { get {
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
    { get {
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

    //private async void UpdateGlyphBitmapImage()
    //{
    //    DeviceThumbnail deviceThumbnail = await DeviceInformation.GetGlyphThumbnailAsync();
    //    var glyphBitmapImage = new BitmapImage();
    //    await glyphBitmapImage.SetSourceAsync(deviceThumbnail);
    //    GlyphBitmapImage = glyphBitmapImage;
    //    OnPropertyChanged("GlyphBitmapImage");
    //}

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}