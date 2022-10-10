import { ConfigurationManager } from '../Configuration/ConfigurationManager';
import { ConnectionManager } from '../Connection/ConnectionManager';
import { InteractionType } from '../TouchFreeToolingTypes';
import { HandCursor } from './HandCursor';
import { SVGCursor } from './SvgCursor';

interface HandImages {
    spriteSheetUrl: string;
    blankUrl: string;
}

type Cursor = 'hand' | 'svg';

export class CursorManager {
    handCursor: HandCursor;
    svgCursor: SVGCursor;
    activeCursor: Cursor = 'svg';
    connected = false;

    constructor(handImages: HandImages) {
        const { spriteSheetUrl, blankUrl } = handImages;
        this.handCursor = new HandCursor(spriteSheetUrl, blankUrl);
        this.svgCursor = new SVGCursor();

        this.handCursor.HideCursor();

        ConnectionManager.AddConnectionListener(() => {
            this.connected = true;
            switch (this.activeCursor) {
                case 'hand':
                    this.EnableGrab();
                    break;
                case 'svg':
                    this.DisableGrab();
                    break;
            }
        });
    }

    private HideAllCursors = () => {
        this.handCursor.DisableCursor();
        this.svgCursor.DisableCursor();
    }

    private EnableGrab = () => {
        if (!this.connected) return;
        ConfigurationManager.RequestConfigChange(
            {
              InteractionType: InteractionType.GRAB,
              UseScrollingOrDragging: true,
            },
            null,
            () => {}
          );
    }

    private DisableGrab = () => {
        if (!this.connected) return;
        ConfigurationManager.RequestConfigFileState((configState) => {
            ConfigurationManager.RequestConfigChange(configState.interaction, configState.physical, () => {})
        });
    }

    SetActiveCursor = (cursor: Cursor) => {
        this.activeCursor = cursor;
        this.HideAllCursors();
        switch (cursor) {
            case 'hand':
                this.EnableGrab();
                this.handCursor.EnableCursor();
                break;
            case 'svg':
                this.DisableGrab();
                this.svgCursor.EnableCursor();
                break;
            default:
                break;
        }
    }
}
