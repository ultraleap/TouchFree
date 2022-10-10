import { HandChirality, TouchFreeInputAction } from '../TouchFreeToolingTypes';
import { TouchlessCursor } from './TouchlessCursor';

const HAND_SPRITE_FRAMES = 16;

export class HandCursor extends TouchlessCursor {
    handImgs: HTMLImageElement[];
    cursor: HTMLElement;
    isActive: boolean;
    currentChirality: null | HandChirality;
    defaultTransform: string;
    blankUrl: string;

    constructor(handSpriteSheetUrl: string, BlankUrl: string) {
        super(undefined);
        this.handImgs = [];
        this.blankUrl = BlankUrl;

        for (let i = 0; i < HAND_SPRITE_FRAMES; i++) {
            this.handImgs[i] = this.constructHandImg(
                'relative',
                250,
                '1000',
                `url(${handSpriteSheetUrl}) 0 -${250 * i}px`,
                '0.35'
            );
        }
        this.cursor = document.createElement('div');
        this.cursor.style.position = 'absolute';
        this.cursor.classList.add('touchfreecursor');
        document.body.appendChild(this.cursor);

        this.isActive = true;
        this.currentChirality = null;
        this.defaultTransform = this.handImgs[0].style.transform;

        this.cursor.appendChild(this.handImgs[0]);

        this.ApplyToAll((hand: HTMLElement) => {
            this.cursor.appendChild(hand);
            hand.style.display = 'none';
        });

        this.handImgs[0].style.display = 'block';
    }

    HandleInputAction(_inputAction: TouchFreeInputAction) {
        if (!this.isActive) return;
        super.UpdateCursor(_inputAction);

        const mappedProg = parseInt((_inputAction.ProgressToClick * 15).toString());

        this.ShowHand(this.handImgs[mappedProg]);

        if (this.currentChirality !== _inputAction.Chirality) {
            this.currentChirality = _inputAction.Chirality;
            switch (_inputAction.Chirality) {
                case HandChirality.LEFT:
                    // Flip the img horizontally when left hand is being tracked
                    this.ApplyToAll((img: HTMLImageElement) => {
                        img.style.transform = `${this.defaultTransform} scaleX(-1)`;
                    });
                    break;
                case HandChirality.RIGHT:
                    this.ApplyToAll((img: HTMLImageElement) => {
                        img.style.transform = `${this.defaultTransform} scaleX(1)`;
                    });
                    break;
                default:
                    break;
            }
        }
    }

    ShowCursor() {
        this.ApplyToAll((hand: HTMLElement) => {
            hand.style.opacity = '1';
        });
        this.isActive = true;
    }

    HideCursor() {
        this.ApplyToAll((hand: HTMLElement) => {
            hand.style.opacity = '0';
        });
        this.isActive = false;
    }

    ApplyToAll(callback: Function) {
        this.handImgs.forEach((hand) => {
            callback(hand);
        });
    }

    ShowHand(hand: HTMLElement) {
        this.ApplyToAll((element: HTMLElement) => {
            element.style.display = 'none';
        });
        if (hand) {
            hand.style.display = 'block';
        }
    }

    constructHandImg(position: string, size: number, zIndex: string, background: string, scale: string) {
        let returnImage = document.createElement('img');

        returnImage.src = this.blankUrl;
        returnImage.style.position = position;
        returnImage.width = size;
        returnImage.height = size;
        returnImage.style.zIndex = zIndex;
        returnImage.style.background = background;
        returnImage.style.transform = `scale(${scale})`;

        // This style makes the images that make up the cursor ignore pointerevents and also
        // makes them invisible to the getElement(s)FromPoint, and as such is required by TouchFree
        // to ensure events are correctly sent to the elements _under_ the cursor.
        returnImage.style.pointerEvents = 'none';

        return returnImage;
    }
}
