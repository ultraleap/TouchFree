import './ControlBar.scss';
import cssVariables from '@/variables.module.scss';

import React, { CSSProperties } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

import { TrackingServiceState } from 'TouchFree/src/TouchFreeToolingTypes';

import { BackArrow, GearIcon, GearIconGlow, Logo } from '@/Images';

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

const tabNames = ['Camera', 'Interactions', 'Settings'] as const;
type TabName = typeof tabNames[number];

const ControlBar: React.FC<ControlBarProps> = ({ tfStatus, touchFreeVersion }) => {
    const [activeTab, setActiveTab] = React.useState<TabName>('Camera');

    const { pathname } = useLocation();
    const navigate = useNavigate();

    if (pathname.includes('calibrate')) return <></>;

    const getBottomContent = () => {
        if (tabNames.map((tabName) => `/settings/${tabName.toLowerCase()}`).includes(pathname)) {
            return (
                <>
                    {tabNames.map((tabName) => {
                        let icon: string | undefined;
                        let hoveredIcon: string | undefined;
                        if (tabName === 'Settings') {
                            icon = GearIcon;
                            hoveredIcon = GearIconGlow;
                        }
                        return (
                            <TabSelector
                                key={tabName}
                                name={tabName}
                                icon={icon}
                                hoveredIcon={hoveredIcon}
                                isActiveTab={activeTab === tabName}
                                onClick={() => {
                                    setActiveTab(tabName);
                                    navigate(`/settings/${tabName.toLowerCase()}`);
                                }}
                            />
                        );
                    })}
                </>
            );
        }
        return (
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
        );
    };

    return (
        <div className="control-bar-container">
            <div className="control-bar-top">
                <StatusIndicator tfStatus={tfStatus} />
                <img src={Logo} alt="Logo: TouchFree by UltraLeap" className="control-bar-logo" />
                <VersionIndicator touchFreeVersion={touchFreeVersion} />
            </div>
            <div className="control-bar-bottom">{getBottomContent()}</div>
        </div>
    );
};

export default ControlBar;
