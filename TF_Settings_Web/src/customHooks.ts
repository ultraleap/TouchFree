import { useEffect, useState } from 'react';

export const useWindowSize = () => {
    const [isFullScreen, setIsFullScreen] = useState(false);
    const [isZoomed, setIsZoomed] = useState(false);

    const resizeHandler = (event?: UIEvent) => {
        const target = event ? (event.currentTarget as Window) : null;

        const viewHeight = target?.innerHeight || window.innerHeight;
        const viewWidth = target?.innerWidth || window.innerWidth;
        const pixelRatio = target?.devicePixelRatio || window.devicePixelRatio;

        if (viewWidth * pixelRatio === screen.width && viewHeight * pixelRatio === screen.height) {
            setIsFullScreen(true);
        } else {
            setIsFullScreen(false);
        }

        if (pixelRatio != 1) {
            setIsZoomed(true);
        } else {
            setIsZoomed(false);
        }
    };

    useEffect(() => {
        resizeHandler();
        window.addEventListener('resize', resizeHandler);
        return () => window.removeEventListener('resize', resizeHandler);
    }, []);

    return { isFullScreen, isZoomed };
};
