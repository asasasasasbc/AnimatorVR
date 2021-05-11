using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OVR;



public class AnimManager : MonoBehaviour
{
    public static AnimManager instance;
    public int currentFrame = 0;
    public int maxFrame = 0;

    public bool startUpdating = false;
    public float frameTimer = 0;
    public int frameRate = 30;
    public bool playing = false;
    public MoveClip target;
    public GameObject targetCursor;

    public MoveClip[] clipPool;
    public GameObject root;

    public Image[] timelineFrameUI;
    public Text[] timelineMarkers;
    public Image timelineCursor;
    public int timelineLeft = 0;
    private Color greyColor;
    private Color whiteColor;

    public bool updateClipPool = false;
    public CCD_IK[] ikPool;

    public UnityEngine.UI.Text debugText;

    private float movingTimer = 0f;

    public string baseFolder = "";
    public string targetFileName = "test.json";

    //public bool grabing = false;
    // Start is called before the first frame update
    void Start()
    {
        baseFolder = Application.dataPath + "/";
        #if UNITY_EDITOR
        baseFolder = baseFolder.Substring(0, baseFolder.Length - 7);
        #endif
        greyColor = timelineFrameUI[0].color;
        whiteColor = timelineFrameUI[1].color;

        if (updateClipPool)
        {
            MoveClip[] mcs = Object.FindObjectsOfType<MoveClip>();
            List<MoveClip> clips = new List<MoveClip>();
            foreach (var mc in mcs)
            {
                if (mc.gameObject.activeInHierarchy)
                {
                    clips.Add(mc);
                }
            }
            clipPool = clips.ToArray();
        }

        foreach (var mc in clipPool)
        {
            if (mc.frames.Length == 0)
            {
                mc.frames = new Frame[maxFrame + 1];
                for (int j = 0; j < mc.frames.Length;j++)
                {
                    mc.frames[j] = new Frame();
                    mc.frames[j].position = mc.transform.localPosition;
                    mc.frames[j].rotation = mc.transform.localRotation;
                }
                mc.frames[0].keyFrame = true;
            }

        }
        
    }
    /// <summary>
    ///  Key the target moveClip's status to its current frame
    /// </summary>
    public void key()
    {
        if (target == null) { return; }
        keyTarget(target);
        if (target.gameObject.GetComponent<CCD_IK>() != null)
        {
            var ik = target.gameObject.GetComponent<CCD_IK>();
            keyTarget(ik.bone0.GetComponent<MoveClip>());
            keyTarget(ik.bone1.GetComponent<MoveClip>());
            keyTarget(ik.bone2.GetComponent<MoveClip>());
        }
        updateAll();
    }

    public void keyTarget(MoveClip target)
    {
        if (target == null) { return; }
        var fs = target.frames;
        fs[currentFrame].position = target.transform.localPosition;
        fs[currentFrame].rotation = target.transform.localRotation;
        fs[currentFrame].keyFrame = true;
        //Update all the left frames before it
        Frame leftKeyFrame = null;
        int j = 0;
        for (j = currentFrame - 1; j >= 0; j--)
        {
            if (fs[j].keyFrame)
            {
                leftKeyFrame = fs[j];
                break;
            }
        }
        if (leftKeyFrame == null) // fill left part of the new key frame
        {
            for (int k = 0; k < currentFrame; k++)
            {
                fs[k].position = fs[currentFrame].position;
                fs[k].rotation = fs[currentFrame].rotation;
            }
        }
        else
        {
            for (int k = j + 1; k < currentFrame; k++)
            {
                float ratio = (1.0f * k - j) / (currentFrame - j);
                fs[k].position = Vector3.Lerp(leftKeyFrame.position, fs[currentFrame].position, ratio);
                fs[k].rotation = Quaternion.Lerp(leftKeyFrame.rotation, fs[currentFrame].rotation, ratio);
            }
        }

        //Update all the right frames after it:
        //Update all the left frames before it
        Frame rightKeyFrame = null;
        j = 0;
        for (j = currentFrame + 1; j < fs.Length; j++)
        {
            if (fs[j].keyFrame)
            {
                rightKeyFrame = fs[j];
                break;
            }
        }
        if (rightKeyFrame == null) // fill left part of the new key frame
        {
            for (int k = currentFrame + 1; k < fs.Length; k++)
            {
                fs[k].position = fs[currentFrame].position;
                fs[k].rotation = fs[currentFrame].rotation;
            }
        }
        else
        {
            for (int k = currentFrame + 1; k < j; k++)
            {
                float ratio = (1.0f * k - currentFrame) / (j - currentFrame);
                fs[k].position = Vector3.Lerp(fs[currentFrame].position, rightKeyFrame.position, ratio);
                fs[k].rotation = Quaternion.Lerp(fs[currentFrame].rotation, rightKeyFrame.rotation, ratio);
            }
        }
    }

