import { ConnectionManager } from "../Connection/ConnectionManager";
import { SwipeDirection } from "../Connection/TouchFreeServiceTypes";
import { InputType, TouchFreeInputAction } from "../TouchFreeToolingTypes";
import { MapRangeToRange } from "../Utilities";
import { TouchlessCursor } from "./TouchlessCursor";

const MAX_SWIPE_NOTIFICATIONS = 1;
const TAIL_FADE_TIME_S = 1.5;
const TAIL_FADE_TIME_MS = TAIL_FADE_TIME_S * 1000;
const MIN_TAIL_LENGTH = 25;

export class SVGCursor extends TouchlessCursor {
    xPositionAttribute: string;
    yPositionAttribute: string;
    cursorCanvas: SVGSVGElement;
    cursorRing: SVGCircleElement;
    cursorTail: SVGPolygonElement;
    cursorPrompt: HTMLDivElement;
    cursorPromptWidth:number;
    ringSizeMultiplier: number;
    hidingCursor: boolean = false;
    currentFadingInterval: NodeJS.Timeout | undefined = undefined;
    swipeNotificationTimeout: NodeJS.Timeout | undefined = undefined;
    totalSwipeNotifications: number = 0;
    swipeDirection?: SwipeDirection;
    previousPosition?: number[];
    previousTime?: number;
    tailLength = [0,0];
    swipeTailTimeout?: NodeJS.Timeout;
    swipeTailInterval?: NodeJS.Timeout;
    drawNewTail = false;
    
    public allowCursorTail = false;
    public allowTextPrompt = true;

    constructor(_xPositionAttribute = "cx", _yPositionAttribute = "cy", _ringSizeMultiplier = 2, _darkCursor = false) {
        super(undefined);

        const documentBody = document.querySelector('body');

        const shadowColour = _darkCursor ? '#ffffffB3' : '#000000B3'

        const svgElement = document.createElementNS('http://www.w3.org/2000/svg','svg');
        svgElement.classList.add('touchfree-cursor');
        svgElement.style.opacity = '0';
        svgElement.style.position = 'absolute';
        svgElement.style.top = '0px';
        svgElement.style.left = '0px';
        svgElement.style.zIndex = '1000';
        svgElement.setAttribute('width', '100%');
        svgElement.setAttribute('height', '100%');
        svgElement.id = 'svg-cursor';
        svgElement.style.pointerEvents = 'none';
        documentBody?.appendChild(svgElement);

        const svgTailElement = document.createElementNS('http://www.w3.org/2000/svg', 'polygon');
        svgTailElement.classList.add('touchfree-cursor');
        svgTailElement.setAttribute('opacity', '1');
        svgTailElement.setAttribute('points', '0,0 0,0 0,0');
        svgTailElement.style.fill = _darkCursor ? 'black' : 'white';
        svgTailElement.style.filter = `drop-shadow(0 0 10px ${shadowColour})`;
        svgElement.appendChild(svgTailElement);
        this.cursorTail = svgTailElement;

        const svgRingElement = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
        svgRingElement.classList.add('touchfree-cursor');
        svgRingElement.setAttribute('r', '15');
        svgRingElement.setAttribute('fill-opacity', '0');
        svgRingElement.setAttribute('stroke-width', '5');
        svgRingElement.setAttribute('stroke', _darkCursor ? 'black' : 'white');
        svgRingElement.setAttribute('cx', '100');
        svgRingElement.setAttribute('cy', '100');
        svgRingElement.style.filter = `drop-shadow(0 0 10px ${shadowColour})`;
        svgElement.appendChild(svgRingElement);
        this.cursorRing = svgRingElement;

        const cursorPromptDiv = document.createElement('div');
        this.cursorPromptWidth = 300;
        Object.assign(cursorPromptDiv.style, {
            width: `${this.cursorPromptWidth}px`,
            height: '40px',
            position: 'absolute',
            left: '0',
            top: '0',
            'background-color': 'black',
            'border-radius': '50px',
            'border': '2px solid white',
            display: 'flex',
            opacity: 0,
            'pointer-events': 'none',
            color: 'white',
            'justify-content': 'center',
            'align-items': 'center',
            'font-size': '22px',
            'font-family': `'Trebuchet MS', sans-serif`,
            'transition': 'opacity 0.2s ease-out'
        });
        cursorPromptDiv.innerHTML = `To Scroll: <strong>Swipe Faster</strong>`
        documentBody?.appendChild(cursorPromptDiv);
        this.cursorPrompt = cursorPromptDiv;

        const svgDotElement = document.createElementNS('http://www.w3.org/2000/svg','circle');
        svgDotElement.classList.add('touchfree-cursor');
        svgDotElement.setAttribute('r', '15');
        svgDotElement.setAttribute('fill', _darkCursor ? 'black' : 'white');
        svgDotElement.setAttribute('cx', '100');
        svgDotElement.setAttribute('cy', '100');
        svgDotElement.setAttribute('opacity', '1');
        svgDotElement.style.transition = 'transform 200ms, opacity 666ms';
        svgDotElement.style.transformBox = 'fill-box';
        svgDotElement.style.transformOrigin = 'center';
        svgDotElement.style.transform = 'scale(1)';
        svgDotElement.style.filter = `drop-shadow(0 0 10px ${shadowColour})`;
        svgElement.appendChild(svgDotElement);

        this.cursor = svgDotElement;

        this.cursorCanvas = svgElement;
        this.xPositionAttribute = _xPositionAttribute;
        this.yPositionAttribute = _yPositionAttribute;

        this.ringSizeMultiplier = _ringSizeMultiplier;

        if (!_darkCursor) {
            this.cursorCanvas.classList.add('touchfree-cursor--light');
        }

        ConnectionManager.instance.addEventListener('HandFound', this.ShowCursor.bind(this));
        ConnectionManager.instance.addEventListener('HandsLost', this.HandleHandsLost.bind(this));
    }

