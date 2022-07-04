import '../Styles/ControlBar.css';

import React, { CSSProperties } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

import backArrow from '../Images/Back_Arrow.svg';
import logo from '../Images/Logo.png';
import { TrackingServiceState } from '../TouchFree/TouchFreeToolingTypes';
import IconTextButton from './Controls/IconTextButton';
import TabSelector from './Controls/TabSelector';
import { StatusIndicator } from './StatusIndicator';

interface ControlBarProps {
    tfStatus: TrackingServiceState;
}

const backButtonStyle: CSSProperties = {
    width: '180px',
    height: '60px',
    borderRadius: '30px',
    marginLeft: '1%',
    marginBottom: '2%',
    background: 'transparent linear-gradient(180deg, #5c5c5c 0%, #454545 100%) 0% 0% no-repeat padding-box',
    display: 'flex',
    justifyContent: 'center',
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

const ControlBar: React.FC<ControlBarProps> = ({ tfStatus }) => {
    const [activeTab, setActiveTab] = React.useState<TabName>('Camera');

    const { pathname } = useLocation();
    const navigate = useNavigate();

    return pathname.includes('calibrate') ? (
        <></>
    ) : (
        <div className="overallContainerStyle">
            <div className="topBarStyle">
                <StatusIndicator tfStatus={tfStatus} />
                <img src={logo} alt="Logo: TouchFree by UltraLeap" className="horizElement" />
                <div className="emptyContainer" />
            </div>
            <div className="tabBarStyle">
                {pathname === '/camera' || pathname === '/interactions' ? (
                    <div className="tabBarStyle">
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
                    </div>
                ) : (
                    <IconTextButton
                        buttonStyle={backButtonStyle}
                        icon={backArrow}
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
