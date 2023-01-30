import styles from './FileInput.module.scss';
import interactionStyles from '@/Pages/Interactions/Interactions.module.scss';

import classnames from 'classnames/bind';
import React, { ChangeEventHandler } from 'react';

import { FileIcon } from '@/Images';

const classes = classnames.bind(styles);
const interactionClasses = classnames.bind(interactionStyles);

interface FileInputProps {
    name: string;
    value: string;
    acceptedFileTypes?: string;
    onChange: ChangeEventHandler<HTMLInputElement>;
}

const FileInput: React.FC<FileInputProps> = ({ name, value, acceptedFileTypes, onChange }) => {
    return (
        <label className={interactionClasses('input-label-container')}>
            <p className={classes('label')}>{name}</p>
            <label className={classes('container')}>
                <input
                    type="file"
                    accept={acceptedFileTypes}
                    className={classes('container__input')}
                    onChange={onChange}
                />
                <div className={classes('container__path')}>{value}</div>
                <img className={classes('container__icon')} src={FileIcon} />
            </label>
        </label>
    );
};

export default FileInput;
