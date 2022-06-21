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
  StatusBar
} from 'react-native';

import { NativeModules, NativeEventEmitter } from 'react-native';

import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';

const WinBluetoothEventEmitter = new NativeEventEmitter(NativeModules.BleManager);
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
}

interface Charactheristic {
  Id: string;
  Name: string;
}




export default function BleManager({ navigation }) {
  const initialDevice: BleDevice[] = [{ Id: "first", Name: "first device" }]
  const initialChar: Charactheristic[] = [{ Id: "first", Name: "first service" }]

  const [isScanningState, setScanning] = useState(false);
  const [IsConnectingState, setConnecting] = useState(false);
  const [knownDevices, setKnownDevices] = useState([]);
  const [knownCharactheristic, setCharactheristic] = useState(initialChar);


  const tempKnownDevices = useRef<BleDevice[]>();
  const tempKnownChar = useRef<Charactheristic[]>();


  console.log("current knownDevice :" + knownDevices);

  useEffect(() => {
    // componentDidMount in functional component.
    WinBluetoothEventEmitter.addListener('Event', BleEvent);
    WinBluetoothEventEmitter.addListener('UserNotification', UserNotification);
    WinBluetoothEventEmitter.addListener('DeviceAdded', DeviceWatcherAdded);
    WinBluetoothEventEmitter.addListener('DeviceUpdated', DeviceWatcherUpdated);
    WinBluetoothEventEmitter.addListener('DeviceRemoved', DeviceWatcherRemoved);
    WinBluetoothEventEmitter.addListener('DeviceEnumerationCompleted', DeviceWatcherEnumerationCompleted);
    WinBluetoothEventEmitter.addListener('IsScanningEvent', SetScanning);
    WinBluetoothEventEmitter.addListener('IsConnecting', SetConnecting);

    return () => {

      // componentwillunmount in functional component.
      // Anything in here is fired on component unmount.
      WinBluetoothEventEmitter.addListener('Event', BleEvent).remove();
      WinBluetoothEventEmitter.addListener('UserNotification', UserNotification).remove();
      WinBluetoothEventEmitter.addListener('DeviceAdded', DeviceWatcherAdded).remove();
      WinBluetoothEventEmitter.addListener('DeviceUpdated', DeviceWatcherUpdated).remove();
      WinBluetoothEventEmitter.addListener('DeviceRemoved', DeviceWatcherRemoved).remove();
      WinBluetoothEventEmitter.addListener('IsScanningEvent', SetScanning).remove();
      WinBluetoothEventEmitter.addListener('IsConnecting', SetConnecting).remove();
      WinBluetoothEventEmitter.addListener('DeviceEnumerationCompleted', DeviceWatcherEnumerationCompleted).remove();
    }
  }, []);

  //#region EVENT FUNCTIONS 

  function SetScanning(isScanning: bool) {
    setScanning(isScanning)
    console.log("scanning set to " + isScanning)
  }
  function SetConnecting(isConnecting: boolean) {
    setConnecting(isConnecting)
  }

  function PairDevice(Id: string) {
    setCharactheristic([]);
    const tempChar: Charactheristic[] = [];



    NativeModules.BleManager.Connect(Id)
      .then(data => {
        var resp = JSON.parse(data);
        if (resp.Valid && resp.Content != null) {
          alert("Successfully Connected");
          resp.Content.forEach(el => {
            tempChar.push({ Id: el.Id, Name: el.Name })
          });

          setCharactheristic([tempChar])
        }
        else {
          alert("Fail to connect");
        }
      });

  }
  function DeviceWatcherAdded(obj: string) {
    var device = JSON.parse(obj);
    if (device != null) {
      const tempDevices: BleDevice[] = tempKnownDevices.current || [];
      tempKnownDevices.current = [...tempDevices, { Id: device.Id, Name: device.Name }];
      setKnownDevices(tempKnownDevices.current);
      console.log("device added : " + device.Id + " " + device.Name);
    }
  }

  function DeviceWatcherUpdated(obj: string) {
    var device = JSON.parse(obj);
    console.log("device updated : " + device.Id + " " + device.Name);
  }
  function DeviceWatcherRemoved(obj: string) {

    var device = JSON.parse(obj);

    var index = knownDevices.findIndex((el) => el.Id == device.Id)
    console.log(index);
    if (index != -1) {
      //console.log(knownDevices);
      const temp = (tempKnownDevices.current as BleDevice[]).slice(index, 1);
      console.log(knownDevices);
      setKnownDevices(temp);
      console.log("device removed : " + device.Id + " " + device.Name);
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
  //#endregion


  function aClick() {

    NativeModules.BleManager.SelectBatteryService();
  }

  function bCLick() {

    NativeModules.BleManager.SelectBatteryLevel();
  }

  function cCLick() {

    NativeModules.BleManager.SubscribeChanges();
  }
  function ScanClicked() {
    if (isScanningState != true) {
      // Calling WinBluetooth.add method
      tempKnownDevices.current = [];
      setKnownDevices(tempKnownDevices.current);
      console.log("know device cleared")
    }
    NativeModules.BleManager.StartScan();
  }
  //
  const Item = ({ item, onPress, backgroundColor, textColor, fontSize }) => (
    <TouchableOpacity onPress={onPress} style={[styles.item, backgroundColor]}>
      <Text style={[styles.title, textColor, fontSize]}>{item.Name}</Text>
    </TouchableOpacity>
  );

  const renderDeviceItem = ({ item }) => {
    //const backgroundColor = item.id === selectedId ? "#6e3b6e" : "#f9c2ff";
    //const color = item.id === selectedId ? 'white' : 'black';

    return (
      <Item
        item={item}
        onPress={() => PairDevice(item.Id)}
        backgroundColor={"#6e3b6e"}
        textColor={'white'}
        fontSize={'11'}
      />
    );
  };

  const renderCharacteristicItem = ({ item }) => {
    //const color = item.id === selectedId ? 'white' : 'black';

    return (
      <Item
        item={item}
        backgroundColor={"#f9c2ff"}
        textColor={'black'}
        fontSize={'11'} 
      />
    );
  };






  return (
    <View style={{ padding: 10, height: 500 }}>
      <View style={{
        flexWrap: "wrap",
        flexDirection: "row",
        marginVertical: 10,

      }}>
      <View style={{ flex: 2 }}><Button
        title="Back to home"
        onPress={() =>
          navigation.navigate('Home', { name: 'HomeScreen' })
        }
      /></View>
      <View style={{ flex: 2}}><Button
        title="Back"
        onPress={() =>
          navigation.goBack()
        }
      /></View>
</View>

      <Text style={{
        fontWeight: 'bold', textAlign: "center",
        marginBottom: 10,
        fontSize: 24
      }}>BLE DEMO asd</Text>
      <Text>{NativeModules.BleManager.Connected}</Text>
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
          flex: 2,
          marginRight: 20
        }} >
          <Text style={{ fontWeight: 'bold' }}>sensor List :</Text>
          <Text style={{}}>select device from list below</Text>
          <View style={{ borderColor: 'black', borderStyle: 'solid', borderWidth: 2, height: 250 }} >
            <FlatList
              data={knownDevices}
              renderItem={renderDeviceItem}
              keyExtractor={(item) => item.Id}
              extraData={knownDevices}
            />
          </View>

        </View>
        <View style={{
          flex: 2,
          marginRight: 20
        }} >
          <Text style={{ fontWeight: 'bold' }}>service List :</Text>
          <Text style={{}}>select available service from list below</Text>
          <View style={{ borderColor: 'black', borderStyle: 'solid', borderWidth: 2, height: 250 }} >
            <FlatList
              data={knownCharactheristic}
              renderItem={renderCharacteristicItem}
              keyExtractor={(item) => item.Id}
              extraData={knownCharactheristic}
            />
          </View>
        </View>
        <View style={{
          flex: 2,
          marginRight: 20
        }} >
          <Text style={{ fontWeight: 'bold' }}>Charactheristic List :</Text>
          <Text style={{}}>select Charactheristic service from list below</Text>
          <View style={{ borderColor: 'black', borderStyle: 'solid', borderWidth: 2, height: 250 }} >

          </View>
        </View>
      </View>

      <Button onPress={aClick} title={"select 'Battery' Service"} />
      <Button onPress={bCLick} title={"select 'BatteryLevel' Characteristic"} />
      <Button onPress={cCLick} title={"subscribe changes"} />


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
    padding: 20,
    marginVertical: 8,
    marginHorizontal: 16,
  },
  title: {
    fontSize: 32,
  },
});