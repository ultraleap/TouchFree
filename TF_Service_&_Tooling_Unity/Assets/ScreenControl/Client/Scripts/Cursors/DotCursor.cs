using System.Collections;
using UnityEngine;
using Ultraleap.ScreenControl.Client.Connection;

namespace Ultraleap.ScreenControl.Client.Cursors
{
    // Class: DotCursor
    // This is an example Touchless Cursor which positions a dot on the screen at the hand location,
    // and reacts to the current ProgressToClick of the action (what determines this depends on the
    //  currently active interaction).
    //
    // We recommend using the "RingCursor" Prefab which is set up for use with this class.
    public class DotCursor : TouchlessCursor
    {
        // Group: Variables

        // Variable: fadeDuration
        // The number of frames over which the cursor should fade when appearing/disappearing
        [Range(0f, 60f)] public float fadeDuration = 30;

        // Variable: cursorBorder
        // The image of the border around the dot, this is the parent image in the prefab and is
        //  used to do all of the scaling of the images that make up this cursor.
        [Header("Graphics")]
        public UnityEngine.UI.Image cursorBorder;

        // Variable: cursorFill
        // The main background image of the Cursor, used for fading the image out.
        public UnityEngine.UI.Image cursorFill;

        // Variable: ringOuterSprite
        // This refers to the Ring around the central cursor that is used to display the "reactive"
        // state of the cursor; the closer the ring is to the dot, the closer you are to "clicking".
        //
        // The sprite is accessed to fade in/out the ring as it is needed.
        public SpriteRenderer ringOuterSprite;

        // Variable: ringEnabled
        // Enables/disables the ring cursor around the dot. Here primarily for use in the inspector.
        [Header("Ring")]

        // Variable: cursorMaxRingSize
        // The maximum size for the ring to be relative to the size of the dot.
        //
        //  e.g. a value of 2 means the ring can be (at largest) twice the scale of the dot.
        public float cursorMaxRingSize = 8;

        // Variable: ringCurve
        // This curve is used to determine how the ring's scale changes with the value of the latest
        // InputAction's ProgressToClick.
        public AnimationCurve ringCurve;

        // Variable: ringOuter
        // This refers to the Ring around the central cursor that is used to display the "reactive"
        // state of the cursor; the closer the ring is to the dot, the closer you are to "clicking"
        public RectTransform ringOuter;

        // Variable: ringMask
        // This is a reference to the mask that is used to make the center of the ring visual
        // transparent. It has to be scaled to match the ring itself.
        public RectTransform ringMask;

        // Variable: ringThickness
        // Used to set the thickness of the ring itself (i.e. the distance between the inner and
        // outer edges of the ring)
        public float ringThickness = 2;

        // Variable: pulseShrinkCurve
        // When a "click" is recognised, an animation plays where the dot "pulses" (briefly
        // shrinking and expanding). This AnimationCurve governs the shrinking that follows a
        // "DOWN" input.
        [Header("Pulse")]
        public AnimationCurve pulseShrinkCurve;

        // Variable: pulseGrowCurve
        // When a "click" is recognised, an animation plays where the dot "pulses" (briefly
        // shrinking and expanding). This AnimationCurve governs the growing back to full size
        // that follows a "UP" input.
        public AnimationCurve pulseGrowCurve;

        // Variable: pulseSeconds
        // The amount of time each part of the pulse should last (a pulse will last twice this long)
        [Range(0.01f, 1f)] public float pulseSeconds;

        // Variable: cursorDownScale
        // The scale of the cursor's dot at its smallest size (between the shrink and the grow )
        [Range(0.01f, 2f)] public float cursorDownScale;

        Coroutine cursorScalingRoutine;

        protected float maxRingScale;

        protected bool hidingCursor = true;
        protected bool growQueued = false;
        protected Vector3 cursorLocalScale = Vector3.one;
        protected const int ringSpriteSortingOrder = 32766;

        // Group: Functions

        // Function: UpdateCursor
        // Used to update the cursor when recieving a "MOVE" <ClientInputAction>. Updates the
        // cursor's position, as well as the size of the ring based on the current ProgressToClick.
        public virtual void UpdateCursor(Vector2 _screenPos, float _progressToClick)
        {
            targetPos = _screenPos;

            if (hidingCursor)
            {
                ringOuterSprite.color = new Color(secondaryColor.r, secondaryColor.g, secondaryColor.b, 0);
            }
            else
            {
                //progressToClick is between 0 and 1. Click triggered at progressToClick = 1
                _progressToClick = Mathf.Clamp01(1f - _progressToClick);

                ScaleRing(_progressToClick);
            }
        }