    /// <summary>
    ///  Clear current frame's keyframe
    /// </summary>
    public void clear()
    {
        clearTarget(target);
        if (target.gameObject.GetComponent<CCD_IK>() != null)
        {
            var ik = target.gameObject.GetComponent<CCD_IK>();
            clearTarget(ik.bone0.GetComponent<MoveClip>());
            clearTarget(ik.bone1.GetComponent<MoveClip>());
            clearTarget(ik.bone2.GetComponent<MoveClip>());
        }
        updateAll();
    }

    public void clearTarget(MoveClip target)
    {
        if (target == null) { return; }
        var fs = target.frames;
        if (fs[currentFrame].keyFrame == false) { return; }
        fs[currentFrame].keyFrame = false;

        Frame leftKeyFrame = null;
        int leftIndex = 0, rightIndex = 0;
        int j = 0;
        for (j = currentFrame - 1; j >= 0; j--)
        {
            if (fs[j].keyFrame)
            {
                leftKeyFrame = fs[j];
                leftIndex = j;
                break;
            }
        }
        Frame rightKeyFrame = null;
        j = 0;
        for (j = currentFrame + 1; j < fs.Length; j++)
        {
            if (fs[j].keyFrame)
            {
                rightKeyFrame = fs[j];
                rightIndex = j;
                break;
            }
        }
        /////////if no keyframe found...
        if (leftKeyFrame == null && rightKeyFrame == null)
        {
            return;
        }
        else if (leftKeyFrame != null && rightKeyFrame == null)
        {
            for (int k = leftIndex + 1; k < fs.Length; k++)
            {
                fs[k].position = leftKeyFrame.position;
                fs[k].rotation = leftKeyFrame.rotation;
            }
        }
        else if (leftKeyFrame == null && rightKeyFrame != null)
        {
            for (int k = 0; k < rightIndex; k++)
            {
                fs[k].position = rightKeyFrame.position;
                fs[k].rotation = rightKeyFrame.rotation;
            }
        }
        else
        {
            for (int k = leftIndex + 1; k < rightIndex; k++)
            {
                float ratio = (1.0f * k - leftIndex) / (rightIndex - leftIndex);
                fs[k].position = Vector3.Lerp(leftKeyFrame.position, rightKeyFrame.position, ratio);
                fs[k].rotation = Quaternion.Lerp(leftKeyFrame.rotation, rightKeyFrame.rotation, ratio);
            }
        }


    }
    /// <summary>
    /// Next frame
    /// </summary>
    public void next(int delta = 1)
    {
        currentFrame = Mathf.Clamp(currentFrame + delta, 0, maxFrame);
        updateAll();
    }

    /// <summary>
    /// Previous frame
    /// </summary>
    public void prev(int delta = 1)
    {
        currentFrame = Mathf.Clamp(currentFrame - delta, 0, maxFrame);
        updateAll();
    }
    /// <summary>
    /// Set current frame to 0.
    /// </summary>
    public void replay()
    {
        currentFrame = 0;
        updateAll();
    }

