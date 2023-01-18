import classes from './Camera.module.scss';

import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { ConnectionManager } from 'TouchFree/src/Connection/ConnectionManager';

import { QuickSetupIcon, ManualSetupIcon, CameraMaskingIcon } from '@/Images';

import { Alert, DocsLink, HorizontalIconTextButton, VerticalIconTextButton } from '@/Components';

const CameraSetupScreen = () => {
    const navigate = useNavigate();

    const [touchFreeConnected, setTouchFreeConnected] = useState<boolean>(false);

    const onConnected = () => {
        setTouchFreeConnected(true);
    };

    useEffect(() => {
        ConnectionManager.AddConnectionListener(onConnected);
    }, []);

    return (
        <div className={classes['camera-setup-container']}>
            <div className={classes['sub-container']}>
                <h1 className={classes['title-line']}> Camera Setup </h1>
                <div className={classes['tf-button-container']}>
                    <VerticalIconTextButton
                        icon={QuickSetupIcon}
                        alt="Icon for Quick Setup option"
                        iconStyle={{ margin: '30px 0px', height: '250px' }}
                        title="Auto Calibration"
                        text="Our automatic calibration enables you to set up quickly"
                        onClick={() => navigate('quick')}
                    />
                    <VerticalIconTextButton
                        icon={ManualSetupIcon}
                        alt="Icon for Manual Setup option"
                        iconStyle={{ margin: '65px 0px', height: '180px' }}
                        title="Manual Calibration"
                        text="Full control of your calibration"
                        onClick={() => navigate('manual')}
                    />
                </div>
            </div>
            <div className={classes['camera-page-divider']} />
            <div className={classes['sub-container']}>
                <h1 className={classes['title-line']}> Tools </h1>
                <HorizontalIconTextButton
                    buttonStyle={{
                        opacity: touchFreeConnected ? 1 : 0.5,
                        pointerEvents: touchFreeConnected ? 'auto' : 'none',
                    }}
                    icon={CameraMaskingIcon}
                    alt="Icon for Camera Masking"
                    iconStyle={{ marginBottom: '0px' }}
                    title="Camera Masking"
                    text="Mask areas of your cameras vision from reflections and harsh areas of light"
                    onClick={() => navigate('masking')}
                />
                <Alert
                    show={!touchFreeConnected}
                    style={{ width: '60%' }}
                    text="Failed to connect to service: masking unavailable"
                    animationType="fadeIn"
                    animationTime={1}
                />
            </div>
            <DocsLink title="Support" url="https://www.ultraleap.com/contact-us/" />
        </div>
    );
};

export default CameraSetupScreen;
