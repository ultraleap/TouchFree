import '../../../Styles/Camera/CameraPage.css';

import IconTextButton from '../../Controls/IconTextButton';

import { PositionType } from './CameraPage';
import CameraFacingUserIcon from '../../../Images/Camera_Facing_User.svg';
import CameraFacingScreenIcon from '../../../Images/Camera_Facing_Screen.svg';
import CameraBelowIcon from '../../../Images/Camera_Below.svg';
import { Component } from 'react';

const buttonStyle: React.CSSProperties = { width: '48.75%', height: '350px' };
const iconStyle: React.CSSProperties = { marginTop: '20px', height: '220px' };
const textStyle: React.CSSProperties = { color: '#00EB85', opacity: '1' };

interface CameraPositionProps {
    onClick: (position: PositionType) => void;
    configPosition: PositionType;
}

export class CameraPositionPage extends Component<CameraPositionProps, {}> {
    constructor(props: CameraPositionProps) {
        super(props);
    }

    render() {
        return (
            <div>
                <div className="titleLine">
                    <h1> Where is Your Camera Positioned? </h1>
                </div>
                <div className="IconTextButtonDiv">
                    <IconTextButton
                        buttonStyle={buttonStyle}
                        icon={CameraFacingUserIcon}
                        iconStyle={iconStyle}
                        title="Camera Above Facing User"
                        text={this.props.configPosition === 'FaceUser' ? 'Current Setup' : ''}
                        textStyle={textStyle}
                        onClick={() => this.props.onClick('FaceUser')}
                    />
                    <IconTextButton
                        buttonStyle={buttonStyle}
                        icon={CameraFacingScreenIcon}
                        iconStyle={iconStyle}
                        title="Camera Above Facing Screen"
                        text={this.props.configPosition === 'FaceScreen' ? 'Current Setup' : ''}
                        textStyle={textStyle}
                        onClick={() => this.props.onClick('FaceScreen')}
                    />
                    <IconTextButton
                        buttonStyle={{ ...buttonStyle, marginTop: '2.5%' }}
                        icon={CameraBelowIcon}
                        iconStyle={iconStyle}
                        title="Camera Below"
                        text={this.props.configPosition === 'Below' ? 'Current Setup' : ''}
                        textStyle={textStyle}
                        onClick={() => this.props.onClick('Below')}
                    />
                </div>
            </div>
        );
    }
}
