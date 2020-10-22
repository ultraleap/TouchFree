using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
using Ultraleap.ScreenControl.Core.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Core
{
    public class HoverAndHoldInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.Hover;
        public bool InteractionEnabled { get; set; } = true;

        public ProgressTimer progressTimer;

        public float hoverDeadzoneEnlargementDistance = 0.02f;
        public float timerDeadzoneEnlargementDistance = 0.02f;

        public float deadzoneShrinkSpeed = 0.3f;
        private Vector2 previousHoverPosDeadzone = Vector2.zero;
        private Vector2 previousHoverPosScreen = Vector2.zero;
        private Vector2 previousScreenPos = Vector2.zero;

        public float hoverTriggerTime = 200f;
        private bool hoverTriggered = false;
        private float hoverTriggeredDeadzoneRadius = 0f;
        private Stopwatch hoverTriggerTimer = new Stopwatch();

        public float clickHoldTime = 200f;
        private bool clickHeld = false;
        private bool clickAlreadySent = false;
        private Stopwatch clickingTimer = new Stopwatch();

        protected override void UpdateData(Leap.Hand hand)
        {
            if (hand == null)
            {
                return;
            }

            if (!InteractionEnabled)
            {
                return;
            }

            positions = positioningModule.CalculatePositions(hand);
            Vector2 cursorPositionM = GlobalSettings.virtualScreen.PixelsToMeters(positions.CursorPosition);
            Vector2 hoverPosM = ApplyHoverzone(cursorPositionM);
            Vector2 hoverPos = GlobalSettings.virtualScreen.MetersToPixels(hoverPosM);

            HandleInteractions(hoverPos);
        }

        private Vector2 ApplyHoverzone(Vector2 _screenPosM)
        {
            float deadzoneRad = positioningModule.Stabiliser.defaultDeadzoneRadius + hoverDeadzoneEnlargementDistance;
            previousHoverPosDeadzone = PositionStabiliser.ApplyDeadzoneSized(previousHoverPosDeadzone, _screenPosM, deadzoneRad);
            return previousHoverPosDeadzone;
        }

        private void HandleInteractions(Vector2 _hoverPosition)
        {
            SendInputAction(InputType.MOVE, positions, progressTimer.Progress);
            HandleInputHoverAndHold(_hoverPosition);
        }

        private void HandleInputHoverAndHold(Vector2 _hoverPosition)
        {
            if (!clickHeld && !hoverTriggered && _hoverPosition == previousHoverPosScreen)
            {
                if (!hoverTriggerTimer.IsRunning)
                {
                    hoverTriggerTimer.Restart();
                }
                else if (hoverTriggerTimer.ElapsedMilliseconds > hoverTriggerTime)
                {
                    hoverTriggered = true;
                    hoverTriggerTimer.Stop();
                    hoverTriggeredDeadzoneRadius = positioningModule.Stabiliser.GetCurrentDeadzoneRadius();
                    previousScreenPos = positions.CursorPosition; // To prevent instant-abandonment of hover
                }
            }

            if (hoverTriggered)
            {
                if (positions.CursorPosition == previousScreenPos)
                {
                    if (!clickHeld)
                    {
                        if (!progressTimer.IsRunning && progressTimer.Progress == 0f)
                        {
                            progressTimer.StartTimer();
                        }
                        else if (progressTimer.IsRunning && progressTimer.Progress == 1f)
                        {
                            positioningModule.Stabiliser.SetCurrentDeadzoneRadius(timerDeadzoneEnlargementDistance + positioningModule.Stabiliser.defaultDeadzoneRadius);
                            progressTimer.StopTimer();
                            clickHeld = true;
                            clickingTimer.Restart();
                            SendInputAction(InputType.DOWN, positions, 0f);
                        }
                        else
                        {
                            float maxDeadzoneRadius = timerDeadzoneEnlargementDistance + positioningModule.Stabiliser.defaultDeadzoneRadius;
                            float deadzoneRadius = Mathf.Lerp(hoverTriggeredDeadzoneRadius, maxDeadzoneRadius, progressTimer.Progress);
                            positioningModule.Stabiliser.SetCurrentDeadzoneRadius(deadzoneRadius);
                        }
                    }
                    else
                    {
                        if (!clickAlreadySent && clickingTimer.ElapsedMilliseconds > clickHoldTime)
                        {
                            SendInputAction(InputType.UP, positions, progressTimer.Progress);
                            clickAlreadySent = true;
                        }
                    }
                }
                else
                {
                    if (clickHeld && !clickAlreadySent)
                    {
                        // Handle unclick if move before timer's up
                        SendInputAction(InputType.UP, positions, progressTimer.Progress);
                    }

                    progressTimer.ResetTimer();

                    hoverTriggered = false;
                    hoverTriggerTimer.Stop();

                    clickHeld = false;
                    clickAlreadySent = false;
                    clickingTimer.Stop();

                    positioningModule.Stabiliser.StartShrinkingDeadzone(ShrinkType.MOTION_BASED, deadzoneShrinkSpeed);
                }
            }

            previousHoverPosScreen = _hoverPosition;
            previousScreenPos = positions.CursorPosition;
        }

        protected override void OnSettingsUpdated()
        {
            base.OnSettingsUpdated();
            hoverTriggerTime = SettingsConfig.Config.HoverCursorStartTimeS * 1000; // s to ms
            progressTimer.timeLimit = SettingsConfig.Config.HoverCursorCompleteTimeS * 1000; // s to ms
        }
    }
}