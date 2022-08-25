import { useEffect, useRef, useState } from 'react';

export const useIsFullScreen = () => {
    const [isFullScreen, setIsFullScreen] = useState(false);

    const resizeHandler = (event?: UIEvent) => {
        const target = event ? (event.currentTarget as Window) : null;

        const viewHeight = target?.innerHeight || window.innerHeight;
        const viewWidth = target?.innerWidth || window.innerWidth;

        if (viewWidth === screen.width && viewHeight === screen.height) {
            setIsFullScreen(true);
        } else {
            setIsFullScreen(false);
        }
    };

    useEffect(() => {
        resizeHandler();
        window.addEventListener('resize', resizeHandler);
        return () => window.removeEventListener('resize', resizeHandler);
    }, []);

    return isFullScreen;
};

export const useStatefulRef = function <T>(initialValue: T): {
    current: T;
} {
    const [currentVal, setCurrentVal] = useState<T>(initialValue);
    const valRef = useRef<T>(currentVal);

    const statefulRef = {
        get current() {
            return valRef.current;
        },
        set current(value: T) {
            valRef.current = value;
            setCurrentVal(value);
        },
    };

    return statefulRef;
};
