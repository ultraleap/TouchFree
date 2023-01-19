import './DocsLink.scss';

import { QRCodeSVG } from 'qrcode.react';
import React, { CSSProperties, useState, useRef, useEffect } from 'react';

import TouchFree from 'TouchFree/src/TouchFree';

import { Alert, BaseTFButton, TextButton } from '@/Components';
import { BaseButtonProps, tFClickIsPointer } from '@/Components/TFButton/TFButtons';

interface DocsLinkProps {
    buttonStyle?: CSSProperties;
    title: string;
    titleStyle?: CSSProperties;
    url: string;
}

const DocsLink: React.FC<DocsLinkProps> = ({ buttonStyle, title, titleStyle, url }) => {
    const [showModal, setShowModal] = useState<boolean>(false);

    const toggleModal = () => setShowModal(!showModal);

    return (
        <>
            {showModal && <DocsModal url={url} toggleModal={toggleModal} />}
            <DocsOpenButton buttonStyle={buttonStyle} title={title} titleStyle={titleStyle} onClick={toggleModal} />
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
            <div className="bg-cover" />
            <div className="docs-modal">
                <div
                    className="docs-modal__qr-code"
                    onPointerEnter={() => TouchFree.GetCurrentCursor()?.SetCursorOpacity(0.5)}
                    onPointerLeave={() => {
                        if (TouchFree.GetCurrentCursor()?.shouldShow) {
                            TouchFree.GetCurrentCursor()?.SetCursorOpacity(1);
                        }
                    }}
                >
                    <QRCodeSVG value={url} style={{ width: '100%', height: '100%' }} />
                </div>
                <h1 className="docs-modal__text">Scan QR Code to find help at Ultraleap.com</h1>
                <div className="docs-modal-buttons">
                    <div className="docs-modal-buttons--link">
                        <TextButton
                            className="docs-modal__button"
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
                                    window.open(url, '_blank')?.focus();
                                    toggleModal();
                                }
                            }}
                        />
                        <Alert
                            show={showAlert}
                            style={{ width: '40%' }}
                            text="Cannot be opened using the TouchFree cursor"
                            animationType="fadeInOut"
                            animationTime={4}
                        />
                    </div>
                    <TextButton
                        className="docs-modal__button"
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

const DocsOpenButton: React.FC<BaseButtonProps> = ({ buttonStyle, title, titleStyle, onClick }) => {
    const buttonContent = <h1 style={titleStyle}>{title}</h1>;

    return <span id="docs-open">{BaseTFButton('docs-open__button', true, onClick, buttonContent, buttonStyle)}</span>;
};

export default DocsLink;