        void ScaleRing(float _progressToClick)
        {
            // 0.8f so that the boundary between ring and dot is not visible.
            float minRingScale = 0.8f;
            float ringScale = Mathf.Lerp(minRingScale, maxRingScale, ringCurve.Evaluate(_progressToClick));
            ringOuterSprite.color = new Color(secondaryColor.r, secondaryColor.g, secondaryColor.b, Mathf.Lerp(1f, 0f, _progressToClick));

            ringOuter.transform.localScale = Vector3.one * ringScale;

            ringMask.transform.localScale = new Vector3()
            {
                x = Mathf.Max(0, ringOuter.localScale.x - ringThickness),
                y = Mathf.Max(0, ringOuter.localScale.y - ringThickness),
                z = ringOuter.localScale.z
            };
        }

        // Group: TouchlessCursor Overrides

        // Function: HandleInputAction
        // This override replaces the basic functionality of the <TouchlessCursor>, making the
        // cursor's ring scale dynamically with the current ProgressToClick and creating a small
        // "shrink" animation when a "DOWN" event is recieved, and a "grow" animation when an "UP"
        // is recieved.
        protected override void HandleInputAction(ClientInputAction _inputData)
        {
            switch (_inputData.InputType)
            {
                case InputType.MOVE:
                    UpdateCursor(_inputData.CursorPosition, _inputData.ProgressToClick);
                    break;

                case InputType.DOWN:
                    growQueued = false;
                    if (cursorScalingRoutine != null)
                    {
                        StopCoroutine(cursorScalingRoutine);
                    }
                    cursorScalingRoutine = StartCoroutine(ShrinkCursorDot());
                    break;

                case InputType.UP:
                    if (cursorScalingRoutine != null)
                    {
                        growQueued = true;
                    }
                    else
                    {
                        growQueued = false;
                        cursorScalingRoutine = StartCoroutine(GrowCursorDot());
                    }
                    break;

                case InputType.CANCEL:
                    HideCursor();
                    break;
            }

            if (hidingCursor && _inputData.InputType != InputType.CANCEL)
            {
                ShowCursor();
            }
        }

        // Function: SetColors
        // This override ensures the correct cursor UI elemets are coloured correctly when new
        // colours are set.
        public override void SetColors(Color _primary, Color _secondary, Color _tertiary)
        {
            _primaryColor = _primary;
            _secondaryColor = _secondary;
            _tertiaryColor = _tertiary;

            cursorFill.color = new Color(primaryColor.r, primaryColor.g, primaryColor.b, cursorFill.color.a);
            ringOuterSprite.color = new Color(secondaryColor.r, secondaryColor.g, secondaryColor.b, ringOuterSprite.color.a);
            cursorBorder.color = new Color(tertiaryColor.r, tertiaryColor.g, tertiaryColor.b, cursorBorder.color.a);
        }

        // Function: OnDisable
        // This override of Unity's OnDisable feature of MonoBehaviour does the teardown of this
        // Cursor when it is disabled.
        protected override void OnDisable()
        {
            base.OnDisable();

            ConnectionManager.HandFound -= ShowCursor;
            ConnectionManager.HandsLost -= HideCursor;
        }

        // Function: InitialiseCursor
        // This override ensures that the DotCursor is properly set up with relative scales and
        // sorting orders for the ring sprites.
        protected override void InitialiseCursor()
        {
            ConnectionManager.HandFound += ShowCursor;
            ConnectionManager.HandsLost += HideCursor;

            bool dotSizeIsZero = Mathf.Approximately(cursorSize, 0f);
            cursorSize = dotSizeIsZero ? 1f : cursorSize;
            cursorBorder.transform.localScale = new Vector3(cursorSize, cursorSize, cursorSize);
            SetCursorLocalScale(cursorSize);

            SetColors(primaryColor, secondaryColor, tertiaryColor);

            maxRingScale = (1f / cursorSize) * cursorMaxRingSize;

            // This is a crude way of forcing the sprites to draw on top of the UI, without masking it.
            ringOuterSprite.sortingOrder = ringSpriteSortingOrder;
            ringMask.GetComponent<SpriteMask>().isCustomRangeActive = true;
            ringMask.GetComponent<SpriteMask>().frontSortingOrder = ringSpriteSortingOrder + 1;
            ringMask.GetComponent<SpriteMask>().backSortingOrder = ringSpriteSortingOrder - 1;
        }

