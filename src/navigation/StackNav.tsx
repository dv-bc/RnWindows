
import React, {Component} from 'react';
//import { createSwitchNavigator, createAppContainer } from 'react-navigation';

//import { createStackNavigator, createSwitchNavigator, createAppContainer } from 'react-navigation';
import { createNativeStackNavigator  } from '@react-navigation/native-stack';
import HomeScreen from '../routes/Home';
import BleManagerScreen from '../routes/BluetoothManager';
import CameraScreen from '../routes/Camera';
import DetailsScreen from '../routes/Details';


const Stack = createNativeStackNavigator();

export default function StackNav() {
    
  return (
        <Stack.Navigator initialRouteName="Home">
          <Stack.Screen name="Home" component={HomeScreen} />
            <Stack.Screen name="BleManager" component={BleManagerScreen} />            

            <Stack.Screen name="Camera" component={CameraScreen} />  
            <Stack.Screen name="Details" component={DetailsScreen} />  
                   
          </Stack.Navigator>
    );
}