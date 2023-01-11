using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrefabChooser : MonoBehaviour
{
    public GameObject chooserPanel;
    public GameObject panel;
    // Start is called before the first frame update
    void Start()
    {
        chooserPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnChoosePrefabButton()
    {
        chooserPanel.SetActive(true);
        panel.SetActive(false);
    }
}
