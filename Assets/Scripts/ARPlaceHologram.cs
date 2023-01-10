using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.EventSystems;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class ARPlaceHologram : MonoBehaviourPunCallbacks
{
    public GameObject placementIndicator;
    // private bool placementIndicatorEnabled;
    private bool placementPoseIsValid;
    private Pose placementPose;

    [SerializeField]
    private string _prefabToPlaceName;
    private ARRaycastManager _raycastManager;
    private ARAnchorManager _anchorManager;
    private ARPlaneManager _planeManager;
    // private List<ARAnchor> _anchors;
    private static readonly List<ARRaycastHit> Hits = new();
    [SerializeField]
    private ARPlaceTrackedImages _placeTrackedImages;
    private GameObject _curObject;  // selected object
    // private List<GameObject> _placedObjects;
    // private ARSessionOrigin _sessionOrigin;
    // if a button to put an object has been selected
    // private bool puttingObject;
    /* [Serializable]
    public struct NamedPrefab {
        public string name;
        public GameObject prefab;
    }
    public NamedPrefab[] _prefabsToPlace;
    private IDictionary<string, GameObject> _prefabsDict; */

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
        // _sessionOrigin = GetComponent<ARSessionOrigin>();
        _planeManager.enabled = true;
        // _anchors = new List<ARAnchor>();
        _prefabToPlaceName = null;
        _curObject = null;
        // _placedObjects = new List<GameObject>();
        // puttingObject = false;
        // placementIndicatorEnabled = true;
        placementPoseIsValid = false;
        // _prefabsDict = _prefabsToPlace.ToDictionary(item => item.name, item => item.prefab);
    }

    // Update is called once per frame
    void Update()
    {
        /* if (placementIndicatorEnabled == false)
        {
            return;
        } */
        UpdatePlacementPose();
        UpdatePlacementIndicator();
        /* if (_raycastManager.Raycast(activeTouches[0].screenPosition, Hits,
            trackableTypes))
        {
            if (_planeManager.enabled)
            {
                Debug.Log("Placing object...");
                PlaceObject(Hits[0]);
            }
            Debug.Log($"Instantiated on: {Hits[0].hitType}");
        } */
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.current.ViewportToScreenPoint(
            new Vector3(0.5f, 0.5f));
        /* var activeTouches
            = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
        // if no active touches have begun or we are not putting an object
        if (activeTouches.Count < 1 || activeTouches[0].phase !=
            TouchPhase.Began || puttingObject == false)
        {
            return;
        } */
        const TrackableType trackableTypes = TrackableType.PlaneWithinPolygon;
        placementPoseIsValid = _raycastManager.Raycast(
            screenCenter, Hits, trackableTypes);
        if (placementPoseIsValid)
        {
            placementPose = Hits[0].pose;
            // set rotation from camera perspective
            var cameraForward = Camera.current.transform.forward;
            var cameraBearing
                = new Vector3(cameraForward.x, cameraForward.y, 0).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }

    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(
                placementPose.position, placementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
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

    /* void PlaceObject(in ARRaycastHit hit)
    {
        if (hit.trackable is ARPlane plane)
        {
            if (!_curObject)
            {
                 GameObject trackedImageGameObject
                    = _placeTrackedImages.trackedImageGameObject;
                Debug.Log(
                    $"Tracked image game object:{trackedImageGameObject}");
                _curObject = PhotonNetwork.Instantiate(_prefabToPlaceName,
                    hit.pose.position, hit.pose.rotation, 0);
                Debug.Log($"Current object: {_curObject}");
                Debug.Log($"Position: {hit.pose.position}, " +
                        $"rotation: {hit.pose.rotation}");
                _curObject.transform.SetParent(trackedImageGameObject.transform);
            }
            else
            {
                _curObject.transform.position = hit.pose.position;
            }
        }
    }

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
                _curObject = PhotonNetwork.Instantiate(
                    _prefabToPlaceName, Vector3.zero,
                    Quaternion.identity, 0);
                Debug.Log($"Current object: {_curObject}");
                Debug.Log($"Position: {hit.pose.position}, " +
                    $"rotation: {hit.pose.rotation}");
                this.photonView.RPC("PutPrefabInstance", RpcTarget.Others,
                    _prefabToPlaceName, hit.pose.position, hit.pose.rotation);
                var oldPrefab = _anchorManager.anchorPrefab;
                _anchorManager.anchorPrefab = _curObject;
                anchor = _anchorManager.AttachAnchor(plane, hit.pose);
                _anchorManager.anchorPrefab = oldPrefab;
                Debug.Log("Created anchor attachment for plane (id: " +
                     $"{anchor.nativePtr})");
                GameObject target = anchor.gameObject;
                target.transform.SetParent(trackedImageGameObject.transform);
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
            //    Debug.Log($"Created regular anchor
            //     (id: {anchor.nativePtr})");
            //}
            return anchor;
        }

        _curObject = PhotonNetwork.Instantiate(
                    _prefabToPlaceName, Vector3.zero,
                    Quaternion.identity, 0);
        Debug.Log($"Current object (out of plane): {_curObject}");
        this.photonView.RPC("PutPrefabInstance", RpcTarget.Others,
        _prefabToPlaceName, hit.pose.position, hit.pose.rotation);
        anchor = ComponentUtils.GetOrAddIf<ARAnchor>(_curObject, true);
                
        return anchor;
    } */

    public void SetPrefab(string prefabName)
    {
        Debug.Log($"Changed prefab name: {prefabName}");
        _prefabToPlaceName = prefabName;
        // puttingObject = true;
    }

    /* [PunRPC]
    public void PutPrefabInstance(string prefabName, Vector3 position,
        Quaternion rotation)
    {
        Debug.Log($"Position: {position}, rotation: {rotation}");
        foreach (GameObject gameObject
            in GameObject.FindObjectsOfType(typeof(GameObject)))
        {
            if (gameObject.name == prefabName + "(Clone)")
            {
                Debug.Log($"Found clone {prefabName}");
                _curObject = gameObject;
                // curObject.transform.parent
                //     = _placeTrackedImages.trackedImageGameObject.transform;
                // _placedObjects.Add(curObject);
                break;
            }
        }
        var oldPrefab = _anchorManager.anchorPrefab;
        _anchorManager.anchorPrefab = _curObject;
        ARAnchor anchor = _anchorManager.AttachAnchor(
            _placeTrackedImages._planes[0], new Pose(position, rotation));
        Debug.Log("Created anchor attachment for plane (id: " +
            $"{anchor.nativePtr}) at pose");
        _anchorManager.anchorPrefab = oldPrefab;
        // _curObject.transform.SetParent(anchor.transform);
        GameObject target = anchor.gameObject;
        target.transform.SetParent(
            _placeTrackedImages.trackedImageGameObject.transform);
    } */

    public void TogglePlaneDetection()
    {
        Debug.Log("Change plane detection");
        _planeManager.enabled = !_planeManager.enabled;

        foreach (ARPlane plane in _planeManager.trackables)
        {
            plane.gameObject.SetActive(_planeManager.enabled);
        }
    }

    public void PutObject()
    {
        _curObject = PhotonNetwork.Instantiate(_prefabToPlaceName,
            Vector3.zero, Quaternion.identity, 0);
        this.photonView.RPC("PutAnchor", RpcTarget.All,
            _curObject.name, placementPose.position, placementPose.rotation);
        Debug.Log("Sent change message");
    }

    [PunRPC]
    void PutAnchor(string objectName, Vector3 position, Quaternion rotation)
    {
        Debug.Log($"Position: {position}, " +
                        $"rotation: {rotation}");
        foreach (GameObject gameObject
            in GameObject.FindObjectsOfType(typeof(GameObject)))
        {
            if (gameObject.name == objectName)
            {
                Debug.Log($"Found object to anchor: {objectName}");
                _curObject = gameObject;
                break;
            }
        }
        // _anchorManager.anchorPrefab = _prefabsDict[objectName];
        Debug.Log($"Anchor prefab: {_anchorManager.anchorPrefab}");
        var anchor = _anchorManager.AttachAnchor(_placeTrackedImages._planes[0], new Pose(position, rotation));
        Debug.Log("Created anchor attachment for plane (id: " +
           $"{anchor.nativePtr}) at pose");
        _curObject.transform.SetPositionAndRotation(position, rotation);
        _curObject.transform.SetParent(anchor.gameObject.transform);
        GameObject target = anchor.gameObject;
        GameObject trackedImageGameObject
            = _placeTrackedImages.trackedImageGameObject;
        Debug.Log($"Tracked image game object:{trackedImageGameObject}");
        target.transform.SetParent(trackedImageGameObject.transform);
        /* _curObject.transform.SetParent(_placeTrackedImages.trackedImageGameObject.transform);
        if (_curObject.GetComponent<ARAnchor>() == null)
        {
            _curObject.AddComponent<ARAnchor>();
            Debug.Log("Anchor added!");
        } */
        // var oldPrefab = _anchorManager.anchorPrefab;
        /* _anchorManager.anchorPrefab = _curObject;
        ARAnchor anchor = _anchorManager.AttachAnchor(
           _placeTrackedImages._planes[0], new Pose(position, rotation));
        Debug.Log("Created anchor attachment for plane (id: " +
           $"{anchor.nativePtr}) at pose");
        GameObject target = anchor.gameObject;
        target.transform.SetParent(
            _placeTrackedImages.trackedImageGameObject.transform); */
    }

    /* [PunRPC]
    public void UpdateObject(string objectName, Vector3 position,
        Quaternion rotation)
    {
        Debug.Log($"Position: {position}, rotation: {rotation}");
        foreach (GameObject gameObject
            in GameObject.FindObjectsOfType(typeof(GameObject)))
        {
            if (gameObject.name == objectName)
            {
                Debug.Log($"Found object {objectName}");
                gameObject.transform.position = position;
                gameObject.transform.rotation = rotation;
                Debug.Log("Changed transform");
                break;
            }
        }
    } */
}
