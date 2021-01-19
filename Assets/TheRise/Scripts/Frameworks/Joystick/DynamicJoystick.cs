using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UtmostInput;

namespace JoystickMovement
{
    public class DynamicJoystick : Joystick
    {
        public float MoveThreshold
        {
            get { return moveThreshold; }
            set { moveThreshold = Mathf.Abs(value); }
        }

        [SerializeField] private float moveThreshold = 1;

        protected override void Start()
        {
            MoveThreshold = moveThreshold;
            base.Start();
            background.gameObject.SetActive(false);
        }

        public override void OnMoveStart(CrossPlatformClick click)
        {
            base.OnMoveStart(click);
            background.anchoredPosition = ScreenPointToAnchoredPosition(click.currentPosition);
            background.gameObject.SetActive(true);
        }

        public override void OnMoveEnd(CrossPlatformClick click)
        {
            base.OnMoveEnd(click);
            background.gameObject.SetActive(false);
        }

        protected override void HandleInput(float magnitude, Vector2 normalised)
        {
            if (magnitude > moveThreshold)
            {
                Vector2 difference = normalised * (magnitude - moveThreshold) * radius;
                background.anchoredPosition += difference;
            }

            base.HandleInput(magnitude, normalised);
        }
    }
}
