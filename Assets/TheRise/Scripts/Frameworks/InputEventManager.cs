#define MOBILE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UtmostInput;

public class InputEventManager : MonoBehaviour
{
    public static InputEventManager inputEvent;

    InputX inputX;

    // mouse and touch input
    CrossPlatformClick _click;
    
    [HideInInspector]
    public bool isMO = false;
    
    private void Awake()
    {
        if (inputEvent == null)
            inputEvent = this;
        else
        {
            Debug.LogError("Input already exist!!");
            Destroy(this);
        }
    }

    void Start()
    {
        inputX = new InputX();
    }

    //Control input events
    private void Update()
    {
        SetTouch();

        #if !MOBILE
        MouseMoved();
        if(inputX.MouseRightClick())
            MouseRightClick(inputX.MouseAxis());
        if (inputX.MouseLeftClick())
            MouseLeftClick(inputX.MouseAxis());
        if(inputX.MouseLeftRelease())
            MouseLeftRelease();
        if(inputX.MouseMiddleClick())
            MouseMiddleClick();
        if(inputX.MouseMiddleRelease())
            MouseMiddleRelease();
        
        MouseWheelMove();
        //if (Mathf.Abs(inputX.Vertical()) >= .01f || Mathf.Abs(inputX.Horizontal()) >= .01f)
        KeyboardMovePressed();
        
        if (inputX.isSpacePressed())
            PressedSpace();

        if (inputX.isSpaceReleased())
            ReleasedSpace();

        if (inputX.isShiftPressed())
            PressedShift();

        if (inputX.isShiftReleased())
            ReleasedShift();
        
        if(inputX.isPressedKey(KeyCode.E))
            PressedKey(KeyCode.E);
        
        if(inputX.isReleasedKey(KeyCode.E))
            ReleasedKey(KeyCode.E);
        #endif
    }

    void SetTouch()
    {
        if(inputX.SetInputs())
        {
            _click = inputX.GetInput(0);

            if (_click.phase == IPhase.Began)
            {
                TouchStarted(_click);
            }
            else if (_click.phase == IPhase.Moved || _click.phase == IPhase.Stationary)
            {
                TouchProceed(_click);
            }
            else
            {
                TouchEnd(_click);
            }
            
            isMO = true;

            return;
        }

        isMO = false;
    }
    
    public CrossPlatformClick Click
    {
        get
        {
            SetTouch();
            
            return _click;
        }
    }

    public event Action<Vector2> onMouseMoved;
    void MouseMoved()
    {
        if(onMouseMoved != null)
        {
            onMouseMoved(inputX.MouseAxis());
        }
    }

    public event Action<Vector2> onRightClick;
    void MouseRightClick(Vector2 clickPos)
    {
        if (onRightClick != null)
        {
            onRightClick(clickPos);
        }
    }
    
    public event Action<Vector2> onLeftClick;
    public event Action<bool> mouseLeftChanged;
    void MouseLeftClick(Vector2 clickPos)
    {
        if (onLeftClick != null)
        {
            onLeftClick(clickPos);
        }

        if (mouseLeftChanged != null)
        {
            mouseLeftChanged(true);
        }
    }

    void MouseLeftRelease()
    {
        if (mouseLeftChanged != null)
        {
            mouseLeftChanged(false);
        }
    }
    
    public event Action<float> mouseWheelMoved;
    void MouseWheelMove()
    {
        if (mouseWheelMoved != null)
        {
            float wheelAxis = inputX.MouseWheelAxis();
            mouseWheelMoved(wheelAxis);
        }
    }

    public event Action<bool> mouseWheelClick;
    void MouseMiddleClick()
    {
        if (mouseWheelClick != null)
        {
            mouseWheelClick(true);
        }
    }
    
    void MouseMiddleRelease()
    {
        if (mouseWheelClick != null)
        {
            mouseWheelClick(false);
        }
    }
    
    public event Action<Vector2> onKeyboardMove; 
    void KeyboardMovePressed()
    {
        if(onKeyboardMove != null)
        {
            Vector2 moveVec = new Vector2(inputX.Horizontal(), inputX.Vertical());

            onKeyboardMove(moveVec);
        }
    }

    public event Action onPressingSpace; 
    void PressingSpace()
    {
        if(onPressingSpace != null)
        {
            onPressingSpace();
        }
    }

    public event Action onPressedShift; 
    void PressedShift()
    {
        if (onPressedShift != null)
        {
            onPressedShift();
        }
    }

    public event Action onReleasedShift; 
    void ReleasedShift()
    {
        if (onReleasedShift != null)
        {
            onReleasedShift();
        }
    }

    public event Action onPressedSpace;
    void PressedSpace()
    {
        if (onPressedSpace != null)
        {
            onPressedSpace();
        }
    }

    public event Action onReleasedSpace; 
    void ReleasedSpace()
    {
        if (onReleasedSpace != null)
        {
            onReleasedSpace();
        }
    }

    public event Action<KeyCode> onPressedKey;
    void PressedKey(KeyCode id)
    {
        if (onPressedKey != null)
        {
            onPressedKey(id);
        }
    }
    
    
    public event Action<KeyCode> onReleasedKey;
    void ReleasedKey(KeyCode id)
    {
        if (onReleasedKey != null)
        {
            onReleasedKey(id);
        }
    }

    public event Action<CrossPlatformClick> onTouchStarted; 
    void TouchStarted(CrossPlatformClick click)
    {
        if (onTouchStarted != null) //if any other object is using this event
        {
            onTouchStarted(click);
        }
    }

    public event Action<CrossPlatformClick> onTouch;
    void TouchProceed(CrossPlatformClick click)
    {
        if (onTouch != null) //if any other object is using this event
        {
            onTouch(click);
        }
    }

    public event Action<CrossPlatformClick> onTouchEnd;
    void TouchEnd(CrossPlatformClick click)
    {
        if (onTouchEnd != null) //if any other object is using this event
        {
            onTouchEnd(click);
        }
    }
    
}
