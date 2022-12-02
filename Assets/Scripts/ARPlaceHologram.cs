using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using TMPro;

[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class ARPlaceHologram : MonoBehaviour
{
    [SerializeField]
    private GameObject _prefabToPlace;
    private ARRaycastManager _raycastManager;
    private ARAnchorManager _anchorManager;
    private List<ARAnchor> _anchors;
    private static readonly List<ARRaycastHit> Hits = new();
    public TextMeshProUGUI Log;

    protected void OnEnable() {
        EnhancedTouchSupport.Enable();
    }

    protected void OnDisable() {
        EnhancedTouchSupport.Disable();
    }

    protected void Awake() {
        _raycastManager = GetComponent<ARRaycastManager>();
        _anchorManager = GetComponent<ARAnchorManager>();
        _anchors = new List<ARAnchor>();
    }

    // Update is called once per frame
    void Update()
    {
        var activeTouches
            = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
        if (activeTouches.Count < 1 ||
            activeTouches[0].phase != TouchPhase.Began)
        {
            return;
        }
        const TrackableType trackableTypes = TrackableType.FeaturePoint |
            TrackableType.PlaneWithinPolygon;
        if (_raycastManager.Raycast(activeTouches[0].screenPosition, Hits,
            trackableTypes))
        {
            CreateAnchor(Hits[0]);
            Debug.Log($"Instantiated on: {Hits[0].hitType}");
        }
    }

    ARAnchor CreateAnchor(in ARRaycastHit hit)
    {
        ARAnchor anchor = null;

        if (hit.trackable is ARPlane plane)
        {
            var planeManager = GetComponent<ARPlaneManager>();
            if (planeManager)
            {
                var oldPrefab = _anchorManager.anchorPrefab;
                _anchorManager.anchorPrefab = _prefabToPlace;
                anchor = _anchorManager.AttachAnchor(plane, hit.pose);
                _anchorManager.anchorPrefab = oldPrefab;
                Debug.Log($"Created anchor attachment for plane (id: " +
                    "{anchor.nativePtr})");
            }
            else
            {
                var instantiatedObject = Instantiate(_prefabToPlace,
                    hit.pose.position, hit.pose.rotation);
                anchor = instantiatedObject.GetComponent<ARAnchor>();
                if (anchor == null)
                {
                    anchor = instantiatedObject.AddComponent<ARAnchor>();
                }
                Debug.Log($"Created regular anchor (id: {anchor.nativePtr})");
            }
        }

        return anchor;
    }
}
