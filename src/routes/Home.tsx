
import React from 'react';
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

export default function HomeScreen({ navigation }) {
    return (
        <View style={{ flex: 1, alignItems: 'center', justifyContent: 'center' }}>
            <Text>Home Screen asd</Text>
            <Button title="Go to Home" onPress={() => navigation.navigate('Home', { name: 'HomeScreen' })} />
            <Button title="Go to Ble" onPress={() => navigation.navigate('BleManager', { name: 'BleManager' })} />
            <Button title="Go to Camera" onPress={() => navigation.navigate('Camera', { name: 'CameraScreen' })} />                     
        </View>
    );
}