    UpdateCursor (_inputAction: TouchFreeInputAction) {
        let ringScaler = MapRangeToRange(_inputAction.ProgressToClick, 0, 1, this.ringSizeMultiplier, 1);

        this.cursorRing.setAttribute('opacity', _inputAction.ProgressToClick.toString());
        this.cursorRing.setAttribute('r', (this.GetCurrentCursorRadius() * ringScaler).toString());

        const position = _inputAction.CursorPosition;
        const time = _inputAction.Timestamp;
        let tailPoints: string;
        
        if (this.previousPosition && this.previousTime && this.swipeDirection != undefined) {
            if (this.drawNewTail) {
                this.tailLength = [0,0];
                this.drawNewTail = false;
                this.cursorTail.setAttribute('opacity', '1');
            }

            let timeModifier = (time - this.previousTime) / 50000;
            timeModifier = timeModifier > 0 ? timeModifier : 1;

            const newTailLength = [
                Math.round(Math.abs(position[0] - this.previousPosition[0]) / timeModifier),
                Math.round(Math.abs(position[1] - this.previousPosition[1]) / timeModifier),
            ];

            if (
                newTailLength[0] > this.tailLength[0] && newTailLength[0] > MIN_TAIL_LENGTH ||
                newTailLength[1] > this.tailLength[1] && newTailLength[1] > MIN_TAIL_LENGTH
            ) {
                this.tailLength = newTailLength;

                clearTimeout(this.swipeTailTimeout);
                clearInterval(this.swipeTailInterval);

                this.swipeTailInterval = setInterval(() => {
                    const currentOpacity = this.cursorTail.getAttribute('opacity');
                    if (currentOpacity) {
                        let opacity = parseFloat(currentOpacity);
                        const modifier = 0.01 * (opacity + 0.5);
                        opacity -= modifier;
                        opacity = opacity || 0;
                        this.cursorTail.setAttribute('opacity', opacity.toString());
                    }
                }, TAIL_FADE_TIME_MS / 100);

                this.swipeTailTimeout = setTimeout(() => {
                    clearInterval(this.swipeTailInterval);
                    this.drawNewTail = true;
                }, TAIL_FADE_TIME_MS);
            }
        } else {
            this.tailLength = [0,0];
        }

        switch (this.swipeDirection) {
            case SwipeDirection.LEFT:
                tailPoints = `${position[0]},${position[1] - 15} ${position[0]},${position[1] + 15} ${position[0] + this.tailLength[0]},${position[1]}`
                break;
            case SwipeDirection.RIGHT:
                tailPoints = `${position[0]},${position[1] - 15} ${position[0]},${position[1] + 15} ${position[0] - this.tailLength[0]},${position[1]}`
                break;
            case SwipeDirection.UP:
                tailPoints = `${position[0] - 15},${position[1]} ${position[0] + 15},${position[1]} ${position[0]},${position[1] + this.tailLength[1]}`
                break;
            case SwipeDirection.DOWN:
                tailPoints = `${position[0] - 15},${position[1]} ${position[0] + 15},${position[1]} ${position[0]},${position[1] - this.tailLength[1]}`
                break;
            default:
                tailPoints = `${position[0]},${position[1]} ${position[0]},${position[1]} ${position[0]},${position[1]}`;
        }
            
        if (position) {
            this.ShowCursor();
            this.cursorRing.setAttribute(this.xPositionAttribute, position[0].toString());
            this.cursorRing.setAttribute(this.yPositionAttribute, position[1].toString());
                
            if(this.allowCursorTail){
                this.cursorTail.setAttribute("points", tailPoints);
            }

            this.cursorPrompt.style.left = `${position[0] - this.cursorPromptWidth/2}px`;
            this.cursorPrompt.style.top = `${position[1] - 80}px`;
    
            if (this.cursor) {
                this.cursor.setAttribute(this.xPositionAttribute, position[0].toString());
                this.cursor.setAttribute(this.yPositionAttribute, position[1].toString());
            }
        } else {
            this.HideCursor();
        }

        this.previousPosition = position;
        this.previousTime = time;
    }

