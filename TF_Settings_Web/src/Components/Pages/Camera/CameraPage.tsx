import '../../../Styles/Camera/CameraPage.css';

import { ManualSetup } from './ManualSetup';
import { CameraPositionPage } from './CameraPosition';
import { ConnectionManager } from '../../../TouchFree/Connection/ConnectionManager';
import { ConfigurationManager } from '../../../TouchFree/Configuration/ConfigurationManager';
import { ConfigState } from '../../../TouchFree/Connection/TouchFreeServiceTypes';
import CameraSetupSelection from './CameraSetupSelection';
import { Component } from 'react';
import { Route, Routes } from 'react-router-dom';

export type PositionType = 'FaceUser' | 'FaceScreen' | 'Below' | undefined;

interface CameraPageState {
    activePosition: PositionType;
}

const getPositionFromConfig = (config: ConfigState): PositionType => {
    const leapRotation = config.physical.LeapRotationD;
    if (Math.abs(leapRotation.Z) > 90) {
        if (leapRotation.X <= 0) {
            return 'FaceUser';
        }
        return 'FaceScreen';
    }
    return 'Below';
};

export class CameraPage extends Component<{}, CameraPageState> {
    constructor(props: {}) {
        super(props);
        this.state = { activePosition: undefined };
    }

    componentDidMount() {
        ConnectionManager.AddConnectionListener(() => {
            ConfigurationManager.RequestConfigFileState((config: ConfigState) => {
                this.setState({ activePosition: getPositionFromConfig(config) });
            });
        });
    }

    render() {
        return (
            <>
                <Routes>
                    <Route path="" element={<CameraSetupSelection />} />
                    <Route
                        path="quick"
                        element={
                            <CameraPositionPage
                                configPosition={this.state.activePosition}
                                onClick={(position: PositionType) => this.setState({ activePosition: position })}
                            />
                        }
                    />
                    <Route path="manual" element={<ManualSetup />} />
                </Routes>
            </>
        );
    }
}
