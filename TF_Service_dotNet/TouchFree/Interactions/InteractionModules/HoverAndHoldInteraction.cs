using System;
using System.Diagnostics;
using System.Numerics;

using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions.InteractionModules;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class HoverAndHoldInteraction : InteractionModule
    {
        public override InteractionType InteractionType { get; } = InteractionType.HOVER;

        public ProgressTimer progressTimer = new ProgressTimer(600f);

        public float hoverDeadzoneEnlargementDistance = 5f;
        public float timerDeadzoneEnlargementDistance = 5f;

        public float deadzoneShrinkSpeed = 0.3f;

        public float hoverTriggerTime = 500f;
        public float clickHoldTime = 200f;

        private Vector2 previousHoverPosDeadzone = Vector2.Zero;
        private Vector2 previousScreenPos = Vector2.Zero;

        private bool hoverTriggered = false;
        private float hoverTriggeredDeadzoneRadius = 0f;
        private Stopwatch hoverTriggerTimer = new Stopwatch();

        private bool clickHeld = false;
        private bool clickAlreadySent = false;
        private Stopwatch clickingTimer = new Stopwatch();

        public HoverAndHoldInteraction(
            HandManager _handManager,
            IVirtualScreen _virtualScreen,
            IConfigManager _configManager,
            IPositioningModule _positioningModule,
            IPositionStabiliser _positionStabiliser) : base(_handManager, _virtualScreen, _configManager, _positioningModule, _positionStabiliser)
        {
            positionConfiguration = new[]
            {
                new PositionTrackerConfiguration(TrackedPosition.INDEX_STABLE, 1)
            };
        }

        protected override InputActionResult UpdateData(Leap.Hand hand)
        {
            if (hand == null)
            {
                if (hadHandLastFrame)
                {
                    // We lost the hand so cancel anything we may have been doing
                    return new InputActionResult(InputType.CANCEL, positions, 0);
                }

                return new InputActionResult();
            }

            Vector2 cursorPositionMm = virtualScreen.PixelsToMillimeters(positions.CursorPosition);
            Vector2 hoverPosMm = ApplyHoverzone(cursorPositionMm);
            positions.CursorPosition = virtualScreen.MillimetersToPixels(hoverPosMm);

            return HandleInteractions();
        }

        private Vector2 ApplyHoverzone(Vector2 _screenPosMm)
        {
            float deadzoneRad = positionStabiliser.defaultDeadzoneRadius + hoverDeadzoneEnlargementDistance;
            previousHoverPosDeadzone = positionStabiliser.ApplyDeadzoneSized(previousHoverPosDeadzone, _screenPosMm, deadzoneRad);
            return previousHoverPosDeadzone;
        }

        private InputActionResult HandleInteractions()
        {
            var inputActionResult = new InputActionResult();

            if (!clickHeld && !hoverTriggered && positions.CursorPosition == previousScreenPos)
            {
                if (!hoverTriggerTimer.IsRunning)
                {
                    hoverTriggerTimer.Restart();
                }
                else if (hoverTriggerTimer.ElapsedMilliseconds > hoverTriggerTime)
                {
                    hoverTriggered = true;
                    hoverTriggerTimer.Stop();
                    hoverTriggeredDeadzoneRadius = positionStabiliser.currentDeadzoneRadius;
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
                            positionStabiliser.currentDeadzoneRadius = (timerDeadzoneEnlargementDistance + positionStabiliser.defaultDeadzoneRadius);
                            progressTimer.StopTimer();
                            clickHeld = true;
                            clickingTimer.Restart();
                            inputActionResult = new InputActionResult(InputType.DOWN, positions, 0f);
                        }
                        else
                        {
                            inputActionResult = new InputActionResult(InputType.MOVE, positions, progressTimer.Progress);

                            float maxDeadzoneRadius = timerDeadzoneEnlargementDistance + positionStabiliser.defaultDeadzoneRadius;
                            float deadzoneRadius = Utilities.Lerp(hoverTriggeredDeadzoneRadius, maxDeadzoneRadius, progressTimer.Progress);

                            positionStabiliser.currentDeadzoneRadius = deadzoneRadius;
                        }
                    }
                    else
                    {
                        if (!clickAlreadySent && clickingTimer.ElapsedMilliseconds > clickHoldTime)
                        {
                            inputActionResult = new InputActionResult(InputType.UP, positions, progressTimer.Progress);
                            clickAlreadySent = true;
                        }
                    }
                }
                else
                {
                    if (clickHeld && !clickAlreadySent)
                    {
                        // Handle unclick if move before timer's up
                        inputActionResult = new InputActionResult(InputType.UP, positions, progressTimer.Progress);
                    }

                    progressTimer.ResetTimer();

                    hoverTriggered = false;
                    hoverTriggerTimer.Stop();

                    clickHeld = false;
                    clickAlreadySent = false;
                    clickingTimer.Stop();

                    positionStabiliser.StartShrinkingDeadzone(deadzoneShrinkSpeed);
                }
            }
            else
            {
                inputActionResult = new InputActionResult(InputType.MOVE, positions, progressTimer.Progress);
            }

            previousScreenPos = positions.CursorPosition;

            return inputActionResult;
        }

        protected override void OnInteractionSettingsUpdated(InteractionConfigInternal _config)
        {
            base.OnInteractionSettingsUpdated(_config);
            if (_config.HoverAndHold != null)
            {
                hoverTriggerTime = _config.HoverAndHold.HoverStartTimeS * 1000; // s to ms
                progressTimer.timeLimit = _config.HoverAndHold.HoverCompleteTimeS * 1000; // s to ms
            }
        }
    }
}