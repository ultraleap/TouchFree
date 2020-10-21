using UnityEngine;
using Leap.Unity;

namespace Ultraleap.ScreenControl.Client
{
    public class Cursor : MonoBehaviour
    {
        public RectTransform cursorTransform;

        public bool _positionOverride = false;
        protected float _screenScale;

        protected Vector2 _targetPos;
        protected Vector2 _overridePosition;

        protected bool hidingCursor = false;

        protected virtual void OnEnable()
        {
            _screenScale = 1;
            //ConfigurationSetupController.EnableCursorVisuals += ShowCursor;
            //ConfigurationSetupController.DisableCursorVisuals += HideCursor;
            //SettingsConfig.OnConfigUpdated += OnConfigUpdated;
            ConnectionManager.coreConnection.TransmitInputAction += OnHandleInputAction;
            OnConfigUpdated();
            ResetCursor();
            ShowCursor();
        }

        protected virtual void OnDisable()
        {
            //ConfigurationSetupController.EnableCursorVisuals -= ShowCursor;
            //ConfigurationSetupController.DisableCursorVisuals -= HideCursor;
            //SettingsConfig.OnConfigUpdated -= OnConfigUpdated;
            ConnectionManager.coreConnection.TransmitInputAction -= OnHandleInputAction;
        }

        protected virtual void Update()
        {
            //if (Hands.Provider.CurrentFrame.Hands.Count > 0)
            //{
            //    cursorTransform.gameObject.SetActive(true);
            //    cursorTransform.anchoredPosition = _positionOverride ? _overridePosition : _targetPos;

            //    if (hidingCursor)
            //    { // Only show the cursor if we are not in the auto setup screen of the configuration
            //        ShowCursor();
            //    }
            //}
            //else
            //{
            //    if (!hidingCursor)
            //    {
            //        HideCursor();
            //    }
            //}

            //TODO: only set active if we are supposed to show the cursor (if an event has been sent recently)
            cursorTransform.gameObject.SetActive(true);
            cursorTransform.anchoredPosition = _positionOverride ? _overridePosition : _targetPos;
        }

        public virtual void UpdateCursor(Vector2 _screenPos, float _progressToClick)
        {
            _targetPos = _screenPos;
        }

        protected virtual void OnHandleInputAction(ScreenControlTypes.ClientInputAction _inputData)
        {
        }

        protected virtual void OnConfigUpdated()
        {
        }

        public virtual void ResetCursor()
        {

        }

        public virtual void ShowCursor()
        {
            hidingCursor = false;
        }

        public virtual void HideCursor()
        {
            hidingCursor = true;
        }

        public virtual void OverridePosition(bool active, Vector2 position)
        {
            _positionOverride = active;
            _overridePosition = position;
        }

        public virtual Vector2 TargetPosition()
        {
            return _targetPos;
        }

        public virtual void SetScreenScale(float _scale)
        {
            _screenScale = _scale;
        }
    }
}