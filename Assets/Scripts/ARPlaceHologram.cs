using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.EventSystems;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class ARPlaceHologram : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private string _prefabToPlaceName;
    private ARRaycastManager _raycastManager;
    private ARAnchorManager _anchorManager;
    private ARPlaneManager _planeManager;
    private List<ARAnchor> _anchors;
    private static readonly List<ARRaycastHit> Hits = new();
    private Button _plantsButton;
    private Button _catButton;
    private Button _planeButton;
    [SerializeField]
    private ARPlaceTrackedImages _placeTrackedImages;
    private GameObject _curObject;
    private ARSessionOrigin _sessionOrigin;

    public override void OnEnable() {
        base.OnEnable();
        EnhancedTouchSupport.Enable();
    }

    public override void OnDisable() {
        base.OnDisable();
        EnhancedTouchSupport.Disable();
    }

    protected void Awake() {
        _raycastManager = GetComponent<ARRaycastManager>();
        _anchorManager = GetComponent<ARAnchorManager>();
        _planeManager = GetComponent<ARPlaneManager>();
        _sessionOrigin = GetComponent<ARSessionOrigin>();
        _planeManager.enabled = true;
        _anchors = new List<ARAnchor>();
        _prefabToPlaceName = null;
        _curObject = null;
        _plantsButton = GameObject.Find("PlantsButton").GetComponent<Button>();
        _plantsButton.onClick.AddListener(delegate{SetPrefab("FBX_Corona Variant");});
        _catButton = GameObject.Find("CatButton").GetComponent<Button>();
        _catButton.onClick.AddListener(delegate{SetPrefab("cat");});
        _planeButton = GameObject.Find("PlaneButton").GetComponent<Button>();
        _planeButton.onClick.AddListener(TogglePlaneDetection);
    }

    // Update is called once per frame
    void Update()
    {
        if (_prefabToPlaceName == null)
        {
            return;
        }

        var activeTouches
            = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
        if (activeTouches.Count < 1 || activeTouches[0].phase !=
            TouchPhase.Began)
        {
            return;
        }
        _prefabToPlaceName = null;
        const TrackableType trackableTypes = TrackableType.Planes;
        if (_raycastManager.Raycast(activeTouches[0].screenPosition, Hits,
            trackableTypes))
        {
            if (_planeManager.enabled)
            {
                CreateAnchor(Hits[0]);
            }
            Debug.Log($"Instantiated on: {Hits[0].hitType}");
        }
    }

    /* private bool IsPointOverUIObject(Vector2 pos, int fingerId)
    {
        Debug.Log("Checking if pointer tr UI");
        if (EventSystem.current.IsPointerOverGameObject(fingerId))
        {
            Debug.Log("Not in UI element");
            return false;
        }

        PointerEventData eventPosition = new PointerEventData(
            EventSystem.current);
        eventPosition.position = new Vector2(pos.x, pos.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventPosition, results);
        Debug.Log($"UI Element: {results.Count > 0}");
        return results.Count > 0;
    } */

    ARAnchor CreateAnchor(in ARRaycastHit hit)
    {
        ARAnchor anchor = null;

        if (hit.trackable is ARPlane plane)
        {
            if (_planeManager)
            {
                GameObject trackedImageGameObject
                    = _placeTrackedImages.trackedImageGameObject;
                Debug.Log(
                    $"Tracked image game object:{trackedImageGameObject}");
                // var oldPrefab = _anchorManager.anchorPrefab;
                
                Debug.Log($"Current object: {_curObject}");
                if (_curObject)
                    return null;

                Debug.Log("Photon instantiating...");
                _curObject = PhotonNetwork.Instantiate(_prefabToPlaceName,
                    hit.pose.position, trackedImageGameObject.transform.rotation, 0);
                _curObject.transform.parent = trackedImageGameObject.transform;
                Debug.Log("Photon instantiated!");
                this.photonView.RPC("PutPrefabInstance", RpcTarget.Others, _prefabToPlaceName);
                _anchorManager.anchorPrefab = _curObject;
                anchor = _anchorManager.AttachAnchor(plane, hit.pose);
                // _anchorManager.anchorPrefab = oldPrefab;
                Debug.Log("Created anchor attachment for plane (id: " +
                     $"{anchor.nativePtr})");
                // GameObject target = anchor.gameObject;
                // target.transform.SetParent(trackedImageGameObject.transform);
                // target.transform.localPosition = Vector3.zero;
            }
            //else
            //{
            //    var instantiatedObject = Instantiate(_prefabToPlace,
            //        hit.pose.position, hit.pose.rotation);
            //    anchor = instantiatedObject.GetComponent<ARAnchor>();
            //    if (anchor == null)
            //    {
            //        anchor = instantiatedObject.AddComponent<ARAnchor>();
            //    }
            //    Debug.Log($"Created regular anchor (id: {anchor.nativePtr})");
            //}
        }

        return anchor;
    }

    public void SetPrefab(string prefabName)
    {
        Debug.Log($"Changed prefab name: {prefabName}");
        _prefabToPlaceName = prefabName;
    }

    [PunRPC]
    public void PutPrefabInstance(string prefabName)
    {
        foreach (GameObject gameObject in GameObject.FindObjectsOfType(typeof(GameObject)))
        {
            if (gameObject.name == prefabName + "(Clone)")
            {
                Debug.Log($"Found clone {prefabName}");
                _curObject = gameObject;
                _curObject.transform.parent = _placeTrackedImages.trackedImageGameObject.transform;
                break;
            }
        }

        _anchorManager.anchorPrefab = _curObject;
        ARPlane plane = _placeTrackedImages._plane;
        ARAnchor anchor = _anchorManager.AttachAnchor(plane, new Pose(plane.center, Quaternion.identity));
        Debug.Log("Created anchor attachment for plane (id: " +
            $"{anchor.nativePtr})");
        // GameObject target = anchor.gameObject;
        // target.transform.SetParent(trackedImageGameObject.transform);
    }

    public void TogglePlaneDetection()
    {
        Debug.Log("Change plane detection");
        _planeManager.enabled = !_planeManager.enabled;

        foreach (ARPlane plane in _planeManager.trackables)
        {
            plane.gameObject.SetActive(_planeManager.enabled);
        }
    }
}