    /// <summary>
    /// Update all move clip
    /// </summary>
    public void updateAll()
    {
        while (currentFrame - timelineLeft >= timelineFrameUI.Length)
        {
            timelineLeft += 10;
            for (int k = 0; k < timelineMarkers.Length; k++)
            {
                timelineMarkers[k].text = "" + (timelineLeft + 10 * k);
            }
        }

        while (currentFrame < timelineLeft)
        {
            timelineLeft -= 10;
            for (int k = 0; k < timelineMarkers.Length; k++)
            {
                timelineMarkers[k].text = "" + (timelineLeft + 10 * k);
            }
        }
        foreach (var ik in ikPool)
        {
            if (ik.gameObject != target.gameObject)
            { ik.updating = false; }
           
        }
        foreach (var mc in clipPool)
        {
            //if (mc == target && grabing) { continue; }
            mc.updateFrame(currentFrame);
        }
        if (target != null)
        {
            for (int k = 0; k < timelineFrameUI.Length; k++)
            {
                var c = whiteColor;
                if (k % 2 == 0) {c = greyColor; }

                if (k + timelineLeft < target.frames.Length)
                {
                    if (target.frames[k + timelineLeft].keyFrame)
                    {
                        c = new Color(1, c.g, c.b);
                    }
                }
                timelineFrameUI[k].color = c;
            }
        }
        else {
            for (int k = 0; k < timelineFrameUI.Length; k++)
            {
                if (k % 2 == 0) { timelineFrameUI[k].color = greyColor; }
                else { timelineFrameUI[k].color = whiteColor; }
            }
        }

        Vector3 pos = timelineCursor.transform.position;
       
        //pos.x = timelineFrameUI[currentFrame - timelineLeft].transform.position.x;
        timelineCursor.transform.position = timelineFrameUI[currentFrame - timelineLeft].transform.position;
    }

