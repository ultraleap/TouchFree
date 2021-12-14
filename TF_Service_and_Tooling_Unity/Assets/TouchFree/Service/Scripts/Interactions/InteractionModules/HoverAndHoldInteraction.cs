using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
using Ultraleap.TouchFree.ServiceShared;
using Leap;

namespace Ultraleap.TouchFree.Service
{
    public class HoverAndHoldInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.HOVER;

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

        InputType nextInput = InputType.NONE;

        private Vector2 ApplyHoverzone(Vector2 _screenPosM)
        {
            float deadzoneRad = positioningModule.Stabiliser.defaultDeadzoneRadius + hoverDeadzoneEnlargementDistance;
            previousHoverPosDeadzone = PositionStabiliser.ApplyDeadzoneSized(previousHoverPosDeadzone, _screenPosM, deadzoneRad);
            return previousHoverPosDeadzone;
        }

        private void HandleInteractions(Vector2 _hoverPosition)
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
                    hoverTriggeredDeadzoneRadius = positioningModule.Stabiliser.currentDeadzoneRadius;
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
                            positioningModule.Stabiliser.currentDeadzoneRadius = (timerDeadzoneEnlargementDistance + positioningModule.Stabiliser.defaultDeadzoneRadius);
                            progressTimer.StopTimer();
                            clickHeld = true;
                            clickingTimer.Restart();

                            nextInput = InputType.DOWN;
                            isTouching = true;
                        }
                        else
                        {
                            nextInput = InputType.MOVE;
                            float maxDeadzoneRadius = timerDeadzoneEnlargementDistance + positioningModule.Stabiliser.defaultDeadzoneRadius;
                            float deadzoneRadius = Mathf.Lerp(hoverTriggeredDeadzoneRadius, maxDeadzoneRadius, progressTimer.Progress);
                            positioningModule.Stabiliser.currentDeadzoneRadius = deadzoneRadius;
                            isTouching = true;
                        }
                    }
                    else
                    {
                        if (!clickAlreadySent && clickingTimer.ElapsedMilliseconds > clickHoldTime)
                        {
                            nextInput = InputType.UP;
                            clickAlreadySent = true;
                            isTouching = false;
                        }
                        else if(clickAlreadySent)
                        {
                            nextInput = InputType.NONE;
                        }
                    }
                }
                else
                {
                    if (clickHeld && !clickAlreadySent)
                    {
                        // Handle unclick if move before timer's up
                        nextInput = InputType.UP;
                    }

                    progressTimer.ResetTimer();

                    hoverTriggered = false;
                    hoverTriggerTimer.Stop();

                    clickHeld = false;
                    clickAlreadySent = false;
                    clickingTimer.Stop();
                    isTouching = false;

                    positioningModule.Stabiliser.StartShrinkingDeadzone(deadzoneShrinkSpeed);
                }
            }
            else
            {
                nextInput = InputType.MOVE;
                isTouching = false;
            }

            previousHoverPosScreen = _hoverPosition;
            previousScreenPos = positions.CursorPosition;
        }

        protected override void OnSettingsUpdated()
        {
            base.OnSettingsUpdated();
            hoverTriggerTime = ConfigManager.InteractionConfig.HoverAndHold.HoverStartTimeS * 1000; // s to ms
            progressTimer.timeLimit = ConfigManager.InteractionConfig.HoverAndHold.HoverCompleteTimeS * 1000; // s to ms
        }

        public override float CalculateProgress(Hand _hand)
        {
            positions = positioningModule.CalculatePositions(_hand);

            if (_hand == null)
            {
                if (InteractionManager.Instance.hadHandLastFrame)
                {
                    // We lost the hand so cancel anything we may have been doing
                    SendInputAction(InputType.CANCEL, positions, 0);
                }
                isTouching = false;

                return 0;
            }

            Vector2 cursorPositionM = ConfigManager.GlobalSettings.virtualScreen.PixelsToMeters(positions.CursorPosition);
            Vector2 hoverPosM = ApplyHoverzone(cursorPositionM);
            Vector2 hoverPos = ConfigManager.GlobalSettings.virtualScreen.MetersToPixels(hoverPosM);

            HandleInteractions(hoverPos);

            if (clickHeld)
            {
                return 1;
            }

            return progressTimer.Progress;
        }

        public override void RunInteraction(Hand _hand, float _progress)
        {
            positions = positioningModule.CalculatePositions(_hand);
            SendInputAction(nextInput, positions, _progress);
        }
    }
}