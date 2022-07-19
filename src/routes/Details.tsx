
import React, { Component } from 'react';
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
const WinModuleEventEmitter = new NativeEventEmitter(NativeModules.Events);

const REALM_API_KEY_US = 'chuQTXRIMshtQlc7o4FjJBzTdLQiHlLcrWOKNcLqT6BHWZZGZtXnjISmPVZiMs2I';
const REALM_APPLICATION_ID_US = 'application-0-ryrbl';


function GetSite(){
  NativeModules.RnData.GetAll("AU","Site").then((data: string) => {      
    var resp = JSON.parse(data);
    console.log(resp)
  });
}

export default function DetailsScreen() {

    NativeModules.RnData.Init("AU",REALM_APPLICATION_ID_US,REALM_API_KEY_US,"aabad87f-9bf0-458c-923a-3980505023ed").then((data: string) => {      
      var resp = JSON.parse(data);
      console.log(resp)
    });
    NativeModules.RnData.Init("US",REALM_APPLICATION_ID_US,REALM_API_KEY_US,"aabad87f-9bf0-458c-923a-3980505023ed").then((data: string) => {
      var resp = JSON.parse(data);
      console.log(resp)
    });
    return (

        <View style={{ padding: 10, height: 600 }}>
        <Text style={{
          fontWeight: 'bold', textAlign: "center",
          marginBottom: 10,
          fontSize: 24
        }}>Mongo Demo</Text>
        
        <View style={{
          flexWrap: "wrap",
          flexDirection: "row",
          marginVertical: 10
        }}>
          <View style={{ flex: 1 }} >
          <Button onPress={GetSite} title="GetSite" />
  
  
          </View>
          <View style={{ flex: 3 }} >
          </View>
        </View>
        
  
  
  
  
      </View>
    );
}



