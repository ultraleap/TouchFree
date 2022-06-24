import '../../../Styles/Camera/Calibrate.css';

import { CSSProperties } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import FingerprintIcon from '../../../Images/Camera/Fingerprint_Icon.svg';
import InteractionGuideIcon from '../../../Images/Camera/Interaction_Guide.png';
import IconTextButton from '../../Controls/IconTextButton';
import { PositionType } from './CameraPage';

const setupButtonStyle: CSSProperties = {
    width: '300px',
    height: '80px',
    borderRadius: '33px',
    marginTop: '360px',
    background: 'transparent linear-gradient(180deg, #5c5c5c 0%, #454545 100%) 0% 0% no-repeat padding-box',
};

const setupButtonTitleStyle: CSSProperties = {
    fontSize: '2rem',
    padding: '0',
    textAlign: 'center',
    margin: '0',
    height: '100%',
};

const CameraCalibrateTop = () => {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const position = useParams().position as PositionType;

    const navigate = useNavigate();

    return (
        <div style={{ height: '100%' }}>
            <div className="instructions">
                <h1>
                    Hold INDEX FINGER against
                    <br /> this <span style={{ color: '#01EB85' }}>GREEN CIRCLE</span>
                </h1>
                <div className="arrow">
                    <div id="downLine" />
                    <div id="arrowHead" />
                </div>
                <div id="touchCircle">
                    <img src={FingerprintIcon} />
                </div>
                <img id="interactionGuide" src={InteractionGuideIcon} />
            </div>
            <IconTextButton
                buttonStyle={setupButtonStyle}
                icon={''}
                alt=""
                title="Cancel Setup"
                titleStyle={setupButtonTitleStyle}
                text={''}
                textStyle={{ display: 'none' }}
                onClick={() => navigate(-1)}
            />
        </div>
    );
};

export default CameraCalibrateTop;
