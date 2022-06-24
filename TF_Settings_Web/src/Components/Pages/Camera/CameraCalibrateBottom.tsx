import '../../../Styles/Camera/Calibrate.css';

import { useNavigate, useParams } from 'react-router-dom';

import InteractionGuideIcon from '../../../Images/Camera/Interaction_Guide_Bottom.png';
import { CalibrateCancelButton, CalibrateInstructions } from './CalibrationComponents';
import { PositionType } from './CameraPage';

const CameraCalibrateBottom = () => {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const position = useParams().position as PositionType;
    const navigate = useNavigate();
    return (
        <div style={{ height: '100%', alignItems: 'center' }}>
            <img
                className="interactionGuide"
                style={{ marginTop: '600px', marginBottom: '120px' }}
                src={InteractionGuideIcon}
                alt="Guide demonstrating how to interact with Quick Setup"
                onClick={() => {
                    navigate('/');
                }}
            />
            <CalibrateInstructions />
            {/* Setup Button starts at 1780px */}
            <CalibrateCancelButton buttonStyle={{ marginTop: '140px' }} />
        </div>
    );
};

export default CameraCalibrateBottom;
