import styles from './Camera.module.scss';

import classnames from 'classnames/bind';
import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { useIsLandscape } from '@/customHooks';

import { TrackingServiceState } from 'TouchFree/src/TouchFreeToolingTypes';

import { QuickSetupIcon, ManualSetupIcon, CameraMaskingIcon } from '@/Images';

import { Alert, DocsLink, HorizontalIconTextButton, VerticalIconTextButton } from '@/Components';
import { TabBar } from '@/Components/ControlBar';

const classes = classnames.bind(styles);

interface CameraSetupScreenProps {
    trackingStatus: TrackingServiceState;
}

const CameraSetupScreen: React.FC<CameraSetupScreenProps> = ({ trackingStatus }) => {
    const [isConnected, setIsConnected] = useState<boolean>(false);
    const navigate = useNavigate();
    const isLandscape = useIsLandscape();

    useEffect(() => {
        setIsConnected(trackingStatus !== TrackingServiceState.UNAVAILABLE);
    }, [trackingStatus]);

    return (
        <>
            <TabBar />
            <div className={classes('camera-setup-container')}>
                <div className={classes('sub-container')}>
                    <h1 className={classes('title-line')}> Camera Setup </h1>
                    <div className={classes('tf-button-container')}>
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
                {isLandscape ? <></> : <div className={classes('page-divider')} />}
                <div className={classes('sub-container')}>
                    <h1 className={classes('title-line')}> Tools </h1>
                    <HorizontalIconTextButton
                        buttonStyle={{
                            opacity: isConnected ? 1 : 0.5,
                            pointerEvents: isConnected ? 'auto' : 'none',
                        }}
                        icon={CameraMaskingIcon}
                        alt="Icon for Camera Masking"
                        iconStyle={{ marginBottom: '0px' }}
                        title="Camera Masking"
                        text="Mask areas of your cameras vision from reflections and harsh areas of light"
                        onClick={() => navigate('masking')}
                    />
                    <Alert
                        show={!isConnected}
                        style={{ width: '60%', marginLeft: '20%', position: 'relative' }}
                        text="Failed to connect to service: masking unavailable"
                        animationType="fadeIn"
                        animationTime={1}
                    />
                </div>
                <DocsLink
                    title="Support"
                    url="https://www.ultraleap.com/contact-us/"
                    buttonStyle={{ position: 'fixed', bottom: '2vh', right: '2vh' }}
                />
            </div>
        </>
    );
};

export default CameraSetupScreen;
