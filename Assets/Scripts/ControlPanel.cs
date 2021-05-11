using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPanel : MonoBehaviour
{
    public Transform target;
    public GameObject panel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetUp(OVRInput.Button.Start))
        {
            this.transform.position = target.transform.position;
            this.transform.rotation = target.transform.rotation;
            panel.SetActive(!panel.activeSelf);
        }
    }
}
