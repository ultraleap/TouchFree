using System.Collections;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    public class DotCursor : TouchlessCursor
    {
        [Header("Graphics")]
        public UnityEngine.UI.Image cursorBorder;
        public UnityEngine.UI.Image cursorFill;
        public SpriteRenderer ringOuterSprite;

        [Header("Ring")]
        public bool ringEnabled;
        public float cursorMaxRingSize = 2;

        public AnimationCurve ringCurve;

        public RectTransform ringOuter;
        public RectTransform ringMask;

        public float ringWidth;
        public float ringSpriteScale;

        [Header("Pulse")]
        public AnimationCurve pulseGrowCurve;
        public AnimationCurve pulseShrinkCurve;
        [Range(0.01f, 1f)] public float pulseSeconds;
        [Range(0.01f, 2f)] public float cursorDownScale;

        [Range(0f, 60f)] public float fadeDuration = 30;

        protected float cursorDotSize;
        protected float maxRingScale;

        protected Color dotFillColor;
        protected Color dotBorderColor;
        protected Color ringColor;

        protected bool growQueued = false;

        protected Vector3 cursorLocalScale = Vector3.one;

        protected const int ringSpriteSortingOrder = 32766;

        protected override void Update()
        {
            base.Update();
            if (growQueued && cursorScalingRoutine == null)
            {
                growQueued = false;
                cursorScalingRoutine = StartCoroutine(GrowCursorDot());
            }
        }

        public override void UpdateCursor(Vector2 _screenPos, float _progressToClick)
        {
            _targetPos = _screenPos;

            if (ringEnabled)
            {
                //progressToClick is between 0 and 1. Click triggered at progressToClick = 1
                var dist = Mathf.Clamp01(1f - _progressToClick);

                if (hidingCursor)
                {
                    ringOuterSprite.color = new Color(ringColor.r, ringColor.g, ringColor.b, 0);
                }
                else
                {
                    ringOuterSprite.color = new Color(ringColor.r, ringColor.g, ringColor.b, Mathf.Lerp(ringColor.a, 0f, dist));

                    // 0.9f so that the boundary between ring and dot is not visible - small overlap.
                    float minRingScale = 0.9f + ringWidth / ringSpriteScale;
                    var ringScale = Mathf.Lerp(minRingScale, maxRingScale, ringCurve.Evaluate(dist));

                    ringOuter.transform.localScale = Vector3.Lerp(ringOuter.transform.localScale, Vector3.one * ringScale * ringSpriteScale, Time.deltaTime * 15);

                    ringMask.transform.localScale = new Vector3()
                    {
                        x = Mathf.Max(0, ringOuter.localScale.x - ringWidth),
                        y = Mathf.Max(0, ringOuter.localScale.y - ringWidth),
                        z = ringOuter.localScale.z
                    };
                }
            }
        }

        protected override void HandleInputAction(ScreenControlTypes.ClientInputAction _inputData)
        {
            base.HandleInputAction(_inputData);
            ScreenControlTypes.InputType _type = _inputData.InputType;
            Vector2 _cursorPosition = _inputData.CursorPosition;
            float _distanceFromScreen = _inputData.ProgressToClick;

            switch (_type)
            {
                case ScreenControlTypes.InputType.MOVE:
                    UpdateCursor(_cursorPosition, _distanceFromScreen);
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
            var dotSizeIsZero = Mathf.Approximately(cursorDotSize, 0f);
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

        Coroutine cursorScalingRoutine;
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

        Coroutine fadeRoutine;
        public override void ShowCursor()
        {
            bool wasShowing = !hidingCursor;

            base.ShowCursor();

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

            base.HideCursor();

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

        public override void ResetCursor()
        {
            base.ResetCursor();
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
                var a = Mathf.Lerp(_from, _to, i / _duration);

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
    }
}