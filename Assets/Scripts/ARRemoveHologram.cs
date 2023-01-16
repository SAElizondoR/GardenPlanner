using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using Photon.Pun;
using Photon.Realtime;

public class ARRemoveHologram : MonoBehaviourPunCallbacks
{
    public bool isDestroying;
    public Button removeButton;
    private ARPlaceHologram _placeHologram;

    public override void OnEnable() {
        base.OnEnable();
        EnhancedTouchSupport.Enable();
    }

    public override void OnDisable() {
        base.OnDisable();
        EnhancedTouchSupport.Disable();
    }

    private void Awake() {
        isDestroying = false;
        _placeHologram = GetComponent<ARPlaceHologram>();
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
        Ray ray = Camera.current.ScreenPointToRay(
            activeTouches[0].screenPosition);
        if (Physics.Raycast(ray, out hit, 20))
        {
            this.photonView.RPC("DestroyObject", RpcTarget.MasterClient,
                hit.transform.gameObject.name);
            // PhotonNetwork.Destroy(hit.transform.gameObject);
            this.photonView.RPC("DecreaseCounter", RpcTarget.All);
            isDestroying = false;
        }
    }

    [PunRPC]
    void DestroyObject(string name)
    {
        foreach (GameObject gameObject
            in GameObject.FindObjectsOfType(typeof(GameObject)))
        {
            if (gameObject.name == name)
            {
                PhotonNetwork.Destroy(gameObject);
                break;
            }
        }
    }

    [PunRPC]
    void DecreaseCounter()
    {
        _placeHologram.counter--;
        if (_placeHologram.counter == 0)
        {
            removeButton.interactable = false;
        }
    }

    public void SetDestroy()
    {
        isDestroying = true;
    }
}
