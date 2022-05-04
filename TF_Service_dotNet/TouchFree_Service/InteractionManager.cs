using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions;

using Ultraleap.TouchFree.Service.Connection;

namespace Ultraleap.TouchFree.Service
{
    public class InteractionManager
    {
        private readonly AirPushInteraction airPush;
        private readonly GrabInteraction grab;
        private readonly HoverAndHoldInteraction hoverAndHold;
        private readonly TouchPlanePushInteraction touchPlane;

        private InteractionType lastInteraction;

        private readonly UpdateBehaviour updateBehaviour;
        private readonly ClientConnectionManager connectionManager;

        public InteractionManager(
            UpdateBehaviour _updateBehaviour,
            ClientConnectionManager _connectionManager,
            AirPushInteraction _airPush,
            GrabInteraction _grab,
            HoverAndHoldInteraction _hoverAndHold,
            TouchPlanePushInteraction _touchPlane,
            IConfigManager _configManager)
        {
            updateBehaviour = _updateBehaviour;
            connectionManager = _connectionManager;

            airPush = _airPush;
            grab = _grab;
            hoverAndHold = _hoverAndHold;
            touchPlane = _touchPlane;

            _configManager.OnInteractionConfigUpdated += OnInteractionSettingsUpdated;

            OnInteractionSettingsUpdated(_configManager.InteractionConfig);


            InputInjector.Initialize(10, TouchFeedback.NONE);
            touches = new PointerTouchInfo[1];
            touches[0].PointerInfo.PointerInputType = PointerInputType.TOUCH;
            touches[0].TouchFlags = TouchFlags.NONE;
            touches[0].TouchMasks = TouchMask.NONE;
            touches[0].PointerInfo.PointerId = 1;

            _configManager.OnPhysicalConfigUpdated += OnPhysicalSettingsUpdated;
            OnPhysicalSettingsUpdated(_configManager.PhysicalConfig);
        }

        protected void OnInteractionSettingsUpdated(InteractionConfigInternal _config)
        {
            switch(lastInteraction)
            {
                case InteractionType.TOUCHPLANE:
                    DisableInteraction(touchPlane);
                    break;

                case InteractionType.PUSH:
                    DisableInteraction(airPush);
                    break;

                case InteractionType.HOVER:
                    DisableInteraction(hoverAndHold);
                    break;

                case InteractionType.GRAB:
                    DisableInteraction(grab);
                    break;
            }

            switch(_config.InteractionType)
            {
                case InteractionType.TOUCHPLANE:
                    EnableInteraction(touchPlane);
                    break;

                case InteractionType.PUSH:
                    EnableInteraction(airPush);
                    break;

                case InteractionType.HOVER:
                    EnableInteraction(hoverAndHold);
                    break;

                case InteractionType.GRAB:
                    EnableInteraction(grab);
                    break;
            }

            lastInteraction = _config.InteractionType;
        }

        protected void EnableInteraction(InteractionModule target)
        {
            updateBehaviour.OnUpdate += target.Update;
            target.HandleInputAction += connectionManager.SendInputActionToWebsocket;
            target.HandleInputAction += HandleInputAction;
            target.Enable();
        }

        protected void DisableInteraction(InteractionModule target)
        {
            updateBehaviour.OnUpdate -= target.Update;
            target.HandleInputAction -= connectionManager.SendInputActionToWebsocket;
            target.HandleInputAction -= HandleInputAction;
            target.Disable();
        }






        protected void OnPhysicalSettingsUpdated(PhysicalConfigInternal _config)
        {
            screenHeight = _config.ScreenHeightPX;
            screenWidth = _config.ScreenWidthPX;
        }

        PointerTouchInfo[] touches;
        bool pressing = false;
        int screenHeight = 0;
        int screenWidth = 0;

        bool useMouse = true;

        void HandleInputAction(InputAction _inputData)
        {
            InputOverride inputOverride = new InputOverride(_inputData.InputType, _inputData.CursorPosition);
            HandleInputOverride(inputOverride);
        }

        public void HandleInputOverride(InputOverride _inputData)
        {
            var x = (int)_inputData.CursorPosition.X;
            var y = screenHeight - (int)_inputData.CursorPosition.Y;

            if (!useMouse)
            {
                switch (_inputData.InputType)
                {
                    case InputType.DOWN:
                        touches[0].PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INCONTACT | PointerFlags.INRANGE;
                        touches[0].PointerInfo.PtPixelLocation.X = x;
                        touches[0].PointerInfo.PtPixelLocation.Y = y;
                        InputInjector.SendTouchEvent(touches);
                        pressing = true;
                        break;
                    case InputType.MOVE:
                        if (pressing)
                        {
                            touches[0].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INCONTACT | PointerFlags.INRANGE;
                        }
                        else
                        {
                            touches[0].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE;
                        }
                        touches[0].PointerInfo.PtPixelLocation.X = x;
                        touches[0].PointerInfo.PtPixelLocation.Y = y;
                        InputInjector.SendTouchEvent(touches);
                        break;
                    case InputType.UP:
                        touches[0].PointerInfo.PointerFlags = PointerFlags.UP | PointerFlags.INRANGE;
                        touches[0].PointerInfo.PtPixelLocation.X = x;
                        touches[0].PointerInfo.PtPixelLocation.Y = y;
                        InputInjector.SendTouchEvent(touches);
                        pressing = false;
                        break;
                    case InputType.CANCEL:
                        touches[0].PointerInfo.PointerFlags = PointerFlags.CANCELLED | PointerFlags.UP;
                        touches[0].PointerInfo.PtPixelLocation.X = x;
                        touches[0].PointerInfo.PtPixelLocation.Y = y;
                        InputInjector.SendTouchEvent(touches);
                        pressing = false;
                        break;
                }
            }
            else
            {
                switch (_inputData.InputType)
                {
                    case InputType.DOWN:
                        InputInjector.SendMouseEvent(MouseEventFlags.LEFTDOWN, x, y, screenWidth, screenHeight);
                        pressing = true;
                        break;
                    case InputType.MOVE:
                        InputInjector.SendMouseEvent(MouseEventFlags.MOVE, x, y, screenWidth, screenHeight);
                        break;
                    case InputType.UP:
                        InputInjector.SendMouseEvent(MouseEventFlags.LEFTUP, x, y, screenWidth, screenHeight);
                        pressing = false;
                        break;
                    case InputType.CANCEL:
                        InputInjector.SendMouseEvent(MouseEventFlags.MOVE, x, y, screenWidth, screenHeight);
                        pressing = false;
                        break;
                    default:
                        InputInjector.SendMouseEvent(MouseEventFlags.MOVE, x, y, screenWidth, screenHeight);
                        break;
                }
            }
        }
    }
}