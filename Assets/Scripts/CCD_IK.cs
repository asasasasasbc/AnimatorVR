using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCD_IK : MonoBehaviour
{
    public Transform bone0;
    public Transform bone1;
    public Transform bone2;
    public Transform target;
    public int rotateIterations = 1;
    public bool updating = true;
    public bool updateBone2Rot = false;

    public bool bone1rotYOnly = false;
    public bool bone1rotYLimit = false;
    public bool bone1rotYLimitInverse = false;

    public bool bone1rotXOnly = false;
    public bool bone1rotXLimit = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void step1()
    {
        //Bone 2 is the end bone
        //Bone1-Bone2: make bone1 look at the target
        //bone1.LookAt(target);


        //var bone1lookAt = Quaternion.LookRotation(target.position - bone1.position);
        //bone1.rotation = bone1lookAt;
        //var diffRotation = Quaternion.LookRotation(bone2.localPosition);
        //bone1.localRotation *= Quaternion.Inverse(diffRotation);

        var localToTarget = bone1.InverseTransformPoint(target.position);
        var diffRotation = Quaternion.FromToRotation(bone2.localPosition, localToTarget);
        bone1.localRotation *= diffRotation;
        if (bone1rotYOnly == true)
        {


            var rot = bone1.localEulerAngles;
            var y = rot.y;
            y = y % 360;
            if (bone1rotYLimit && !bone1rotYLimitInverse)
            {
                if ( y > 5 && y < 90) { y = 5; }
                else if (y > 90 && y < 180) { y = 180; }
            }
            if (bone1rotYLimit && bone1rotYLimitInverse)
            {
                if (y > 150 && y < 180) { y = 150; }
                else if (y > 180) { y = 0; }
            }
            bone1.localEulerAngles = new Vector3(0, y, 0);
            
        } else if (bone1rotXOnly == true)
        {
            //My own rotation projection function, project a three-axis rotation to one-axis rotation!
            //Rotate the bone1 to match bone0's rigt axis (x-axis) to make sure bone1 is only rotate around its original x-axis
            var localToParentXaxis = bone1.InverseTransformPoint(bone1.transform.position + bone0.transform.right);
            var rotDiffRotation = Quaternion.FromToRotation(new Vector3(1,0,0), localToParentXaxis);
            bone1.localRotation *= rotDiffRotation;


             var rot = bone1.localRotation.eulerAngles;
             var x = rot.x;
             x= x % 360;
             //Debug.Log(rot);
             if (bone1rotXLimit)
             {

                 if (x > 120 && x < 180) { x = 150; }
                 else if (x > 180) { x = 0; }
             }
            //Have to use rot.y and rot.z maybe 180 instead of 0 because of using Quaternion rotations functions before
            bone1.localEulerAngles = new Vector3(x, rot.y, rot.z);
            
            
        //Debug.Log(bone1.localEulerAngles);
        }

        //var diffRotation = Quaternion.FromToRotation(bone2.position - bone1.position, target.position - bone1.position);
        //var targetRotation = bone1.rotation * diffRotation;
        //bone1.rotation = targetRotation;
        //bone1.rotation = Quaternion.RotateTowards(bone1.rotation, targetRotation, rotateSpeed * Time.deltaTime);

    }

    public void step2()
    {
        //var diffRotation = Quaternion.FromToRotation(bone2.position - bone0.position, target.position - bone0.position);
        //var targetRotation = bone0.rotation * diffRotation;
        //bone0.rotation *= diffRotation;
        var diffRotation = Quaternion.FromToRotation(bone0.InverseTransformPoint(bone2.position), bone0.InverseTransformPoint(target.position));
        bone0.localRotation *= diffRotation;
    }
    public void updateOnce()
    {
        step1();
       step2();
        if (updateBone2Rot) {
            bone2.rotation = target.rotation;
        }
        
       // bone0.rotation = Quaternion.RotateTowards(bone0.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            //Debug.Log("Step 1");
            step1();

        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            step2();

        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            updating = !updating;

        }

        if (updating)
        {
            for (int i = 0; i < rotateIterations;i++)
            {
                updateOnce();
            }
            
        }
       
    }
}
