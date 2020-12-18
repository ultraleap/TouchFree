using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    public class TouchlessCursor : MonoBehaviour
    {
        public RectTransform cursorTransform;

        public float cursorSize = 0.25f;

        protected Vector2 _targetPos;

        protected bool hidingCursor = false;

        protected virtual void OnEnable()
        {
            ConnectionManager.AddConnectionListener(OnCoreConnection);
            InitialiseCursor();
            ResetCursor();
            ShowCursor();
        }

        protected virtual void OnCoreConnection()
        {
            ConnectionManager.coreConnection.TransmitInputAction += HandleInputAction;
        }

        protected virtual void OnDisable()
        {
            if (ConnectionManager.coreConnection != null)
            {
                ConnectionManager.coreConnection.TransmitInputAction -= HandleInputAction;
            }
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
            cursorTransform.anchoredPosition = _targetPos;
        }

        public virtual void UpdateCursor(Vector2 _screenPos, float _progressToClick)
        {
            _targetPos = _screenPos;
        }

        protected virtual void HandleInputAction(ScreenControlTypes.ClientInputAction _inputData)
        {
        }

        protected virtual void InitialiseCursor()
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
    }
}