    HandleInputAction(_inputData: TouchFreeInputAction) {
        if (this.cursor) {
            switch (_inputData.InputType) {
                case InputType.MOVE:
                    this.UpdateCursor(_inputData);
                    break;
                case InputType.DOWN:
                    this.SetCursorSize(0, this.cursorRing);
                    this.cursor.style.transform = 'scale(0.5)';
                    break;
                case InputType.UP:
                    this.cursor.style.transform = 'scale(1)';
                    break;

                case InputType.CANCEL:
                    break;
            }
        }
    }

    SetCursorSize(_newWidth: number, _cursorToChange: SVGElement) {
        _cursorToChange?.setAttribute('r', _newWidth.toString());
    }

    ShowCursor() {
        this.hidingCursor = false;
        this.cursorCanvas.style.opacity = '1';
    }

    HideCursor() {
        this.hidingCursor = true;
        this.cursorCanvas.style.opacity = '0';
        if (this.cursor) {
            this.cursor.style.transform = 'scale(1)';
        }
    }

    GetCurrentCursorRadius(): number {
        if (this.cursor) {
            const radius = this.cursor.getAttribute('r');
            if (!radius) {
                return 0;
            }

            let radiusAsNumber = parseFloat(radius);

            return radiusAsNumber;
        }
        return 0;
    }

    ShowCloseToSwipe(): void {
        if(!this.allowTextPrompt) return;
        if (this.totalSwipeNotifications >= MAX_SWIPE_NOTIFICATIONS || this.swipeNotificationTimeout ) {
            return;
        }

        this.cursorPrompt.style.opacity = '1';
        
        // this.totalSwipeNotifications++;

        this.swipeNotificationTimeout = setTimeout(() => {
            this.HideCloseToSwipe();
        }, 2000);
    }

    HandleHandsLost(): void {
        this.totalSwipeNotifications = 0;
        this.HideCursor();
        this.HideCloseToSwipe();
    }

    HideCloseToSwipe(): void {
        this.cursorPrompt.style.opacity = '0';
        clearTimeout(this.swipeNotificationTimeout);
        this.swipeNotificationTimeout = undefined;
    }

    SetSwipeDirection = (direction?: SwipeDirection) => {
        this.swipeDirection = direction;
        this.drawNewTail = true;
        this.tailLength = [0,0];
    }
}