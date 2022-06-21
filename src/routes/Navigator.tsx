import React, { useState } from "react";
import { Button, View } from 'react-native';
import BleManager from "./BluetoothManager";
import HomeScreen from "./Home";

export default function Navigator() {

    const [page, setPage] = useState<string>('home');


    const renderNav = () => {
        return (
            <View>
                <Button title={"Home"} onPress={() => {
                    setPage('home');
                }} />
                <Button title={"Ble"} onPress={() => {
                    setPage('ble');
                }} />
            </View>
        )
    }


    const renderPage = () => {
        switch (page) {
            case 'home':
                return <HomeScreen />
            case 'ble':
                return <BleManager />
            default:
                return <HomeScreen />
        }
    }



    return (
        <View>
            {renderNav()}
            {renderPage()}
        </View>
    );
};