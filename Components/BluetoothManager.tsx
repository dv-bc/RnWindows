import React, { Component, useState, useEffect } from 'react';
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





export default function BleManager() {
  const initialDevice: BleDevice[] = [
    { Id: "", Name: "Initial" }
  ];
  const [knownDevices, setDevices] = useState(initialDevice);
  console.log(knownDevices);

  useEffect(() => {
    // componentDidMount in functional component.
    WinBluetoothEventEmitter.addListener('Event', BleEvent);
    WinBluetoothEventEmitter.addListener('UserNotification', UserNotification);
    WinBluetoothEventEmitter.addListener('DeviceAdded', DeviceWatcherAdded);
    WinBluetoothEventEmitter.addListener('DeviceUpdated', DeviceWatcherUpdated);
    WinBluetoothEventEmitter.addListener('DeviceRemoved', DeviceWatcherRemoved);
    WinBluetoothEventEmitter.addListener('DeviceEnumerationCompleted', DeviceWatcherEnumerationCompleted);

    return () => {

      // componentwillunmount in functional component.
      // Anything in here is fired on component unmount.
      WinBluetoothEventEmitter.addListener('Event', BleEvent).remove();
      WinBluetoothEventEmitter.addListener('UserNotification', UserNotification).remove();
      WinBluetoothEventEmitter.addListener('DeviceAdded', DeviceWatcherAdded).remove();
      WinBluetoothEventEmitter.addListener('DeviceUpdated', DeviceWatcherUpdated).remove();
      WinBluetoothEventEmitter.addListener('DeviceRemoved', DeviceWatcherRemoved).remove();
      WinBluetoothEventEmitter.addListener('DeviceEnumerationCompleted', DeviceWatcherEnumerationCompleted).remove();
    }
  },[]);

function PairDevice(Id:string)
{
  NativeModules.BleManager.PairDevice(Id);

}
  function DeviceWatcherAdded(obj: string) {
    var device = JSON.parse(obj);
    //knownDevices.push({ Id: device.Id, Name: device.Name })
    setDevices(knownDevices);

    console.log(knownDevices);
    console.log("device added : " + device.Id + " " + device.Name);
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
      console.log(knownDevices);
      knownDevices.splice(index, 1);
      console.log(knownDevices);
      setDevices(knownDevices);
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

  function ScanClicked() {
    let device : BleDevice[] = [];
    setDevices(device);
    // Calling WinBluetooth.add method
    NativeModules.BleManager.StartScan();
  }
//
const Item = ({ item, onPress, backgroundColor, textColor }) => (
  <TouchableOpacity onPress={onPress} style={[styles.item, backgroundColor]}>
    <Text style={[styles.title, textColor]}>{item.Name}</Text>
  </TouchableOpacity>
);

  const renderItem = ({ item }) => {
    //const backgroundColor = item.id === selectedId ? "#6e3b6e" : "#f9c2ff";
    //const color = item.id === selectedId ? 'white' : 'black';

    return (
      <Item
        item={item}
        onPress={() => PairDevice(item.Id)}
        backgroundColor={"#6e3b6e" }
        textColor={ 'white' }
      />
    );
  };

  return (
    <View>
      <Text>BleManagers says  = {NativeModules.BleManager.Connected}</Text>
      <Button onPress={ScanClicked} title="Scan for sensor" />
      <FlatList
        data={knownDevices}
        renderItem={renderItem}
        keyExtractor={(item) => item.Id}
        extraData={knownDevices}
      />
    </View>);
}





const styles = StyleSheet.create({
  container: {
    flex: 1,
    marginTop: StatusBar.currentHeight || 0,
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