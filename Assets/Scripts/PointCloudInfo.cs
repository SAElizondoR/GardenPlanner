using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TMPro;


public class PointCloudInfo : MonoBehaviour
{
    public TextMeshProUGUI Log;
    private ARPointCloud _pointCloud;

    void OnEnable() {
        _pointCloud = GetComponent<ARPointCloud>();
        _pointCloud.updated += OnPointCloudChanged;
    }

    void OnDisable() {
        _pointCloud.updated -= OnPointCloudChanged;
    }

    private void OnPointCloudChanged(ARPointCloudUpdatedEventArgs eventArgs)
    {
        if (!_pointCloud.positions.HasValue ||
            !_pointCloud.identifiers.HasValue ||
            !_pointCloud.confidenceValues.HasValue)
            return;
        
        var positions = _pointCloud.positions.Value;
        if (positions.Length == 0)
            return;
        var identifiers = _pointCloud.identifiers.Value;
        var confidenceValues = _pointCloud.confidenceValues.Value;

        var logText = "Number of points: " + positions.Length +
            "\nPoint info: x = " + positions[0].x + ", y = " + positions[0].y +
            "z = " + positions[0].z + "\n Identifier = " + identifiers[0] +
            ", Confidence = " + confidenceValues[0];
        
        if (Log)
        {
            Log.SetText(logText);
        }
        else
        {
            {
                Debug.Log(logText);
            }
        }
    }
}
