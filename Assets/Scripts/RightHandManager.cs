using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightHandManager : MonoBehaviour
{
    public AnimManager am;
    public Transform rightHandAnchor;
    public Transform rightHandChildAnchor;

    public float prevTriggerVal = 0;

    public float prevGripVal = 0;

    // 0 means only move position,
    // 1 means only rotate
    // 2 means position and rotation
    public int mode = 0;
    string[] modeDescription = {"Current mode: translation", "Current mode: rotation", "Current mode: translation&rotation" };

    public bool dragging = false;

    private Vector3 copiedPos = new Vector3();
    private Quaternion copiedRot = Quaternion.identity;

    public UnityEngine.UI.Text indicateUI;
    // Start is called before the first frame update
    void Start()
    {
        if (am == null) {
            am = GameObject.Find("AnimManager").GetComponent<AnimManager>();
        }
    }
    public Vector3 rayPointClosestPos(Vector3 org, Vector3 dir, Vector3 point)
    {
        return org + dir * Vector3.Dot(dir, point - org);
    }

    public void findNewTarget()
    {
        MoveClip tar = null;
        float distance = float.MaxValue;
        foreach (var mc in am.clipPool)
        {
            //var dis = Vector3.Distance(mc.transform.position, rightHandAnchor.transform.position);
            var closePoint = rayPointClosestPos(rightHandAnchor.transform.position, rightHandAnchor.transform.forward, mc.transform.position);
            
            var dis = Vector3.Distance(mc.transform.position, closePoint);

            if (Vector3.Dot(closePoint - rightHandAnchor.transform.position, rightHandAnchor.transform.forward) < 0)
            {
                dis *= 3;
            }
            if (dis < distance && mc.selectable)
            {
                 distance = dis;
                tar = mc;
            }
            
        }

        am.target = tar;
        am.updateAll();
    }

    public void copyTransform()
    {
        if (am.target == null) { return; }
        copiedPos = am.target.transform.position;
        copiedRot = am.target.transform.rotation;
    }

    public void pasteTransform()
    {
        if (am.target == null) { return; }
        am.target.transform.position = copiedPos;
        am.target.transform.rotation = copiedRot;
        if (am.target.GetComponent<CCD_IK>())
        {
            var ik = am.target.GetComponent<CCD_IK>();
            for (int i = 0;i < ik.rotateIterations;i++)
            {
                ik.updateOnce();
            }
        }
    }

    public void dragStart()
    {
        if (am.target == null) { return; }
        rightHandChildAnchor.transform.position = am.target.transform.position;
        rightHandChildAnchor.transform.rotation = am.target.transform.rotation;
        dragging = true;
        if (am.target.GetComponent<CCD_IK>())
        {
            am.target.GetComponent<CCD_IK>().updating = true;
        }
    }

    public void dragEnd()
    {
        dragging = false;
        if (am.target.GetComponent<CCD_IK>())
        {
            am.target.GetComponent<CCD_IK>().updating = false;
        }
    }


    // Update is called once per frame
    void Update()
    {
        Vector2 thumStickMove = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
        if (thumStickMove.y > 0.8f)
        {
            copyTransform();
            OVRInput.SetControllerVibration(1, 0.2f, OVRInput.Controller.RTouch);
        }

        else if (thumStickMove.y < -0.8f)
        {
            pasteTransform();
            OVRInput.SetControllerVibration(1, 0.2f, OVRInput.Controller.RTouch);
        }
        else {
            OVRInput.SetControllerVibration(1, 0f, OVRInput.Controller.RTouch);
        }

        var triggerVal = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger);
        if (triggerVal < 0.5f && prevTriggerVal >= 0.5f)
        {
            findNewTarget();
        }

        prevTriggerVal = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger); ;

        var gripVal = OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger); 
        if (gripVal > 0.2f && prevGripVal <= 0.2f)
        {
            dragStart();
        }

        if (gripVal <= 0.2f && dragging)
        {
            dragEnd();
        }

        if (dragging && am.target != null)
        {
            if (mode == 0 || mode == 2)
            {
                am.target.transform.position = rightHandChildAnchor.transform.position;
            }
            if (mode == 1 || mode == 2)
            {
                am.target.transform.rotation = rightHandChildAnchor.transform.rotation;
            }
        }
        if (indicateUI != null) { indicateUI.text = modeDescription[mode]; }
        prevGripVal = OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger);

        if (OVRInput.GetUp(OVRInput.RawButton.RThumbstick)) {
            mode = (mode + 1 )  % 3;
        }
    }
}
