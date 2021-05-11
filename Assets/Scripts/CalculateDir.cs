using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateDir : MonoBehaviour
{
    public float angle = 0;
    public Transform cam;
    public GameObject enableThisWhenVisible;
    public float visibleAngle = 70;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        angle = Vector3.Angle(cam.position - this.transform.position, transform.forward);
        enableThisWhenVisible.SetActive(angle < visibleAngle);
    }
}
