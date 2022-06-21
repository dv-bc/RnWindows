
import React, {Component} from 'react';
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



export default function Camera({ navigation }) {
    return (
        <View style={{ flex: 1, alignItems: 'center', justifyContent: 'center' }}>
            <Text>This is Camera Screen asd</Text>


            <Button title="Go to Home" onPress={() => navigation.navigate('Home', { name: 'HomeScreen' })} />
            <Button title="Go to Ble" onPress={() => navigation.navigate('BleManager', { name: 'BleManager' })} />



        </View>
    );
}



