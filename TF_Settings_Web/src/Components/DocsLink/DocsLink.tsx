import './DocsLink.scss';

import { QRCodeSVG } from 'qrcode.react';
import React, { CSSProperties, useState } from 'react';

import { BaseTFButton } from '@/Components';
import { BaseButtonProps } from '@/Components/TFButton/TFButtons';

interface DocsLinkProps {
    buttonStyle?: CSSProperties;
    title: string;
    titleStyle?: CSSProperties;
    link: string;
}

const DocsLink: React.FC<DocsLinkProps> = ({ buttonStyle, title, titleStyle, link }) => {
    const [showModal, setShowModal] = useState<boolean>(true);

    const onClick = () => {
        setShowModal(!showModal);
        console.log(link);
    };

    return (
        <>
            {showModal && <DocsModal link={link} />}
            <DocsButton buttonStyle={buttonStyle ?? {}} title={title} titleStyle={titleStyle ?? {}} onClick={onClick} />
        </>
    );
};

const DocsModal: React.FC<{ link: string }> = ({ link }) => {
    return (
        <>
            <div className="docs-modal--cover" />
            <div className="docs-modal--container">
                <div className="qr-code--container">
                    <QRCodeSVG value={link} style={{ width: '100%', height: '100%' }} />
                </div>
            </div>
            ;
        </>
    );
};

const DocsButton: React.FC<BaseButtonProps> = ({ buttonStyle, title, titleStyle, onClick }) => {
    const buttonContent = <h1 style={titleStyle}>{title}</h1>;

    return (
        <span id="docs-button--container">
            {BaseTFButton('docs-button', true, buttonStyle, onClick, buttonContent)}
        </span>
    );
};

export default DocsLink;
