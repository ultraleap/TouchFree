import './ControlBar.scss';
import cssVariables from '@/variables.module.scss';

import React, { CSSProperties } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

import { TrackingServiceState } from 'TouchFree/src/TouchFreeToolingTypes';

import { BackArrow, Logo } from '@/Images';

import { VerticalIconTextButton } from '@/Components';

import { StatusIndicator, TabSelector, VersionIndicator } from './';

interface ControlBarProps {
    tfStatus: TrackingServiceState;
    touchFreeVersion: string;
}

const backButtonStyle: CSSProperties = {
    width: '200px',
    height: '80px',
    borderRadius: '30px',
    marginBottom: '30px',
    background: cssVariables.lightGreyGradient,
    display: 'flex',
    justifyContent: 'center',
    alignSelf: 'flex-end',
    alignItems: 'center',
};

const backButtonIconStyle: CSSProperties = {
    margin: '0',
    marginRight: '0.8rem',
};

const backButtonTitleStyle: CSSProperties = {
    fontSize: '28px',
    padding: '0',
    margin: '0',
};

const getBackLocation = (path: string): string => {
    const lastIndex = path.endsWith('/') ? path.slice(0, -1).lastIndexOf('/') : path.lastIndexOf('/');

    return path.slice(0, lastIndex);
};

type TabName = 'Camera' | 'Interactions';

const ControlBar: React.FC<ControlBarProps> = ({ tfStatus, touchFreeVersion }) => {
    const [activeTab, setActiveTab] = React.useState<TabName>('Camera');

    const { pathname } = useLocation();
    const navigate = useNavigate();

    return pathname.includes('calibrate') ? (
        <></>
    ) : (
        <div className="control-bar-container">
            <div className="control-bar-top">
                <StatusIndicator tfStatus={tfStatus} />
                <img src={Logo} alt="Logo: TouchFree by UltraLeap" className="control-bar-logo" />
                <VersionIndicator touchFreeVersion={touchFreeVersion} />
            </div>
            <div className="control-bar-bottom">
                {pathname === '/settings/camera' || pathname === '/settings/interactions' ? (
                    <>
                        <TabSelector
                            name="Camera"
                            isActiveTab={activeTab === 'Camera'}
                            onClick={() => setActiveTab('Camera')}
                        />
                        <TabSelector
                            name="Interactions"
                            isActiveTab={activeTab === 'Interactions'}
                            onClick={() => setActiveTab('Interactions')}
                        />
                    </>
                ) : (
                    <VerticalIconTextButton
                        buttonStyle={backButtonStyle}
                        icon={BackArrow}
                        alt="Arrow pointing back"
                        iconStyle={backButtonIconStyle}
                        title="Back"
                        titleStyle={backButtonTitleStyle}
                        text={''}
                        textStyle={{ display: 'none' }}
                        onClick={() => {
                            navigate(getBackLocation(pathname));
                        }}
                    />
                )}
            </div>
        </div>
    );
};

export default ControlBar;
