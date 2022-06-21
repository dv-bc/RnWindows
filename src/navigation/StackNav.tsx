
import React, {Component} from 'react';
//import { createSwitchNavigator, createAppContainer } from 'react-navigation';

//import { createStackNavigator, createSwitchNavigator, createAppContainer } from 'react-navigation';
import { createNativeStackNavigator  } from '@react-navigation/native-stack';
import HomeScreen from '../routes/Home';
import BleManagerScreen from '../routes/BluetoothManager';
import CameraScreen from '../routes/Camera';



const Stack = createNativeStackNavigator();

export default function StackNav({ navigation }) {
    return (
        <Stack.Navigator initialRouteName="Home">
          <Stack.Screen name="Home" component={HomeScreen} />
            <Stack.Screen name="BleManager" component={BleManagerScreen} />            

            <Stack.Screen name="Camera" component={CameraScreen} />  
                   
          </Stack.Navigator>
    );
}