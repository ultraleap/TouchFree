import styles from './FileInput.module.scss';
import interactionStyles from '@/Pages/Interactions/Interactions.module.scss';

import classnames from 'classnames/bind';
import React from 'react';

import { isDesktop } from '@/TauriUtils';
import { openFilePicker } from '@/TauriUtils';

import { FileIcon } from '@/Images';

const classes = classnames.bind(styles);
const interactionClasses = classnames.bind(interactionStyles);

interface FileInputProps {
    name: string;
    value: string;
    acceptedExtensions: string[];
    onFilePicked: (path: string) => void;
}

// Only to be used when inside a Tauri context (as openFilePickers uses Tauri dialogs)
const FileInput: React.FC<FileInputProps> = ({ name, value, acceptedExtensions, onFilePicked }) => {
    if (!isDesktop()) return <></>;

    return (
        <label className={interactionClasses('input-label-container')}>
            <p className={classes('label')}>{name}</p>
            <label
                className={classes('container')}
                onClick={() => {
                    openFilePicker(acceptedExtensions)
                        .then((path) => {
                            if (typeof path === 'string' && !Array.isArray(path)) {
                                onFilePicked(path.replaceAll('\\', '/'));
                            }
                        })
                        .catch((e) => console.error(e));
                }}
            >
                <div className={classes('container__path')}>{value}</div>
                <img className={classes('container__icon')} src={FileIcon} />
            </label>
        </label>
    );
};

export default FileInput;
