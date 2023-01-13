import './DocsLink.scss';

import { QRCodeSVG } from 'qrcode.react';
import React, { CSSProperties, useState } from 'react';

import { BaseTFButton } from '@/Components';
import { BaseButtonProps, TextButton } from '@/Components/TFButton/TFButtons';

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
            <DocsButton buttonStyle={buttonStyle} title={title} titleStyle={titleStyle} onClick={toggleModal} />
        </>
    );
};

interface DocsModalProps {
    url: string;
    toggleModal: () => void;
}

const DocsModal: React.FC<DocsModalProps> = ({ url, toggleModal }) => (
    <>
        <div className="docs-modal--cover" />
        <div className="docs-modal--container">
            <div className="qr-code--container">
                <div className="qr-img--container">
                    <QRCodeSVG value={url} style={{ width: '100%', height: '100%' }} />
                </div>
                <h1>Scan QR Code to find help at Ultraleap.com</h1>
            </div>
            <div className="docs-modal-buttons--container">
                <TextButton
                    className="modal-button"
                    title="Open Link"
                    text=""
                    textStyle={{ display: 'none' }}
                    onClick={() => {
                        window.open(url, '_blank')?.focus();
                        toggleModal();
                    }}
                />
                <TextButton
                    className="modal-button"
                    title="Close"
                    text=""
                    textStyle={{ display: 'none' }}
                    onClick={toggleModal}
                />
            </div>
        </div>
        ;
    </>
);

const DocsButton: React.FC<BaseButtonProps> = ({ buttonStyle, title, titleStyle, onClick }) => {
    const buttonContent = <h1 style={titleStyle}>{title}</h1>;

    return (
        <span id="docs-button--container">
            {BaseTFButton('docs-button', true, onClick, buttonContent, buttonStyle)}
        </span>
    );
};

export default DocsLink;
