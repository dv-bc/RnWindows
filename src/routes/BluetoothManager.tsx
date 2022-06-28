import React, { Component, useState, useEffect, useRef } from 'react';
import {
  AppRegistry,
  Alert,
  Text,
  View,
  Button,
  FlatList,
  TouchableOpacity,
  StyleSheet,
  TextInput,
  Switch
} from 'react-native';

import { NativeModules, NativeEventEmitter } from 'react-native';

import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';

const WinBluetoothEventEmitter = new NativeEventEmitter(NativeModules.RnBluetooth);
let KnownDevice = [
  // {
  //   id: 'bd7acbea-c1b1-46c2-aed5-3ad53abb28ba',
  //   title: 'First Item',
  // },
  // {
  //   id: '3ac68afc-c605-48d3-a4f8-fbd91aa97f63',
  //   title: 'Second Item',
  // },
  // {
  //   id: '58694a0f-3da1-471f-bd96-145571e29d72',
  //   title: 'Third Item',
  // },
];



interface BleDevice {
  Id: string;
  Name: string;
  IsPaired: boolean;
  IsConnected: boolean;
  IsConnectable: boolean;
  Services: BleServices[]
}


interface BleServices {
  DeviceId: string;
  UUId: string;
  Name: string;
  Characteristic: BleServiceCharactheristic[]
}

interface BleServiceCharactheristic {
  Uuid: string;
  Name: string;
}

interface CharacteristicDescriptor {
  CharacteristicUuid: string;
  DeviceId: string;
  ServiceId: string;
  Value: string;
  SubscriptionState: Boolean;
}





