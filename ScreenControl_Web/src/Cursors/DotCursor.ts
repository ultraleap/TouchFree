import {
    ClientInputAction, InputType
} from '../ScreenControlTypes';
import { ConnectionManager } from '../Connection/ConnectionManager';
import { TouchlessCursor } from './TouchlessCursor';

 //Class: DotCursor
 //This is an example Touchless Cursor which positions a dot on the screen at the hand location,
 //and reacts to the current ProgressToClick of the action (what determines this depends on the
 //currently active interaction).
export class DotCursor extends TouchlessCursor{
    // Group: Variables

    // Variable: cursorDotSize
    // The size of the dot when it isn't being shrunk.
    cursorDotSize: number = 80;

    // Variable: cursorRing
    // The HTMLElement that visually represents the cursors ring.
    cursorRing: HTMLElement;

    // Variable: cursorMaxRingSize
    // The maximum size for the ring to be relative to the size of the dot.
    //
    // e.g. a value of 2 means the ring can be (at largest) twice the scale of the dot.
    cursorMaxRingSize: number = 2;

    // Variable: pulseShrinkCurve
    // When a "click" is recognised, an animation plays where the dot "pulses" (briefly
    // shrinking and expanding). This AnimationCurve governs the shrinking that follows a
    // "DOWN" input.
//    public AnimationCurve pulseShrinkCurve;

    // Variable: pulseGrowCurve
    // When a "click" is recognised, an animation plays where the dot "pulses" (briefly
    // shrinking and expanding). This AnimationCurve governs the growing back to full size
    // that follows a "UP" input.
//    public AnimationCurve pulseGrowCurve;

    // Variable: pulseSeconds
    // The amount of time each part of the pulse should last (a pulse will last twice this long)
//    [Range(0.01f, 1f)] public float pulseSeconds;

    // Variable: cursorDownScale
    // The scale of the cursor's dot at its smallest size (between the shrink and the grow )
//    [Range(0.01f, 2f)] public float cursorDownScale;


    //protected float maxRingScale;
    //protected bool hidingCursor = false;
    //protected Vector3 cursorLocalScale = Vector3.one;

    constructor(_cursor: HTMLElement, _cursorRing: HTMLElement) {
        super(_cursor);
        this.cursorRing = _cursorRing;
    }

    // Group: Functions

    // Function: UpdateCursor
    // Used to update the cursor when recieving a "MOVE" <ClientInputAction>. Updates the
    // cursor's position, as well as the size of the ring based on the current ProgressToClick.
    UpdateCursor(_inputAction: Array<number> | ClientInputAction): void{
        var inputAction = _inputAction as ClientInputAction;

        //progressToClick is between 0 and 1. Click triggered at progressToClick = 1
        var dist = (1 - inputAction.ProgressToClick);

        this.cursorRing.style.width = this.cursor.clientWidth * (this.cursorMaxRingSize * dist) + "px";
        this.cursorRing.style.height = this.cursor.clientHeight * (this.cursorMaxRingSize * dist) + "px";

        this.cursorRing.style.left = (inputAction.CursorPosition[0] - (this.cursorRing.clientWidth / 2)) + "px";
        this.cursorRing.style.top = (window.innerHeight - (inputAction.CursorPosition[1] + (this.cursorRing.clientHeight / 2))) + "px";

        super.UpdateCursor(inputAction.CursorPosition);
    }

    // Group: TouchlessCursor Overrides

    // Function: HandleInputAction
    // This override replaces the basic functionality of the <TouchlessCursor>, making the
    // cursor's ring scale dynamically with the current ProgressToClick and creating a small
    // "shrink" animation when a "DOWN" event is recieved, and a "grow" animation when an "UP"
    // is recieved.
    HandleInputAction(_inputData: ClientInputAction): void {

        switch (_inputData.InputType) {
            case InputType.MOVE:
                this.UpdateCursor(_inputData);
                break;
            case InputType.DOWN:
                //growQueued = false;
                //if (cursorScalingRoutine != null) {
                //    StopCoroutine(cursorScalingRoutine);
                //}
                //cursorScalingRoutine = StartCoroutine(ShrinkCursorDot());
                break;

            case InputType.UP:
                //growQueued = true;
                break;

            case InputType.CANCEL:
                break;
        }

    }

    //    // Function: Update
    //    // This override runs the basic functionality of <TouchlessCursor> and also ensures that if
    //    // the cursor has a <growQueued> and has the ability to, it should start the "grow"
    //    // animation.
    //    protected override void Update()
    // {
    //    base.Update();

    //    if (growQueued && cursorScalingRoutine == null) {
    //        growQueued = false;
    //        cursorScalingRoutine = StartCoroutine(GrowCursorDot());
    //    }
    //}

    //    // Function: InitialiseCursor
    //    // This override ensures that the DotCursor is properly set up with relative scales and
    //    // sorting orders for the ring sprites.
    //    protected override void InitialiseCursor()
    //{
    //    dotFillColor = cursorFill.color;
    //    dotBorderColor = cursorBorder.color;

    //    if (ringEnabled) {
    //        ringColor = ringOuterSprite.color;
    //    }

    //    bool dotSizeIsZero = Mathf.Approximately(cursorDotSize, 0f);
    //    cursorDotSize = dotSizeIsZero ? 1f: cursorDotSize;
    //    cursorBorder.transform.localScale = new Vector3(cursorDotSize, cursorDotSize, cursorDotSize);
    //    SetCursorLocalScale(cursorDotSize);

    //    if (ringEnabled) {
    //        maxRingScale = (1f / cursorDotSize) * cursorMaxRingSize;