        // Function: ShowCursor
        // This override replaces the basic functionality of the <TouchlessCursor> to ensure that the cursor is faded smoothly
        // when being set to show.
        public override void ShowCursor()
        {
            bool wasShowing = !hidingCursor;
            hidingCursor = false;

            if (wasShowing)
            {
                return;
            }

            ResetCursor();
            StartCoroutine(FadeCursor(0, 1, fadeDuration));
            cursorBorder.enabled = true;
            cursorFill.enabled = true;

            ringOuterSprite.enabled = true;
            ringOuterSprite.color = new Color(secondaryColor.r, secondaryColor.g, secondaryColor.b, 0);
        }

        // Function: HideCursor
        // This override replaces the basic functionality of the <TouchlessCursor> to ensure that the cursor is faded smoothly
        // when being set to hide.
        public override void HideCursor()
        {
            bool wasHiding = hidingCursor;
            hidingCursor = true;

            if (wasHiding)
            {
                return;
            }

            ResetCursor();
            StartCoroutine(FadeCursor(1, 0, fadeDuration, true));

            ringOuterSprite.color = new Color(secondaryColor.r, secondaryColor.g, secondaryColor.b, 0);
        }

        // Group: Coroutine Functions

        // Function: GrowCursorDot
        // This coroutine smoothly expands the cursor dots size.
        public virtual IEnumerator GrowCursorDot()
        {
            SetCursorLocalScale(cursorDownScale * cursorSize);

            YieldInstruction yieldInstruction = new YieldInstruction();
            float elapsedTime = 0.0f;

            while (elapsedTime < pulseSeconds)
            {
                yield return yieldInstruction;
                elapsedTime += Time.deltaTime;

                float scale = Utilities.MapRangeToRange(pulseGrowCurve.Evaluate(elapsedTime / pulseSeconds), 0, 1, cursorDownScale * cursorSize, cursorSize);
                SetCursorLocalScale(scale);
            }

            SetCursorLocalScale(cursorSize);
            cursorScalingRoutine = null;
        }

        // Function: ShrinkCursorDot
        // This coroutine smoothly contracts the cursor dots size.
        public virtual IEnumerator ShrinkCursorDot()
        {
            ScaleRing(1);
            YieldInstruction yieldInstruction = new YieldInstruction();
            float elapsedTime = 0.0f;

            while (elapsedTime < pulseSeconds)
            {
                yield return yieldInstruction;
                elapsedTime += Time.deltaTime;

                float scale = Utilities.MapRangeToRange(pulseShrinkCurve.Evaluate(elapsedTime / pulseSeconds), 0, 1, cursorDownScale * cursorSize, cursorSize);
                SetCursorLocalScale(scale);
            }

            SetCursorLocalScale(cursorDownScale * cursorSize);
            cursorScalingRoutine = null;

            if (growQueued)
            {
                growQueued = false;
                cursorScalingRoutine = StartCoroutine(GrowCursorDot());
            }
        }

        // Function: FadeCursor
        // This coroutine smoothly fades the cursors colours.
        protected virtual IEnumerator FadeCursor(float _from, float _to, float _duration, bool _disableOnEnd = false)
        {
            for (int i = 0; i < _duration; i++)
            {
                yield return null;
                float a = Mathf.Lerp(_from, _to, i / _duration);

                cursorBorder.color = new Color()
                {
                    r = cursorBorder.color.r,
                    g = cursorBorder.color.g,
                    b = cursorBorder.color.b,
                    a = a * tertiaryColor.a
                };

                cursorFill.color = new Color()
                {
                    r = cursorFill.color.r,
                    g = cursorFill.color.g,
                    b = cursorFill.color.b,
                    a = a * primaryColor.a
                };
            };

            if (_disableOnEnd)
            {
                SetCursorLocalScale(cursorSize);

                cursorBorder.enabled = false;
                cursorFill.enabled = false;
                ringOuterSprite.enabled = false;
            }
        }

        // Function: SetCursorLocalScale
        // This function resizes the cursor and its border.
        protected virtual void SetCursorLocalScale(float _scale)
        {
            cursorLocalScale = new Vector3(_scale, _scale, _scale);
            cursorBorder.transform.localScale = cursorLocalScale;
        }

        // Function: ResetCursor
        // This function stops all scaling coroutines and clears their related variables.
        public virtual void ResetCursor()
        {
            StopAllCoroutines();
            cursorScalingRoutine = null;
            growQueued = false;

            SetCursorLocalScale(cursorSize);
        }
    }
}