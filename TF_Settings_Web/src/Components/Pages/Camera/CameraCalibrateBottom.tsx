import '../../../Styles/Camera/Calibrate.css';

import { useNavigate } from 'react-router-dom';

import InteractionGuideIcon from '../../../Images/Camera/Interaction_Guide_Bottom.png';
import { CalibrateCancelButton, CalibrateInstructions } from './CalibrationComponents';

const CameraCalibrateBottom = () => {
    const navigate = useNavigate();
    return (
        <div style={{ height: '100%', alignItems: 'center' }}>
            <img
                className="interactionGuide"
                style={{ paddingTop: '430px', marginBottom: '120px' }}
                src={InteractionGuideIcon}
                alt="Guide demonstrating how to interact with Quick Setup"
                onPointerUp={() => {
                    navigate('/camera/quick/calibrate/complete');
                }}
            />
            <CalibrateInstructions />
            <CalibrateCancelButton buttonStyle={{ marginTop: '140px' }} />
        </div>
    );
};

export default CameraCalibrateBottom;