    //        // This is a crude way of forcing the sprites to draw on top of the UI, without masking it.
    //        ringOuterSprite.sortingOrder = ringSpriteSortingOrder;
    //        ringMask.GetComponent<SpriteMask>().isCustomRangeActive = true;
    //        ringMask.GetComponent<SpriteMask>().frontSortingOrder = ringSpriteSortingOrder + 1;
    //        ringMask.GetComponent<SpriteMask>().backSortingOrder = ringSpriteSortingOrder - 1;
    //    }
    //}

    //    // Function: ShowCursor
    //    // This override replaces the basic functionality of the <TouchlessCursor> to ensure that the cursor is faded smoothly
    //    // when being set to show.
    //    public override void ShowCursor()
    //{
    //    bool wasShowing = !hidingCursor;
    //    hidingCursor = false;

    //    if (wasShowing) {
    //        return;
    //    }

    //    ResetCursor();

    //    fadeRoutine = StartCoroutine(FadeCursor(0, 1, fadeDuration));
    //    cursorBorder.enabled = true;
    //    cursorFill.enabled = true;

    //    if (ringEnabled) {
    //        ringOuterSprite.enabled = true;
    //        ringOuterSprite.color = new Color(ringColor.r, ringColor.g, ringColor.b, 0);
    //    }
    //}

    //    // Function: HideCursor
    //    // This override replaces the basic functionality of the <TouchlessCursor> to ensure that the cursor is faded smoothly
    //    // when being set to hide.
    //    public override void HideCursor()
    //{
    //    bool wasHiding = hidingCursor;
    //    hidingCursor = true;

    //    if (wasHiding) {
    //        return;
    //    }

    //    ResetCursor();

    //    fadeRoutine = StartCoroutine(FadeCursor(1, 0, fadeDuration, true));

    //    if (ringEnabled) {
    //        ringOuterSprite.color = new Color(ringColor.r, ringColor.g, ringColor.b, 0);
    //    }
    //}

    //    // Group: Coroutine Functions

    //    // Function: GrowCursorDot
    //    // This coroutine smoothly expands the cursor dots size.
    //    public IEnumerator GrowCursorDot()
    //{
    //    SetCursorLocalScale(cursorDownScale * cursorDotSize);

    //    YieldInstruction yieldInstruction = new YieldInstruction();
    //    float elapsedTime = 0.0f;

    //    while (elapsedTime < pulseSeconds) {
    //        yield return yieldInstruction;
    //        elapsedTime += Time.deltaTime;

    //        float scale = Utilities.MapRangeToRange(pulseGrowCurve.Evaluate(elapsedTime / pulseSeconds), 0, 1, cursorDownScale * cursorDotSize, cursorDotSize);
    //        SetCursorLocalScale(scale);
    //    }

    //    SetCursorLocalScale(cursorDotSize);
    //    cursorScalingRoutine = null;
    // }

    //    // Function: ShrinkCursorDot
    //    // This coroutine smoothly contracts the cursor dots size.
    //    public IEnumerator ShrinkCursorDot()
    //{
    //    YieldInstruction yieldInstruction = new YieldInstruction();
    //    float elapsedTime = 0.0f;

    //    while (elapsedTime < pulseSeconds) {
    //        yield return yieldInstruction;
    //        elapsedTime += Time.deltaTime;

    //        float scale = Utilities.MapRangeToRange(pulseShrinkCurve.Evaluate(elapsedTime / pulseSeconds), 0, 1, cursorDownScale * cursorDotSize, cursorDotSize);
    //        SetCursorLocalScale(scale);
    //    }

    //    SetCursorLocalScale(cursorDownScale * cursorDotSize);
    //    cursorScalingRoutine = null;
    //}

    //    // Function: FadeCursor
    //    // This coroutine smoothly fades the cursors colours.
    //    private IEnumerator FadeCursor(float _from, float _to, float _duration, bool _disableOnEnd = false)
    //{
    //    for (int i = 0; i < _duration; i++)
    //    {
    //        yield return null;
    //        float a = Mathf.Lerp(_from, _to, i / _duration);

    //        cursorBorder.color = new Color()
    //        {
    //            r = cursorBorder.color.r,
    //                g = cursorBorder.color.g,
    //                b = cursorBorder.color.b,
    //                a = a * dotBorderColor.a
    //        };

    //        cursorFill.color = new Color()
    //        {
    //            r = cursorFill.color.r,
    //                g = cursorFill.color.g,
    //                b = cursorFill.color.b,
    //                a = a * dotFillColor.a
    //        };
    //    };

    //    if (_disableOnEnd) {
    //        SetCursorLocalScale(cursorDotSize);

    //        cursorBorder.enabled = false;
    //        cursorFill.enabled = false;

    //        if (ringEnabled) {
    //            ringOuterSprite.enabled = false;
    //        }
    //    }

    //    fadeRoutine = null;
    //}

    //    // Function: SetCursorLocalScale
    //    // This function resizes the cursor and its border.
    //    private void SetCursorLocalScale(float _scale)
    //{
    //    cursorLocalScale = new Vector3(_scale, _scale, _scale);
    //    cursorBorder.transform.localScale = cursorLocalScale;
    //}

    //    // Function: ResetCursor
    //    // This function stops all scaling coroutines and clears their related variables.
    //    public void ResetCursor()
    //{
    //    StopAllCoroutines();
    //    fadeRoutine = null;
    //    cursorScalingRoutine = null;
    //    growQueued = false;

    //    SetCursorLocalScale(cursorDotSize);
    //}
}