export default function BleManager() {

  const [isScanningState, setScanning] = useState(false);
  const [IsConnectingState, setConnecting] = useState(false);
  const [KnownDevices, setKnownDevices] = useState([]);
  const [ConnectedDevice, setConnectedDevice] = useState([]);
  const [DeviceService, setDeviceService] = useState([]);
  const [DeviceCharacteristic, setDeviceCharacteristic] = useState([]);
  const [CharacteristicDescriptor, setCharacteristicDescriptor] = useState<CharacteristicDescriptor[]>([]);
  const [writeValue, setwriteValue] = useState(String);
  const [sendAsInt, setsendAsInt] = useState(Boolean);
  const [ReadOutput, setReadOutput] = useState(String);





  console.log("current knownDevice :" + KnownDevices);

  useEffect(() => {
    // componentDidMount in functional component.
    WinBluetoothEventEmitter.addListener('Event', BleEvent);
    WinBluetoothEventEmitter.addListener('UserNotification', UserNotification);
    WinBluetoothEventEmitter.addListener('KnownDeviceUpdated', KnownDevicesUpdated);
    WinBluetoothEventEmitter.addListener('ConnectedDevicesUpdated', ConnectedDevicesUpdated);
    WinBluetoothEventEmitter.addListener('OnDeviceDisconnect', DeviceDisconnect);

    WinBluetoothEventEmitter.addListener('DeviceEnumerationCompleted', DeviceWatcherEnumerationCompleted);
    WinBluetoothEventEmitter.addListener('IsScanningEvent', SetScanning);
    WinBluetoothEventEmitter.addListener('IsConnecting', SetConnecting);
    WinBluetoothEventEmitter.addListener('SubscriptionEvent', SubscriptionEvent);
    return () => {

      // componentwillunmount in functional component.
      // Anything in here is fired on component unmount.
      WinBluetoothEventEmitter.addListener('Event', BleEvent).remove();
      WinBluetoothEventEmitter.addListener('UserNotification', UserNotification).remove();
      WinBluetoothEventEmitter.addListener('KnownDeviceUpdated', KnownDevicesUpdated).remove();
      WinBluetoothEventEmitter.addListener('ConnectedDevicesUpdated', ConnectedDevicesUpdated).remove();
      WinBluetoothEventEmitter.addListener('IsScanningEvent', SetScanning).remove();
      WinBluetoothEventEmitter.addListener('IsConnecting', SetConnecting).remove();
      WinBluetoothEventEmitter.addListener('DeviceEnumerationCompleted', DeviceWatcherEnumerationCompleted).remove();
      WinBluetoothEventEmitter.addListener('SubscriptionEvent', SubscriptionEvent).remove();
    }
  }, []);

  //#region EVENT FUNCTIONS 

  function SetScanning(isScanning: boolean) {
    setScanning(isScanning)
    console.log("scanning set to " + isScanning)
  }
  function SetConnecting(isConnecting: boolean) {
    setConnecting(isConnecting)
  }

  function PairDevice(Id: string) {
    const tempChar: BleServices[] = [];
    NativeModules.RnBluetooth.Connect(Id)
      .then((data: string) => {
        var resp = JSON.parse(data);
        if (resp.Valid && resp.Content != null) {
          alert("Successfully Connected");
          setDeviceService(resp.Content.Services);
        }
        else if (resp.Valid) {
          alert(resp.Message.join());
        }
        else {
          alert("Fail to connect");
        }
      });

  }

  function SetCurrentDeviceService(Id: string) {
    let currentConnectedDevice = ConnectedDevice.find((device) => {
      return device.Id === Id;
    });
    if (currentConnectedDevice) {
      setDeviceService(currentConnectedDevice.Services);
    }
    else {
      alert("device not found");

    }

  }



  function KnownDevicesUpdated(knownDevicesData: string) {
    var devices = JSON.parse(knownDevicesData);
    if (devices != null) {
      const knownDevices: BleDevice[] = [];
      devices.forEach(item => {
        knownDevices.push(item)
      });
      setKnownDevices(knownDevices);
    }
  }

  function DeviceDisconnect() {


  }

  function ConnectedDevicesUpdated(connectedDevicesList: string) {
    var devices = JSON.parse(connectedDevicesList);
    if (devices != null) {
      const connectedDevices: BleDevice[] = [];
      devices.forEach(item => {
        connectedDevices.push(item)
      });
      setConnectedDevice(connectedDevices);
    }
  }


  function DeviceWatcherEnumerationCompleted(message: string) {
    alert(message);
  }
  function BleEvent(message: string) {
    console.log("Event was fired with: " + message);
  }
  function UserNotification(message: string) {
    console.log("Notification to user: " + message);
  }
  function SubscriptionEvent(message: string) {
    console.log(message);
  }

  //#endregion

  function ScanClicked() {
    if (isScanningState != true) {
      console.log("know device cleared")
    }
    NativeModules.RnBluetooth.StartScan();
  }

  function GetServiceCharacteristic(deviceId: string, serviceId: string) {
    NativeModules.RnBluetooth.GetBleCharacteristic(deviceId, serviceId)
      .then((data: string) => {

        var resp = JSON.parse(data);
        if (resp.Valid && resp.Content != null) {
          setDeviceCharacteristic(resp.Content)
        }
        else if (resp.Valid) {
          alert(resp.Message.join());
        }
        else {
          alert("Fail to get Characteristc");
        }
      });;

  }

  function GetCharacteristicDescriptor(deviceId: string, serviceId: string, characteristicId: string) {
    NativeModules.RnBluetooth.GetCharacteristicDescriptor(deviceId, serviceId, characteristicId)
      .then((data: string) => {

        var resp = JSON.parse(data);
        if (resp.Valid && resp.Content != null) {
          setCharacteristicDescriptor(resp.Content)
        }
        else if (resp.Valid) {
          alert(resp.Message.join());
        }
        else {
          alert("Fail to get Characteristc descriptor");
        }
      });;

  }

  function ReadCharacteristic(deviceId: string, serviceId: string, characteristicId: string, descriptor: string) {
    NativeModules.RnBluetooth.SendCommand(deviceId, serviceId, characteristicId, descriptor)
      .then((data: string) => {

        var resp = JSON.parse(data);
        if (resp.Valid && resp.Content != null) {
          setReadOutput(resp.Content);
        }
        else if (resp.Valid) {
          alert(resp.Message.join());
        }
        else {
          alert("Fail to get read Characteristc ");
        }
      });
  }

  function WriteCharacteristic(deviceId: string, serviceId: string, characteristicId: string, descriptor: string) {
    NativeModules.RnBluetooth.SendWriteCommand(deviceId, serviceId, characteristicId, descriptor, writeValue, sendAsInt)
      .then((data: string) => {

        var resp = JSON.parse(data);
        if (resp.Valid && resp.Content != null) {
          alert(resp.Content);
        }
        else if (resp.Valid) {
          alert(resp.Message.join());
        }
        else {
          alert("Fail to get read Characteristc ");
        }
      });
  }

  function ToogleSubscribeCharacteristic(deviceId: string, serviceId: string, characteristicId: string, descriptor: string) {
    NativeModules.RnBluetooth.SendCommand(deviceId, serviceId, characteristicId, descriptor)
      .then((data: string) => {

        var resp = JSON.parse(data);
        if (resp.Valid && resp.Content != null) {
          setCharacteristicDescriptor(resp.Content)
        }
        else if (resp.Valid) {
          alert(resp.Message.join());
        }
        else {
          alert("Fail to subscribe Characteristc ");
        }
      });
  }



  //
  const Item = ({ item, onPress, backgroundColor, textColor, fontSize }) => (
    <TouchableOpacity onPress={onPress} style={{
      ...styles.item,
      backgroundColor: backgroundColor
    }}>
      <Text style={{
        ...styles.title,
        color: textColor,
        fontSize: fontSize
      }}>{item.Name}</Text>
    </TouchableOpacity>
  );

  const renderDeviceItem = ({ item, index }) => {

    const backgroundColor = item.IsConnected === true ? "#6e3b6e" : "#ffffff";
    const color = item.IsConnected === true ? 'white' : 'black';

    return (
      <View>
        <Item
          key={index}
          item={item}
          onPress={() => PairDevice(item.Id)}
          backgroundColor={{ backgroundColor }}
          textColor={{ color }}
          fontSize={15}
        />
        <Text>{item.IsConnected}</Text>
      </View>
    );
  };

  const renderConnectedDevice = (data) => {
    console.log("Connected device", data)
    const { item, index } = data;

    return (
      <Item
        key={index}
        onPress={() => SetCurrentDeviceService(item.Id)}
        item={item}
        backgroundColor={'#ffffff'}
        textColor={'black'}
        fontSize={12}
      />
    );
  };

  const renderDeviceService = (data) => {
    console.log("Services", data)
    const { item, index } = data;

    return (
      <Item
        key={index}
        onPress={() => GetServiceCharacteristic(item.DeviceId, item.UUid)}
        item={item}
        backgroundColor={"#ffffff"}
        textColor={'black'}
        fontSize={12}
      />
    );
  };

  const renderDeviceCharacteristic = (data) => {
    console.log("Characteristic", data)
    const { item, index } = data;

    return (
      <Item
        key={index}
        onPress={() => GetCharacteristicDescriptor(item.DeviceId, item.ServiceId, item.Uuid)}
        item={item}
        backgroundColor={"#ffffff"}
        textColor={'black'}
        fontSize={12}
      />
    );
  };



  const toggleSwitch = () => setsendAsInt(previousState => !previousState);

  const RenderCharacteristicDescriptor = ({
    data
  }: {
    data: CharacteristicDescriptor[]
  }) => {

    const renderButtonList = () => {
      return data.map((s: CharacteristicDescriptor, index: number) => {

        
        switch (s.Value) {
          case 'Read':
            return (
              <View>
                <Text style={{
                  ...styles.title,
                  color: 'black',
                  fontSize: 12
                }}>{ReadOutput}</Text>
                <TouchableOpacity key={index} onPress={() => ReadCharacteristic(s.DeviceId, s.ServiceId, s.CharacteristicUuid, s.Value)}
                  style={{
                    ...styles.item,
                    backgroundColor: '#ffffff'
                  }}>
                  <Text style={{
                    ...styles.title,
                    color: 'black',
                    fontSize: 12
                  }}>Read</Text>
                </TouchableOpacity>
              </View>
            );
          case 'Write':
            return (

              <View>
                <TextInput
                  key="textinput2"
                  style={{
                    width: "100%",
                    padding: 10,
                    borderWidth: 1,
                    marginTop: 20,
                    marginBottom: 20
                  }}
                  onChangeText={text => {
                    setwriteValue(text);
                  }}
                  value={writeValue}
                />

                <Switch
                  trackColor={{ false: "#767577", true: "#81b0ff" }}
                  thumbColor={sendAsInt ? "#f5dd4b" : "#f4f3f4"}
                  ios_backgroundColor="#3e3e3e"
                  onValueChange={toggleSwitch}
                  value={sendAsInt}
                />

                <Text style={{
                  ...styles.title,
                  color: 'black',
                  fontSize: 12
                }}>Send as Int</Text>

                <TouchableOpacity key={index} onPress={() => WriteCharacteristic(s.DeviceId, s.ServiceId, s.CharacteristicUuid, s.Value)}
                  style={{
                    ...styles.item,
                    backgroundColor: '#ffffff'
                  }}>
                  <Text style={{
                    ...styles.title,
                    color: 'black',
                    fontSize: 12
                  }}>Write</Text>
                </TouchableOpacity>
              </View>
            );
          case 'Notify':
            const subscriptionText = s.SubscriptionState === true ? "Unsubscribe" : "Subscribe";
            return (<TouchableOpacity key={index}  onPress={() => ToogleSubscribeCharacteristic(s.DeviceId, s.ServiceId, s.CharacteristicUuid, s.Value)}
              style={{ ...styles.item,
              backgroundColor: '#ffffff'
            }}>
              <Text key={index} style={{
                ...styles.title,
                color: 'black',
                fontSize: 12
              }}>{ subscriptionText }</Text>
            </TouchableOpacity>);
          default:
            return <View key={index} />
        }

      });
    }


    return (

      <View>
        {renderButtonList()}
      </View>
    );
  };



  return (
    <View style={{ padding: 10, height: 600 }}>
      <Text style={{
        fontWeight: 'bold', textAlign: "center",
        marginBottom: 10,
        fontSize: 24
      }}>BLE DEMO</Text>
      <Text>{NativeModules.RnBluetooth.Connected}</Text>
      <View style={{
        flexWrap: "wrap",
        flexDirection: "row",
        marginVertical: 10
      }}>
        <View style={{ flex: 1 }} >
          <Button onPress={ScanClicked} title={isScanningState ? "Scanning..." : "Scan for sensor"} />

        </View>
        <View style={{ flex: 3 }} >
        </View>
      </View>
      <View style={{
        flexWrap: "wrap",
        flexDirection: "row",
        marginVertical: 10,

      }}>
        <View style={{
          flex: 1,
          marginRight: 20
        }} >
          <Text style={{ fontWeight: 'bold' }}>sensor List :</Text>
          <Text style={{}}>Available scanned device from list below</Text>
          <View style={{ borderColor: 'black', borderStyle: 'solid', borderWidth: 2, height: 400 }} >
            <FlatList
              data={KnownDevices}
              renderItem={renderDeviceItem}
              keyExtractor={(item) => item.Id}
              extraData={KnownDevices}
            />
          </View>

        </View>
        <View style={{
          flex: 1,
          marginRight: 20
        }} >
          <Text style={{ fontWeight: 'bold' }}>device service :</Text>
          <Text style={{}}>select connected device from list to see their service and Charasteristic</Text>
          <View style={{ borderColor: 'black', borderStyle: 'solid', borderWidth: 2, height: 400 }} >
            <FlatList
              data={ConnectedDevice}
              renderItem={renderConnectedDevice}
              keyExtractor={(item) => item.Id}
              extraData={ConnectedDevice}
            />
          </View>
        </View>
        <View style={{
          flex: 2,
          marginRight: 20
        }} >
          <Text style={{ fontWeight: 'bold' }}>Connected List :</Text>
          <Text style={{}}>select available service</Text>
          <View style={{ borderColor: 'black', borderStyle: 'solid', borderWidth: 2, height: 400 }} >
            <FlatList
              data={DeviceService}
              renderItem={renderDeviceService}
              keyExtractor={(item) => item.Id}
              extraData={DeviceService}
            />
          </View>
        </View>
        <View style={{
          flex: 1,
          marginRight: 20
        }} >
          <Text style={{ fontWeight: 'bold' }}>Charactheristic List :</Text>
          <Text style={{}}>select Charactheristic service from list below</Text>
          <View style={{ borderColor: 'black', borderStyle: 'solid', borderWidth: 2, height: 400 }} >
            <FlatList
              data={DeviceCharacteristic}
              renderItem={renderDeviceCharacteristic}
              keyExtractor={(item) => item.Id}
              extraData={DeviceCharacteristic}
            />
          </View>
        </View>
        <View style={{
          flex: 1,
          marginRight: 20
        }} >
          <Text style={{ fontWeight: 'bold' }}>characteristic Descriptor :</Text>
          <Text style={{}}>select characteristic descriptor from list below</Text>
          <View style={{ borderColor: 'black', borderStyle: 'solid', borderWidth: 2, height: 400 }} >
            <RenderCharacteristicDescriptor data={CharacteristicDescriptor}></RenderCharacteristicDescriptor>
          </View>
        </View>
      </View>




    </View>
    // <View style={{ padding: 10, flex: 1 }}>
    //   
    //  

    //   <View
    //     style={{
    //       flexDirection: "row",
    //       height: 500
    //     }}
    //   >
    //     <View style={{ flex: 0.3 }} >
    //       <Button onPress={ScanClicked} title={isScanningState ? "Scanning..." : "Scan for sensor"} />
    //       
    //     </View>
    //     <View style={{ flex: 0.3 }} >
    //       <Text style={{ fontWeight: 'bold' }}>service List :</Text>
    //       <FlatList
    //         data={knownCharactheristic}
    //         renderItem={renderCharacteristicItem}
    //         keyExtractor={(item) => item.Id}
    //         extraData={knownCharactheristic}
    //       />
    //     </View>
    //   </View>
    // </View>
  );
}





const styles = StyleSheet.create({
  container: {
    flex: 1,
    paddingVertical: 10,
  },
  item: {
    padding: 10,
    marginVertical: 8,
    marginHorizontal: 16,
    justifyContent: 'center'
  },
  title: {
    fontSize: 32,
  },
});
