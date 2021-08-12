using System;
using UnityEngine;

public class InputController
{
#if UNITY_EDITOR
    private IInputHandlerBase inputHandler = new MouseHandler();
#elif UNITY_ANDROID
    private IInputHandlerBase inputHandler = new TouchHandler();
#endif 
    
    public bool IsInputDown => inputHandler.IsInputDown;
    public bool IsInputUp => inputHandler.IsInputUp;
    public bool IsInputHeld => inputHandler.IsInputHeld;
    public Vector2 InputPosition => inputHandler.InputPosition;
    public Vector2 InputWorldPositionVec2 => Camera.main.ScreenToWorldPoint(inputHandler.InputPosition);

}

public interface IInputHandlerBase
{
    bool IsInputDown { get; }
    bool IsInputUp { get; }
    bool IsInputHeld { get; }
    Vector2 InputPosition { get; }
}

public class MouseHandler : IInputHandlerBase
{
    bool IInputHandlerBase.IsInputDown => Input.GetMouseButtonDown(0);
    bool IInputHandlerBase.IsInputUp => Input.GetMouseButtonUp(0);
    bool IInputHandlerBase.IsInputHeld => Input.GetMouseButton(0);
    Vector2 IInputHandlerBase.InputPosition => Input.mousePosition;
}

public class TouchHandler : IInputHandlerBase
{
    bool IInputHandlerBase.IsInputDown => CheckTouchPhase(TouchPhase.Began);
    bool IInputHandlerBase.IsInputUp => CheckTouchPhase(TouchPhase.Ended);
    bool IInputHandlerBase.IsInputHeld => CheckTouchPhase(TouchPhase.Moved) || CheckTouchPhase(TouchPhase.Stationary);
    Vector2 IInputHandlerBase.InputPosition => Input.GetTouch(0).position;

    private bool CheckTouchPhase(TouchPhase phase)
    {
        if (Input.touchCount == 0) return false;
        switch (phase)
        {
            case TouchPhase.Began:
                return Input.GetTouch(0).phase == TouchPhase.Began;
            case TouchPhase.Ended:
                return Input.GetTouch(0).phase == TouchPhase.Ended;
            case TouchPhase.Moved:
                return Input.GetTouch(0).phase == TouchPhase.Moved;
            case TouchPhase.Stationary:
                return Input.GetTouch(0).phase == TouchPhase.Stationary;
        }
        return false;
    }
}
