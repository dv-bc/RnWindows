import React, { useState } from "react";
import { Button, View } from 'react-native';
import BleManager from "./BluetoothManager";
import HomeScreen from "./Home";
import Camera from "./Camera";
import DetailsScreen from "./Details";

export default function Navigator() {

    const [page, setPage] = useState<string>('home');


    const renderNav = () => {
        return (
            <View style={{ padding: 20, flexDirection: "row" }}>

                <View style={{ flex: 1 }}>
                    <Button title={"Ble"} onPress={() => { setPage('ble'); }} />
                    <Button title={"Camera"} onPress={() => { setPage('camera'); }} />
                </View>
                <View style={{ flex: 1 }}>
                    <Button title={"Details"} onPress={() => { setPage('Details'); }} />
                </View>

            </View>
        )
    }


    const renderPage = () => {
        switch (page) {
            case 'Details':
                return <DetailsScreen />
            case 'ble':
                return <BleManager />
            case 'camera':
                return <BleManager />
            default:
                return <BleManager />
        }
    }



    return (
        <View>
            {renderNav()}
            {renderPage()}
        </View>
    );
};