using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using Photon.Pun;
using Photon.Realtime;

public class ARRemoveHologram : MonoBehaviour
{
    private bool isDestroying;

    public void OnEnable() {
        EnhancedTouchSupport.Enable();
    }

    public void OnDisable() {
        EnhancedTouchSupport.Disable();
    }

    private void Awake() {
        isDestroying = false;
    }

    void FixedUpdate()
    {
        if (!isDestroying)
        {
            return;
        }
        RaycastHit hit;
        var activeTouches
            = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
        if (activeTouches.Count < 1 || activeTouches[0].phase !=
            TouchPhase.Began)
        {
            return;
        }
        Ray ray = Camera.current.ScreenPointToRay(activeTouches[0].screenPosition);
        if (Physics.Raycast(ray, out hit, 20))
        {
            PhotonNetwork.Destroy(hit.transform.gameObject);
            isDestroying = false;
        }
    }

    public void SetDestroy()
    {
        isDestroying = true;
    }
}
