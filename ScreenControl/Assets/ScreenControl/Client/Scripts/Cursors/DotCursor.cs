using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client.Cursors
{
    /**
        Class: DotCursor

        This is an example Touchless Cursor which positions a dot on the screen at the hand location,
        and reacts to the current progress to click of the interaction (what determines this depends
        on the currently active interaction)

        There is a Prefab set up to make full use of this Class which we recommend we use and
     */
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class DotCursor : TouchlessCursor
    {
        // Group: Variables

        /**
            Variable: fadeDuration

            The amount of time the
         */
        [Range(0f, 60f)] public float fadeDuration = 30;

        /**
            Variable: cursorBorder

            The image of the black border around the dot, this is the parent image in the prefab
            and is used to do all of the scaling of the images that make up this cursor.
         */
        [Header("Graphics")]
        public UnityEngine.UI.Image cursorBorder;
        /**
            Variable: cursorFill

            The main background image of the Cursor, used for fading the image out.
         */
        public UnityEngine.UI.Image cursorFill;

        /**
            Variable: ringOuterSprite

            This refers to the Ring around the central cursor that is used to display the "reactive"
            state of the cursor; the closer the ring is to the dot, the closer you are to "clicking"

            The sprite is accessed to fade in/out the ring as it is needed.
         */
        public SpriteRenderer ringOuterSprite;

        [Header("Ring")]
        /**
            Variable: ringEnabled

            Enables/disables the ring cursor around the dot. Here primarily for use in the inspector.
         */
        public bool ringEnabled;

        /**
            Variable: cursorMaxRingSize

            The maximum size for the ring to be (relative to the size of the dot, e.g. a value of 2
            means the ring can be (at largest) twice the scale of the dot. )
         */
        public float cursorMaxRingSize = 2;

        /**
            Variable: ringCurve

            This curve is used to determine how the ring's scale changes with the value of the latest
            InputAction's ProgressToClick.
         */
        public AnimationCurve ringCurve;

        /**
            Variable: ringOuter

            This refers to the Ring around the central cursor that is used to display the "reactive"
            state of the cursor; the closer the ring is to the dot, the closer you are to "clicking"
         */
        public RectTransform ringOuter;

        /**
            Variable: ringOuter

            This is a reference to the mask that is used to make the center of the ring visual
            transparent. It has to be scaled to match the ring itself.
         */
        public RectTransform ringMask;

        /**
            Used to set the width / thickness of the ring itself
            (i.e. the distance between the inner and outer edges of the ring)
         */
        public float ringWidth;

        [Header("Pulse")]
        /**
            Variable: pulseGrowCurve

            When a "click" is recognised, an animation plays where the dot "pulses" (briefly
            shrinking and expanding). This AnimationCurve governs the shrinking that follows a
            "Down" input.
         */
        public AnimationCurve pulseShrinkCurve;

        /**
            Variable: pulseGrowCurve

            When a "click" is recognised, an animation plays where the dot "pulses" (briefly
            shrinking and expanding). This AnimationCurve governs the growing back to full size
         */
        public AnimationCurve pulseGrowCurve;

        /**
            Variable: pulseSeconds

            The amount of time each part of the pulse should last (a pulse will last twice this long)
         */
        [Range(0.01f, 1f)] public float pulseSeconds;

        /**
            Variable: cursorDownScale

            The scale of the cursor's dot at its smallest size (between the shrink and the grow )
         */
        [Range(0.01f, 2f)] public float cursorDownScale;

        Coroutine cursorScalingRoutine;
        Coroutine fadeRoutine;

        protected float cursorDotSize;
        protected float maxRingScale;
        protected Color dotFillColor;
        protected Color dotBorderColor;
        protected Color ringColor;


        protected bool hidingCursor = false;
        protected bool growQueued = false;
        protected Vector3 cursorLocalScale = Vector3.one;
        protected const int ringSpriteSortingOrder = 32766;

        // Group: Functions


        public IEnumerator GrowCursorDot()
        {
            SetCursorLocalScale(cursorDownScale * cursorDotSize);

            YieldInstruction yieldInstruction = new YieldInstruction();
            float elapsedTime = 0.0f;

            while (elapsedTime < pulseSeconds)
            {
                yield return yieldInstruction;
                elapsedTime += Time.deltaTime;

                float scale = Utilities.MapRangeToRange(pulseGrowCurve.Evaluate(elapsedTime / pulseSeconds), 0, 1, cursorDownScale * cursorDotSize, cursorDotSize);
                SetCursorLocalScale(scale);
            }

            SetCursorLocalScale(cursorDotSize);
            cursorScalingRoutine = null;
        }

        public IEnumerator ShrinkCursorDot()
        {
            YieldInstruction yieldInstruction = new YieldInstruction();
            float elapsedTime = 0.0f;

            while (elapsedTime < pulseSeconds)
            {
                yield return yieldInstruction;
                elapsedTime += Time.deltaTime;

                float scale = Utilities.MapRangeToRange(pulseShrinkCurve.Evaluate(elapsedTime / pulseSeconds), 0, 1, cursorDownScale * cursorDotSize, cursorDotSize);
                SetCursorLocalScale(scale);
            }

            SetCursorLocalScale(cursorDownScale * cursorDotSize);
            cursorScalingRoutine = null;
        }

        private void SetCursorLocalScale(float scale)
        {
            cursorLocalScale = new Vector3(scale, scale, scale);
            cursorBorder.transform.localScale = cursorLocalScale;
        }

        public void UpdateCursor(Vector2 _screenPos, float _progressToClick)
        {
            _targetPos = _screenPos;

            if (ringEnabled)
            {
                //progressToClick is between 0 and 1. Click triggered at progressToClick = 1
                float dist = Mathf.Clamp01(1f - _progressToClick);

                if (hidingCursor)
                {
                    ringOuterSprite.color = new Color(ringColor.r, ringColor.g, ringColor.b, 0);
                }
                else
                {
                    // 0.9f so that the boundary between ring and dot is not visible - small overlap.
                    float minRingScale = 0.9f + ringWidth;
                    float ringScale = Mathf.Lerp(minRingScale, maxRingScale, ringCurve.Evaluate(dist));

                    ringOuterSprite.color = new Color(ringColor.r, ringColor.g, ringColor.b, Mathf.Lerp(ringColor.a, 0f, dist));

                    ringOuter.transform.localScale = Vector3.Lerp(ringOuter.transform.localScale, Vector3.one * ringScale, Time.deltaTime * 15);

                    ringMask.transform.localScale = new Vector3()
                    {
                        x = Mathf.Max(0, ringOuter.localScale.x - ringWidth),
                        y = Mathf.Max(0, ringOuter.localScale.y - ringWidth),
                        z = ringOuter.localScale.z
                    };
                }
            }
        }

        public void ResetCursor()
        {
            StopAllCoroutines();
            fadeRoutine = null;
            cursorScalingRoutine = null;
            growQueued = false;

            SetCursorLocalScale(cursorDotSize);
        }

        private IEnumerator FadeCursor(float _from, float _to, float _duration, bool _disableOnEnd = false)
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
                    a = a * dotBorderColor.a
                };

                cursorFill.color = new Color()
                {
                    r = cursorFill.color.r,
                    g = cursorFill.color.g,
                    b = cursorFill.color.b,
                    a = a * dotFillColor.a
                };
            };

            if (_disableOnEnd)
            {
                SetCursorLocalScale(cursorDotSize);

                cursorBorder.enabled = false;
                cursorFill.enabled = false;

                if (ringEnabled)
                {
                    ringOuterSprite.enabled = false;
                }
            }

            fadeRoutine = null;
        }

        // Group: TouchlessCursor Overrides
        protected override void Update()
        {
            base.Update();

            if (growQueued && cursorScalingRoutine == null)
            {
                growQueued = false;
                cursorScalingRoutine = StartCoroutine(GrowCursorDot());
            }
        }

        protected override void HandleInputAction(ScreenControlTypes.ClientInputAction _inputData)
        {
            switch (_inputData.InputType)
            {
                case ScreenControlTypes.InputType.MOVE:
                    UpdateCursor(_inputData.CursorPosition, _inputData.ProgressToClick);
                    break;

                case ScreenControlTypes.InputType.DOWN:
                    growQueued = false;
                    if (cursorScalingRoutine != null)
                    {
                        StopCoroutine(cursorScalingRoutine);
                    }
                    cursorScalingRoutine = StartCoroutine(ShrinkCursorDot());
                    break;

                case ScreenControlTypes.InputType.UP:
                    growQueued = true;
                    break;

                case ScreenControlTypes.InputType.CANCEL:
                    break;
            }
        }

        protected override void InitialiseCursor()
        {
            dotFillColor = cursorFill.color;
            dotBorderColor = cursorBorder.color;

            if (ringEnabled)
            {
                ringColor = ringOuterSprite.color;
            }

            cursorDotSize = cursorSize;
            bool dotSizeIsZero = Mathf.Approximately(cursorDotSize, 0f);
            cursorDotSize = dotSizeIsZero ? 1f : cursorDotSize;
            cursorBorder.transform.localScale = new Vector3(cursorDotSize, cursorDotSize, cursorDotSize);
            SetCursorLocalScale(cursorDotSize);

            if (ringEnabled)
            {
                maxRingScale = (1f / cursorDotSize) * cursorMaxRingSize;

                // This is a crude way of forcing the sprites to draw on top of the UI, without masking it.
                ringOuterSprite.sortingOrder = ringSpriteSortingOrder;
                ringMask.GetComponent<SpriteMask>().isCustomRangeActive = true;
                ringMask.GetComponent<SpriteMask>().frontSortingOrder = ringSpriteSortingOrder + 1;
                ringMask.GetComponent<SpriteMask>().backSortingOrder = ringSpriteSortingOrder - 1;
            }
        }

        public override void ShowCursor()
        {
            bool wasShowing = !hidingCursor;
            hidingCursor = false;

            if (wasShowing)
            {
                return;
            }

            ResetCursor();

            fadeRoutine = StartCoroutine(FadeCursor(0, 1, fadeDuration));
            cursorBorder.enabled = true;
            cursorFill.enabled = true;

            if (ringEnabled)
            {
                ringOuterSprite.enabled = true;
                ringOuterSprite.color = new Color(ringColor.r, ringColor.g, ringColor.b, 0);
            }
        }

        public override void HideCursor()
        {
            bool wasHiding = hidingCursor;
            hidingCursor = true;

            if (wasHiding)
            {
                return;
            }

            ResetCursor();

            fadeRoutine = StartCoroutine(FadeCursor(1, 0, fadeDuration, true));

            if (ringEnabled)
            {
                ringOuterSprite.color = new Color(ringColor.r, ringColor.g, ringColor.b, 0);
            }
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}