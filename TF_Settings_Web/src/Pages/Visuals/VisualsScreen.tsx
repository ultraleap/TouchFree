import styles from './Visuals.module.scss';

import classNames from 'classnames/bind';
import React from 'react';

import { useIsLinux } from '@/customHooks';

import { OutlinedTextButton, RadioGroup } from '@/Components';

const classes = classNames.bind(styles);

const StyleOptions: string[] = ['Recommended (Light)', 'Recommended (Dark)', 'Solid (Light)', 'Solid (Dark)', 'Custom'];

const VisualsScreen: React.FC = () => {
    if (useIsLinux()) return <></>;

    return (
        <div className={classes('container')}>
            <div className={classes('title-line')}>
                <h1> Visuals </h1>
                <div className={classes('misc-button-container')}>
                    <OutlinedTextButton title="Reset to Default" onClick={() => console.log('RESET')} />
                </div>
            </div>
            <div className={classes('section-container')}>
                <div className={classes('content')}>
                    <div className={classes('horizontalContainer')}>
                        <RadioGroup
                            name="InteractionType"
                            selected={0}
                            options={StyleOptions}
                            onChange={(value) => console.log(value)}
                        />
                    </div>
                </div>
            </div>
            <div className={classes('page-divider')} />
        </div>
    );
};

export default VisualsScreen;
