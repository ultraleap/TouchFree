import styles from './TFButtons.module.scss';

import classnames from 'classnames/bind';
import React, { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';

import { BackArrow } from '@/Images';

const classes = classnames.bind(styles);

export type TFClickEvent = React.PointerEvent<HTMLButtonElement> | React.KeyboardEvent<HTMLButtonElement>;

export const tFClickIsPointer = (event: TFClickEvent): event is React.PointerEvent<HTMLButtonElement> =>
    event.type.includes('pointer');

interface BaseButtonProps {
    className?: string;
    buttonStyle?: React.CSSProperties;
    titleStyle?: React.CSSProperties;
    onClick: (event: TFClickEvent) => void;
    canHover?: boolean;
}

interface TextButtonProps extends BaseButtonProps {
    text?: string;
    title?: string;
    textStyle?: React.CSSProperties;
}

const getButtonTextElements = (
    title?: string,
    titleStyle?: React.CSSProperties,
    text?: string,
    textStyle?: React.CSSProperties
): JSX.Element[] => {
    const elements: JSX.Element[] = [];
    if (title) {
        elements.push(
            <h1 key={title} style={titleStyle}>
                {title}
            </h1>
        );
    }

    if (text) {
        elements.push(
            <p key={text} style={textStyle}>
                {text}
            </p>
        );
    }

    return elements;
};

export const TextButton: React.FC<TextButtonProps> = ({
    className,
    buttonStyle,
    title,
    titleStyle,
    text,
    textStyle,
    onClick,
    canHover = true,
}) => (
    <BaseTFButton buttonClass={className} canHover={canHover} onClick={onClick} buttonStyle={buttonStyle} type="text">
        {getButtonTextElements(title, titleStyle, text, textStyle)}
    </BaseTFButton>
);

interface IconTextButtonProps extends TextButtonProps {
    icon: string;
    alt: string;
    title: string;
    iconStyle?: React.CSSProperties;
}

export const VerticalIconTextButton: React.FC<IconTextButtonProps> = ({
    className,
    buttonStyle,
    icon,
    alt,
    iconStyle,
    title,
    titleStyle,
    text,
    textStyle,
    onClick,
    canHover = true,
}) => (
    <BaseTFButton
        buttonClass={className}
        canHover={canHover}
        onClick={onClick}
        buttonStyle={buttonStyle}
        type="vertical"
    >
        <img style={iconStyle} src={icon} alt={alt} />
        {getButtonTextElements(title, titleStyle, text, textStyle)}
    </BaseTFButton>
);

export const HorizontalIconTextButton: React.FC<IconTextButtonProps> = ({
    className,
    buttonStyle,
    icon,
    alt,
    iconStyle,
    title,
    titleStyle,
    text,
    textStyle,
    onClick,
    canHover = true,
}) => (
    <BaseTFButton
        buttonClass={className}
        canHover={canHover}
        onClick={onClick}
        buttonStyle={buttonStyle}
        type="horizontal"
    >
        <div className={classes('tf-button-text-container')}>
            {getButtonTextElements(title, titleStyle, text, textStyle)}
        </div>
        <img style={iconStyle} src={icon} alt={alt} />
    </BaseTFButton>
);

export const BackButton = () => {
    const nav = useNavigate();
    return (
        <BaseTFButton canHover onClick={() => nav('/')} type="back">
            <div className={classes('back-container')}>
                <img src={BackArrow} alt="Arrow pointing back" />
                <h1>Back</h1>
            </div>
        </BaseTFButton>
    );
};

interface OutlinedTextButtonProps extends BaseButtonProps {
    title: string;
}

export const OutlinedTextButton: React.FC<OutlinedTextButtonProps> = ({
    buttonStyle,
    title,
    titleStyle,
    onClick,
    className,
}) => (
    <BaseTFButton className={className} canHover onClick={onClick} buttonStyle={buttonStyle} type="misc">
        <h1 style={titleStyle}>{title}</h1>
    </BaseTFButton>
);

interface BaseTFButtonProps extends BaseButtonProps {
    type: 'horizontal' | 'vertical' | 'text' | 'misc' | 'back';
    buttonClass?: string;
    children?: ReactNode;
}

export const BaseTFButton: React.FC<BaseTFButtonProps> = ({
    type,
    buttonClass,
    canHover,
    onClick,
    buttonStyle,
    children,
}) => {
    const [hovered, setHovered] = React.useState<boolean>(false);
    const [pressed, setPressed] = React.useState<boolean>(false);

    return (
        <button
            className={classes(buttonClass, 'tf-button', `tf-button--${type}`, {
                'tf-button--hovered': canHover && hovered,
                [`tf-button--${type}--hovered`]: canHover && hovered,
                'tf-button--pressed': pressed,
            })}
            style={buttonStyle}
            onPointerOver={() => setHovered(true)}
            onPointerLeave={() => {
                setHovered(false);
                setPressed(false);
            }}
            onPointerDown={() => {
                setPressed(true);
            }}
            onPointerUp={(event) => {
                if (pressed) {
                    onClick(event);
                }
                setPressed(false);
            }}
            onKeyDown={(keyEvent) => {
                if (keyEvent.key === 'Enter') onClick(keyEvent);
            }}
        >
            {children}
        </button>
    );
};
