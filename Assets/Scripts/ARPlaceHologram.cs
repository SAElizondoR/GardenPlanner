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
    public GameObject panel;
    public GameObject chooserPanel;

    public float y_offset;

    public GameObject placementIndicator;
    public Button putButton;
    public Button removeButton;
    private bool placementPoseIsValid;
    private Pose placementPose;

    [SerializeField]
    private string _prefabToPlaceName;
    private ARRaycastManager _raycastManager;
    private ARAnchorManager _anchorManager;
    private ARPlaneManager _planeManager;
    private ARRemoveHologram _removeHologram;
    private static readonly List<ARRaycastHit> Hits = new();
    [SerializeField]
    private ARPlaceTrackedImages _placeTrackedImages;
    private GameObject _curObject;  // selected object
    private int objectNumber;
    public int counter;

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
        _planeManager.enabled = true;
        _removeHologram = GetComponent<ARRemoveHologram>();
        _prefabToPlaceName = null;
        _curObject = null;
        placementPoseIsValid = false;
        objectNumber = 0;
        counter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.current.ViewportToScreenPoint(
            new Vector3(0.5f, 0.5f));
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
            if (_prefabToPlaceName != null)
            {
                putButton.interactable = true;
            }
        }
        else
        {
            putButton.interactable = false;
            placementIndicator.SetActive(false);
        }
    }

    public void SetPrefab(string prefabName)
    {
        chooserPanel.SetActive(false);
        panel.SetActive(true);

        Debug.Log($"Changed prefab name: {prefabName}");
        _prefabToPlaceName = prefabName;
    }

    public void SetYOffset(float y)
    {
        Debug.Log("SetYOffset is called");
        y_offset = y;
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

    public void PutObject()
    {
        _removeHologram.isDestroying = false;
        var rotation = placementPose.rotation;
        rotation.y += 90;
        _curObject = PhotonNetwork.Instantiate(_prefabToPlaceName,
            Vector3.zero, Quaternion.identity, 0);
        Debug.Log($"Current object: {_curObject.name}, {_curObject.transform.position.y}");
        this.photonView.RPC("SetCurrentObject", RpcTarget.All,
            _curObject.name, objectNumber);
        this.photonView.RPC("PutAnchor", RpcTarget.All,
            _curObject.name, placementPose.position);
        Debug.Log("Sent change message");
    }

    [PunRPC]
    void SetCurrentObject(string oldName, int number)
    {
        foreach (GameObject gameObject
            in GameObject.FindObjectsOfType(typeof(GameObject)))
        {
            if (gameObject.name == oldName)
            {
                Debug.Log($"Found current object: {oldName}");
                gameObject.name = "Object" + number;
                objectNumber = number + 1;
                counter++;
                _curObject = gameObject;
                removeButton.interactable = true;
                break;
            }
        }
    }

    [PunRPC]
    void PutAnchor(string objectName, Vector3 position)
    {
        Debug.Log($"Position: {position}");
        Debug.Log($"Anchor prefab: {_anchorManager.anchorPrefab}");
        var anchor = _anchorManager.AttachAnchor(
            _placeTrackedImages._planes[0],
            new Pose(position, Quaternion.identity));
        Debug.Log("Created anchor attachment for plane (id: " +
           $"{anchor.nativePtr}) at pose");
        GameObject target = anchor.gameObject;
        GameObject trackedImageGameObject
            = _placeTrackedImages.trackedImageGameObject;
        Debug.Log($"Tracked image game object:{trackedImageGameObject}");
        target.transform.SetParent(trackedImageGameObject.transform);

        /* target.transform.localPosition = position;
        target.transform.localRotation = Quaternion.identity;
        target.transform.localScale = trackedImageGameObject.transform.localScale; */
        _curObject.transform.SetParent(target.transform);
        _curObject.transform.localPosition = Vector3.zero;
        // _curObject.transform.localRotation = Quaternion.identity;
        // _curObject.transform.localScale = target.transform.localScale;
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