    /// <summary>
    /// start playing
    /// </summary>
    public void play()
    {
        if (playing) { playing = false;return; }
        playing = true;

        if (currentFrame >= maxFrame) { currentFrame = 0; }
        if (maxFrame == 0) { playing = false; }
        updateAll();
    }
    /// <summary>
    ///  Handle oculus VR input
    /// </summary>
    public void vrInput()
    {
        //VR input

        //Press A to key
        if (OVRInput.GetDown(OVRInput.RawButton.A)) { key(); }

        //Press A to clear
        if (OVRInput.GetDown(OVRInput.RawButton.B)) { clear(); }

        //Press X to play
        if (OVRInput.GetDown(OVRInput.RawButton.X)) { play(); }

        //Press Y to replay
        if (OVRInput.GetDown(OVRInput.RawButton.Y)) { replay(); }

       

        //Press left thumb stick to move current frame:
        Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        
        if (primaryAxis.x > -0.2f && primaryAxis.x < 0.2f || OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) >= 0.5f || OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) >= 0.5f)
        {
            movingTimer = 0;
            OVRInput.SetControllerVibration(1, 0, OVRInput.Controller.LTouch);
        } else
        {
            if (movingTimer <= 0.01f) {
                if (primaryAxis.x > 0) { next(); } else { prev(); }
                movingTimer = 0.5f;
                OVRInput.SetControllerVibration(1, 0.2f, OVRInput.Controller.LTouch);
            }
            movingTimer -= Time.deltaTime * 5 * Mathf.Abs(2* primaryAxis.x * primaryAxis.x);
            if (movingTimer  < 0.4f) { OVRInput.SetControllerVibration(1, 0, OVRInput.Controller.LTouch); }
        }

    }

    /// <summary>
    /// Save to a file
    /// </summary>
    public void save(string f )
    {
        if (f != "") { targetFileName = f; }
        var path = baseFolder + targetFileName;
        Debug.Log("File saved to:" + path);

        var clipCollection = new JsonMoveClipCollection(clipPool);
        var json = clipCollection.toJson();
        //Debug.Log("Json file generated<\n" + json + ">");
        System.IO.File.WriteAllText(path, json);
    }


    public string findRelativePathToRoot(MoveClip  current)
    {
        var parent = current.transform.parent;
        var recordedPath = current.gameObject.name;
        while (parent != null)
        {
            if (parent == root.transform)
            {
                return recordedPath;
            }
            recordedPath = parent.name +"/" + recordedPath;
            parent = parent.parent;
        }
        return null;
    }

    /// <summary>
    /// Save as Unity3d's animation format
    /// </summary>
    /// <param name="f"></param>
    public void saveAsAC(string f)
    {
        AnimationClip ac = new AnimationClip();
        ac.frameRate = frameRate;
        ac.legacy = true;
        foreach (var mc in clipPool)
        {
            var objPath = mc.gameObject.name;
            objPath = findRelativePathToRoot(mc);
            if (objPath == null) { continue; }
            AnimationCurve curveX = new AnimationCurve(); 
            AnimationCurve curveY = new AnimationCurve();
            AnimationCurve curveZ = new AnimationCurve();

            AnimationCurve curveRX = new AnimationCurve();
            AnimationCurve curveRY = new AnimationCurve();
            AnimationCurve curveRZ = new AnimationCurve();
            AnimationCurve curveRW = new AnimationCurve();

            for (int i = 0; i < mc.frames.Length;i++)
            {
                var frame = mc.frames[i];
                if (frame.keyFrame)
                {
                    //Keyframe k = new Keyframe();
                    curveX.AddKey(1.0f * i / frameRate, frame.position.x);
                    curveY.AddKey(1.0f * i / frameRate, frame.position.y);
                    curveZ.AddKey(1.0f * i / frameRate, frame.position.z);

                    curveRX.AddKey(1.0f * i / frameRate, frame.rotation.x);
                    curveRY.AddKey(1.0f * i / frameRate, frame.rotation.y);
                    curveRZ.AddKey(1.0f * i / frameRate, frame.rotation.z);
                    curveRW.AddKey(1.0f * i / frameRate, frame.rotation.w);
                }
            }
            ac.SetCurve(objPath, typeof(Transform), "localPosition.x", curveX); 
            ac.SetCurve(objPath, typeof(Transform), "localPosition.y", curveY); 
            ac.SetCurve(objPath, typeof(Transform), "localPosition.z", curveZ); 

            ac.SetCurve(objPath, typeof(Transform), "localRotation.x", curveRX); 
            ac.SetCurve(objPath, typeof(Transform), "localRotation.y", curveRY); 
            ac.SetCurve(objPath, typeof(Transform), "localRotation.z", curveRZ); 
            ac.SetCurve(objPath, typeof(Transform), "localRotation.w", curveRW); ac.EnsureQuaternionContinuity();
        }

        //Editor feature only
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.CreateAsset(ac, "Assets/" + f);
        #endif
    }
    /// <summary>
    /// Load the target json file
    /// </summary>
    public void load(string f )
    {
        if (f != "") { targetFileName = f; }
        var clipCollection = JsonMoveClipCollection.loadJson(baseFolder + targetFileName);
        clipCollection.applyToClips(clipPool);
        updateAll();
    }

    // Update is called once per frame
    void Update()
    {
        if (startUpdating) {
            var pos = timelineCursor.transform.position;
            startUpdating = false; updateAll();
            timelineCursor.transform.position = pos;
        }
        if (targetCursor != null)
        {
            if (target == null)
            {
                targetCursor.SetActive(false);
            }
            else {
                targetCursor.SetActive(true);
                targetCursor.transform.position = target.transform.position;
                targetCursor.transform.rotation = target.transform.rotation;
            }
        }
        if (Input.GetKeyDown(KeyCode.A)) { prev(); if (Input.GetKey(KeyCode.LeftShift)) { prev(4); } }
        if (Input.GetKeyDown(KeyCode.D)) { next(); if (Input.GetKey(KeyCode.LeftShift)) { next(4); } }
        if (Input.GetKeyDown(KeyCode.Space)) { play(); }
        if (Input.GetKeyDown(KeyCode.R)) { replay(); }
        if (Input.GetKeyDown(KeyCode.F)) { key(); }
        if (Input.GetKeyDown(KeyCode.G)) { clear(); }

        if (Input.GetKeyDown(KeyCode.S)) { save(""); }
        if (Input.GetKeyDown(KeyCode.P)) { saveAsAC("test.anim"); }
        if (Input.GetKeyDown(KeyCode.L)) { load(""); }

        vrInput();
        
        debugText.text = "Current Frame:" + currentFrame;

        if (playing)
        {
            frameTimer += Time.deltaTime;
            while (frameTimer > 1f / frameRate)
            {
                currentFrame += 1;
                frameTimer -= 1f / frameRate;
                if (currentFrame > maxFrame) {
                    playing = false;
                    currentFrame = maxFrame;
                    frameTimer = 0;
                    updateAll();
                    break;
                }
                updateAll();
            }
        }
    }
}
