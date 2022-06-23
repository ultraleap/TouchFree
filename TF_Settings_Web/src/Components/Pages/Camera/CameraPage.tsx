import '../../../Styles/Camera/CameraPage.css';

import { Page } from '../Page';
import { ManualSetupPage } from './ManualSetupPage';
import { CameraPositionPage } from './CameraPosition';
import { ConnectionManager } from '../../../TouchFree/Connection/ConnectionManager';
import { ConfigurationManager } from '../../../TouchFree/Configuration/ConfigurationManager';
import { ConfigState } from '../../../TouchFree/Connection/TouchFreeServiceTypes';
import CameraSetupSelection from './CameraSetupSelection';

export type SetupType = 'Quick' | 'Manual' | undefined;
export type PositionType = 'FaceUser' | 'FaceScreen' | 'Below' | undefined;

interface CameraPageState {
    activeSetup: SetupType;
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

export class CameraPage extends Page<{}, CameraPageState> {
    constructor(props: {}) {
        super(props);
        this.state = { activeSetup: undefined, activePosition: undefined };
    }

    componentDidMount() {
        ConnectionManager.AddConnectionListener(() => {
            ConfigurationManager.RequestConfigFileState((config: ConfigState) => {
                this.setState({ activePosition: getPositionFromConfig(config) });
            });
        });
    }

    render() {
        switch (this.state.activeSetup) {
            case 'Quick':
                return (
                    <CameraPositionPage
                        configPosition={this.state.activePosition}
                        onClick={(position: PositionType) => this.setState({ activePosition: position })}
                    />
                );
            case 'Manual':
                return <ManualSetupPage />;
            default:
                return <CameraSetupSelection onClick={(setup: SetupType) => this.setState({ activeSetup: setup })} />;
        }
    }
}
