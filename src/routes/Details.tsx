
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


import { useNavigation } from '@react-navigation/native';
export default function DetailsScreen() {
    
    const navigation = useNavigation();
    return (
        <View style={{ flex: 1, alignItems: 'center', justifyContent: 'center' }}>
            <Text>This is Details Screen asd</Text>


            <Button title="Go to Home" onPress={() => navigation.navigate('Home')} />
            <Button title="Go to Ble" onPress={() => navigation.navigate('BleManager')} />
            <Button title="Go to Camera" onPress={() => navigation.navigate('Camera')} /> 



        </View>
    );
}



