using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ARPlaceTrackedImages : MonoBehaviour
{
    ARTrackedImageManager _trackedImagesManager;
    public GameObject[] ArPrefabs;
    private readonly Dictionary<string, GameObject> _instantiatedPrefabs
        = new();
    public TextMeshProUGUI Log;
    
    void Awake()
    {
        _trackedImagesManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        _trackedImagesManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        _trackedImagesManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(
        ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            var imageName = trackedImage.referenceImage.name;

            foreach (var curPrefab in ArPrefabs)
            {
                if (string.Compare(curPrefab.name, imageName,
                    StringComparison.Ordinal) == 0 &&
                    !_instantiatedPrefabs.ContainsKey(imageName))
                {
                    var newPrefab = Instantiate(curPrefab, trackedImage.transform);
                    _instantiatedPrefabs.Add(imageName, newPrefab);
                    // _instantiatedPrefabs[imageName] = newPrefab;
                    Debug.Log($"{Time.time} -> Instantiated prefab for " +
                        $"tracked image (name: {imageName}).\n" +
                        $"newPrefab.transform.parent.name: " +
                        $"{newPrefab.transform.parent.name}.\n" +
                        $"guid: {trackedImage.referenceImage.guid}");
                    ShowAndroidToastMessage("Instantiated!");
                }
            }
        }

        foreach (var trackedImage in eventArgs.updated)
        {
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
        }
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
