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
    // private bool isReady;
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
            /* trackedImage.transform.localScale
                = new Vector3(trackedImage.size.x, 1f,
                trackedImage.size.y); */
            Debug.Log($"Tracking image: {trackedImage.name}");
            trackedImageGameObject = GameObject.Find(trackedImage.name);
           trackedImageGameObject.transform.localScale
                = new Vector3(trackedImage.size.x, trackedImage.size.x,
                trackedImage.size.x);
            _sessionOrigin.MakeContentAppearAt(
                trackedImageGameObject.transform, Vector3.zero,
                Quaternion.identity);
            Debug.Log("Image tracked!");
            StartCoroutine(WaitPlane());
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            // Debug.Log("Tracked image updated");
            trackedImageGameObject = GameObject.Find(trackedImage.name);
            /* _sessionOrigin.MakeContentAppearAt(
                trackedImageGameObject.transform, Vector3.zero,
                Quaternion.identity); */
        }
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
