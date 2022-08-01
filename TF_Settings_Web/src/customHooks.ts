import { useEffect, useState } from 'react';

export const useIsFullScreen = () => {
    const [isFullScreen, setIsFullScreen] = useState(false);

    const resizeHandler = () => {
        if (window.innerWidth === screen.width && window.innerHeight === screen.height) {
            setIsFullScreen(true);
            return;
        }

        setIsFullScreen(false);
    };

    useEffect(() => {
        resizeHandler();
        window.addEventListener('resize', resizeHandler);
        return () => window.removeEventListener('resize', resizeHandler);
    }, []);

    return isFullScreen;
};
