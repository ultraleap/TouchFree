import styles from './DocsLink.module.scss';

import classnames from 'classnames/bind';
import { QRCodeSVG } from 'qrcode.react';
import React, { CSSProperties, useState, useRef, useEffect } from 'react';

import { openWithShell } from '@/TauriUtils';

import TouchFree from 'touchfree/src/TouchFree';

import { Alert, TextButton, OutlinedTextButton } from '@/Components';
import { tFClickIsPointer } from '@/Components/TFButton/TFButtons';

const classes = classnames.bind(styles);

interface DocsLinkProps {
    buttonStyle?: CSSProperties;
    title: string;
    url: string;
}

const DocsLink: React.FC<DocsLinkProps> = ({ title, url, buttonStyle }) => {
    const [showModal, setShowModal] = useState<boolean>(false);

    const toggleModal = () => setShowModal(!showModal);

    return (
        <>
            {showModal && <DocsModal url={url} toggleModal={toggleModal} />}
            <OutlinedTextButton title={title} onClick={toggleModal} buttonStyle={buttonStyle} />
        </>
    );
};

interface DocsModalProps {
    url: string;
    toggleModal: () => void;
}

const DocsModal: React.FC<DocsModalProps> = ({ url, toggleModal }) => {
    const [showAlert, setShowAlert] = useState<boolean>(false);
    const timeoutRef = useRef<number>();

    useEffect(() => {
        return () => window.clearTimeout(timeoutRef.current);
    }, []);

    return (
        <>
            <div className={classes('bg-cover')} />
            <div className={classes('docs-modal')}>
                <div
                    className={classes('docs-modal__qr-code')}
                    onPointerEnter={() => TouchFree.GetCurrentCursor()?.SetCursorOpacity(0.5)}
                    onPointerLeave={() => {
                        if (TouchFree.GetCurrentCursor()?.shouldShow) {
                            TouchFree.GetCurrentCursor()?.SetCursorOpacity(1);
                        }
                    }}
                >
                    <QRCodeSVG value={url} style={{ width: '100%', height: '100%' }} />
                </div>
                <h1 className={classes('docs-modal__text')}>Scan QR Code to find help at Ultraleap.com</h1>
                <div className={classes('docs-modal-buttons')}>
                    <div className={classes('docs-modal-buttons--link')}>
                        <TextButton
                            className={classes('docs-modal__button')}
                            title="Open Link"
                            text=""
                            textStyle={{ display: 'none' }}
                            onClick={(event) => {
                                if (tFClickIsPointer(event) && event.pointerType === 'pen') {
                                    if (!showAlert) {
                                        setShowAlert(true);
                                        timeoutRef.current = window.setTimeout(() => setShowAlert(false), 4000);
                                    }
                                } else {
                                    openWithShell(url, () => {
                                        window.open(url, '_blank')?.focus();
                                    });
                                    toggleModal();
                                }
                            }}
                        />
                        <Alert
                            show={showAlert}
                            style={{ width: '100%', position: 'relative' }}
                            text="Cannot be opened using the TouchFree cursor"
                            animationType="fadeInOut"
                            animationTime={4}
                        />
                    </div>
                    <TextButton
                        className={classes('docs-modal__button')}
                        title="Close"
                        text=""
                        textStyle={{ display: 'none' }}
                        onClick={toggleModal}
                    />
                </div>
            </div>
        </>
    );
};

export default DocsLink;
