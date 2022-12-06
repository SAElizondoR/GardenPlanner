using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

public class RotateScaleObject : MonoBehaviour
{

    private Slider scaleSlider;
    private Slider rotateSlider;

    private GameObject slidersPanel;

    public float scaleMinValue;
    public float scaleMaxValue;
    public int rotMinValue;
    public int rotMaxValue;

    public Ray ray;
    public RaycastHit hit;

    private GameObject hitObject;

    // Start is called before the first frame update
    void Awake()
    {
        scaleSlider = GameObject.Find("ScaleSlider").GetComponent<Slider>();
        scaleSlider.minValue = scaleMinValue;
        scaleSlider.maxValue = scaleMaxValue;

        scaleSlider.onValueChanged.AddListener(ScaleSliderUpdate);


        rotateSlider = GameObject.Find("RotateSlider").GetComponent<Slider>();
        rotateSlider.minValue = rotMinValue;
        rotateSlider.maxValue = rotMaxValue;

        rotateSlider.onValueChanged.AddListener(RotateSliderUpdate);

        slidersPanel = GameObject.Find("SlidersPanel");
        slidersPanel.gameObject.SetActive(false);

        gameObject.GetComponent<BoxCollider>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.GetComponent<BoxCollider>().enabled == false)
        {
            gameObject.GetComponent<BoxCollider>().enabled = true;
        }
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            //slidersPanel.gameObject.SetActive(true);
            ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            if (Physics.Raycast(ray, out hit))
            {
                hitObject = hit.transform.gameObject;
                if (slidersPanel.gameObject.activeSelf)
                {
                    slidersPanel.gameObject.SetActive(false);
                }
                else
                {
                    slidersPanel.gameObject.SetActive(true);
                }
            }
        }
    }

    void ScaleSliderUpdate(float value)
    {
        hitObject.transform.localScale = new Vector3(value, value, value);
    }

    void RotateSliderUpdate(float value)
    {
        hitObject.transform.localEulerAngles = new Vector3(transform.rotation.x, value, transform.rotation.z);
    }
}
