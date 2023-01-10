using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class DragToMove : MonoBehaviour
{
    private float modSpeed;

    public void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    public void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        modSpeed = 0.001f;
    }

    // Update is called once per frame
    void Update()
    {
        var activeTouches
            = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
        if (activeTouches.Count < 1 || activeTouches[0].phase != TouchPhase.Moved)
        {
            return;
        }
        var touch = activeTouches[0];
        transform.position = new Vector3(
            transform.position.x + touch.delta.x * modSpeed,
            transform.position.y,
            transform.position.z + touch.delta.y * modSpeed);
    }
}
