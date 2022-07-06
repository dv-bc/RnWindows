
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
import { RNCamera } from 'react-native-camera';


export default function Camera() {

    return (
        <View style={{}}>
            <Text style={{
                fontWeight: 'bold', textAlign: "center",
                marginBottom: 10,
                fontSize: 24
            }}>Camera Screen</Text>

<View style={{
          flex: 1,
          marginRight: 20
        }} >
          <View style={{ borderColor: 'black', borderStyle: 'solid', borderWidth: 2, height: 400 }} >
          <RNCamera></RNCamera>
          </View>

        </View>



        </View>
    );
}



