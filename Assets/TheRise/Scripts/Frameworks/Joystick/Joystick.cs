using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UtmostInput;

namespace JoystickMovement
{
    public class Joystick : MonoBehaviour
    {
        public bool Active => _active;
        public float Horizontal => _input.x;
        public float Vertical => _input.y;
        public Vector2 Direction => new Vector2(Horizontal, Vertical);
        public Vector2 Delta => _rawInputDelta;
    
        public float HandleRange
        {
            get { return handleRange; }
            set { handleRange = Mathf.Abs(value); }
        }

        public float DeadZone
        {
            get { return deadZone; }
            set { deadZone = Mathf.Abs(value); }
        }
    
        [SerializeField] private float handleRange = 1;
        [SerializeField] private float deadZone = 0;

        [SerializeField] protected RectTransform background = null;
        [SerializeField] protected RectTransform handle = null;
    
        private RectTransform _baseRect = null;
        private Canvas _canvas;
        private Camera _cam;
        private Vector2 _input = Vector2.zero;
        private Vector2 _rawInputDelta = Vector2.zero;
        private bool _active = false;

        protected Vector2 radius = Vector2.zero;

        protected virtual void Start()
        {
            HandleRange = handleRange;
            DeadZone = deadZone;
            _baseRect = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas == null)
                Debug.LogError("The Joystick is not placed inside a canvas");

            Vector2 center = new Vector2(0.5f, 0.5f);
            background.pivot = center;
            handle.anchorMin = center;
            handle.anchorMax = center;
            handle.pivot = center;
            handle.anchoredPosition = Vector2.zero;
        
            radius = background.sizeDelta / 2;
        
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
                _cam = _canvas.worldCamera;
        
            InputEventManager.inputEvent.onTouchStarted += OnMoveStart;
            InputEventManager.inputEvent.onTouch += OnMove;
            InputEventManager.inputEvent.onTouchEnd += OnMoveEnd;
        }

        public virtual void OnMoveStart(CrossPlatformClick click)
        {
            _active = true;
        }
        public virtual void OnMove(CrossPlatformClick click)
        {
            OnDrag(click);
        }
        public virtual void OnMoveEnd(CrossPlatformClick click)
        {
            _input = Vector2.zero;
            _rawInputDelta = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;

            _active = false;
        }
    
        protected virtual void OnDrag(CrossPlatformClick click)
        {
            _rawInputDelta = click.delta;

            //        Vector2 position = RectTransformUtility.WorldToScreenPoint(cam, background.position);

            _input = (ScreenPointToAnchoredPosition(click.currentPosition) - background.anchoredPosition) / (radius * _canvas.scaleFactor);

            HandleInput(_input.magnitude, _input.normalized);
            handle.anchoredPosition = _input * radius * handleRange;
        }

        protected virtual void HandleInput(float magnitude, Vector2 normalized)
        {
            if (magnitude > deadZone)
            {
                if (magnitude > 1)
                    _input = normalized;
            }
            else
                _input = Vector2.zero;
        }

        protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
        {
            Vector2 localPoint = Vector2.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_baseRect, screenPosition, _cam, out localPoint))
            {
                //Vector2 pivotOffset = _baseRect.pivot * _baseRect.sizeDelta;
                return localPoint;// - (background.anchorMax * _baseRect.sizeDelta) + pivotOffset;
            }
            return Vector2.zero;
        }
    }
}
