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
public class ARPlaceHologram : MonoBehaviour
{
    [SerializeField]
    private string _prefabToPlaceName;
    private ARRaycastManager _raycastManager;
    private ARAnchorManager _anchorManager;
    private ARPlaneManager _arPlaneManager;
    private List<ARAnchor> _anchors;
    private static readonly List<ARRaycastHit> Hits = new();
    private Button _planeButton;
    [SerializeField]
    private ARPlaceTrackedImages _placeTrackedImages;

    protected void OnEnable() {
        EnhancedTouchSupport.Enable();
    }

    protected void OnDisable() {
        EnhancedTouchSupport.Disable();
    }

    protected void Awake() {
        _raycastManager = GetComponent<ARRaycastManager>();
        _anchorManager = GetComponent<ARAnchorManager>();
        _arPlaneManager = GetComponent<ARPlaneManager>();
        _arPlaneManager.enabled = true;
        _anchors = new List<ARAnchor>();

        _planeButton = GameObject.Find("PlaneButton").GetComponent<Button>();
        _planeButton.onClick.AddListener(TogglePlaneDetection);
    }

    // Update is called once per frame
    void Update()
    {
        var activeTouches
            = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
        // Debug.Log($"Active touches: {activeTouches.Count}");
        if (activeTouches.Count < 1 || activeTouches[0].phase != TouchPhase.Began)
        {
            return;
        }
        if (IsPointOverUIObject(activeTouches[0].screenPosition, activeTouches[0].finger.index)) {
            return;
        }

        const TrackableType trackableTypes = TrackableType.AllTypes;
        if (_raycastManager.Raycast(activeTouches[0].screenPosition, Hits,
            trackableTypes))
        {
            if (_arPlaneManager.enabled)
            {
                CreateAnchor(Hits[0]);
            }
            Debug.Log($"Instantiated on: {Hits[0].hitType}");
        }
    }

    private bool IsPointOverUIObject(Vector2 pos, int fingerId)
    {
        Debug.Log("Checking if pointer tr UI");
        if (EventSystem.current.IsPointerOverGameObject(fingerId))
        {
            Debug.Log("Not in UI element");
            return false;
        }

        PointerEventData eventPosition = new PointerEventData(EventSystem.current);
        eventPosition.position = new Vector2(pos.x, pos.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventPosition, results);
        Debug.Log($"UI Element: {results.Count > 0}");
        return results.Count > 0;
    }

    ARAnchor CreateAnchor(in ARRaycastHit hit)
    {
        ARAnchor anchor = null;

        if (hit.trackable is ARPlane plane)
        {
            if (_arPlaneManager)
            {
                GameObject trackedImageGameObject = _placeTrackedImages.trackedImageGameObject;
                Debug.Log($"Tracked image game object: {trackedImageGameObject}");
                var oldPrefab = _anchorManager.anchorPrefab;
                GameObject curObject = null;
                foreach (GameObject gameObject in GameObject.FindObjectsOfType(typeof(GameObject)))
                {
                    if (gameObject.name == _prefabToPlaceName + "(Clone)")
                    {
                        Debug.Log($"Found clone {_prefabToPlaceName}");
                        curObject = gameObject;
                    }
                }
                Debug.Log($"Current object: {curObject}");
                if (!curObject)
                {
                    Debug.Log("Photon instantiating...");
                    curObject = PhotonNetwork.Instantiate(_prefabToPlaceName, trackedImageGameObject.transform.position, Quaternion.identity, 0);
                    Debug.Log("Photon instantiated!");
                }
                _anchorManager.anchorPrefab = curObject;
                anchor = _anchorManager.AttachAnchor(plane, hit.pose);
                // _anchorManager.anchorPrefab = oldPrefab;
                Debug.Log("Created anchor attachment for plane (id: " +
                    $"{anchor.nativePtr})");
                GameObject target = anchor.gameObject;
                target.transform.SetParent(trackedImageGameObject.transform);
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
        _prefabToPlaceName = prefabName;
    }

    public void TogglePlaneDetection()
    {
        _arPlaneManager.enabled = !_arPlaneManager.enabled;

        foreach (ARPlane plane in _arPlaneManager.trackables)
        {
            plane.gameObject.SetActive(_arPlaneManager.enabled);
        }
    }
}
