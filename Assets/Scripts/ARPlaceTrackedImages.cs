using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using TMPro;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ARPlaceTrackedImages : MonoBehaviourPunCallbacks
{
    private ARTrackedImageManager _trackedImagesManager;
    // private GameObject curObject;
    // private readonly Dictionary<string, GameObject> _instantiatedPrefabs
    //     = new();
    // private bool isReady;
    // public bool isTracking;
    public GameObject trackedImageGameObject;
    private ARSessionOrigin _sessionOrigin;
    [SerializeField]
    private ARPlaneManager _planeManager;
    public List<ARPlane> _planes;
    private bool planeDetected;
    
    void Awake()
    {
        _planes = new List<ARPlane>();
        planeDetected = false;
        _trackedImagesManager = GetComponent<ARTrackedImageManager>();
        _sessionOrigin = GetComponent<ARSessionOrigin>();
        _planeManager = GetComponent<ARPlaneManager>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        _trackedImagesManager.trackedImagesChanged += OnTrackedImagesChanged;
        _planeManager.planesChanged += PlanesChanged;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        _trackedImagesManager.trackedImagesChanged -= OnTrackedImagesChanged;
        _planeManager.planesChanged -= PlanesChanged;
    }

    private void FixedUpdate()
    {
        /* if (trackedImage)
        {
            for (int i = 1; i < trackedImage.transform.childCount; i++)
            {
                trackedImage.transform.GetChild(i).gameObject.SetActive(isTracking);
            }
        } */
    }

    private void PlanesChanged(ARPlanesChangedEventArgs args)
    {
        foreach (var plane in args.added)
        {
            _planes.Add(plane);
        }
        foreach (var plane in args.removed)
        {
            _planes.Remove(plane);
        }
        planeDetected = _planes.Count > 0 ? true : false;
    }

    private void OnTrackedImagesChanged(
        ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            Debug.Log($"Tracking image: {trackedImage.name}");
            trackedImageGameObject = GameObject.Find(trackedImage.name);
            _sessionOrigin.MakeContentAppearAt(trackedImageGameObject.transform, Vector3.zero, Quaternion.identity);
            Debug.Log("Image tracked!");
            StartCoroutine(WaitPlane());
            /*
            if (!curObject) {
                Debug.Log("Photon instantiating...");
                curObject = PhotonNetwork.Instantiate("cat", trackedImageGameObject.transform.position, Quaternion.identity, 0);
                Debug.Log("Photon instantiated");
            }
            curObject.name = "cat";
            gameObject.transform.SetParent(trackedImageGameObject.transform);
            gameObject.transform.localPosition = Vector3.zero; */
            // isReady = true;
            /* var imageName = trackedImage.referenceImage.name;

            foreach (var curPrefab in ArPrefabs)
            {
                if (string.Compare(curPrefab.name, imageName,
                    StringComparison.Ordinal) == 0 &&
                    !_instantiatedPrefabs.ContainsKey(imageName))
                {
                    var newPrefab = Instantiate(curPrefab, trackedImage.transform);
                    _instantiatedPrefabs.Add(imageName, newPrefab);
                    _instantiatedPrefabs[imageName] = newPrefab;
                    Debug.Log($"{Time.time} -> Instantiated prefab for " +
                        $"tracked image (name: {imageName}).\n" +
                        $"newPrefab.transform.parent.name: " +
                        $"{newPrefab.transform.parent.name}.\n" +
                        $"guid: {trackedImage.referenceImage.guid}");
                    isTracking = true;
                    ShowAndroidToastMessage("Instantiated!");
                }
            } */
        }

        /* foreach (var image in eventArgs.updated)
        {
            trackedImage = image;
            _instantiatedPrefabs[trackedImage.referenceImage.name]
                .SetActive(trackedImage.trackingState ==
                TrackingState.Tracking);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            Destroy(_instantiatedPrefabs[trackedImage.referenceImage.name]);
            _instantiatedPrefabs.Remove(trackedImage.referenceImage.name);
            Debug.Log($"Removed (guid: " +
                $"{trackedImage.referenceImage.guid}).");
        } */
    }

    private IEnumerator WaitPlane()
    {
        while (planeDetected == false)
        {
            Debug.Log("Waiting plane...");
            yield return new WaitForSeconds(1);
        }
        Debug.Log("Plane detected");
        ShowAndroidToastMessage("Sync ready!");
    }

    private static void ShowAndroidToastMessage(string message)
    {
#if UNITY_ANDROID
        using var unityPlayer = new AndroidJavaClass(
            "com.unity3d.player.UnityPlayer");
        var unityActivity = unityPlayer.GetStatic<AndroidJavaObject>(
            "currentActivity");
        if (unityActivity == null)
            return;
        var toastClass = new AndroidJavaClass("android.widget.Toast");
        unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            using var toastObject = toastClass.CallStatic<AndroidJavaObject>(
                "makeText", unityActivity, message, 1);
            toastObject.Call("show");
        }));
#endif
    }
}
