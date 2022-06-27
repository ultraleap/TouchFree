import '../../../Styles/Camera/Calibrate.css';

import { useNavigate, useParams } from 'react-router-dom';

import InteractionGuideIcon from '../../../Images/Camera/Interaction_Guide_Top.png';
import { CalibrateCancelButton, CalibrateInstructions } from './CalibrationComponents';
import { PositionType } from './CameraPage';

const CameraCalibrateTop = () => {
    const position = useParams().position as PositionType;
    const navigate = useNavigate();
    return (
        <div style={{ height: '100%', alignItems: 'center', marginTop: '-40px' }}>
            <CalibrateInstructions />
            <img
                className="interactionGuide"
                style={{ marginTop: '150px' }}
                src={InteractionGuideIcon}
                alt="Guide demonstrating how to interact with Quick Setup"
                onPointerUp={() => {
                    navigate(`/camera/quick/${position}/calibrateBottom`);
                }}
            />
            <CalibrateCancelButton buttonStyle={{ marginTop: '580px' }} />
        </div>
    );
};

export default CameraCalibrateTop;
