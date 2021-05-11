using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Frame {
    public bool keyFrame = false;
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public class JsonMoveClip
{
    public string name = "None";
    public Frame[] frames;
    public JsonMoveClip(MoveClip mc)
    {
        name = mc.gameObject.name;
        frames = mc.frames;
    }
    public void apply(MoveClip[] mcs)
    {
        foreach (var mc in mcs)
        {
            if (mc.gameObject.name == name)
            {
                mc.frames = frames;
                return;
            }
        }
    }
}

[System.Serializable]
public class JsonMoveClipCollection
{
    public JsonMoveClip[] moveClips;

    public static JsonMoveClipCollection loadJson(string path)
    {
        var json = System.IO.File.ReadAllText(path);
       return JsonUtility.FromJson<JsonMoveClipCollection>(json);
    }

    public JsonMoveClipCollection(JsonMoveClip[] clips)
    {
        moveClips = clips;
    }

    public JsonMoveClipCollection(MoveClip[] clips)
    {
        moveClips = new JsonMoveClip[clips.Length];
        for (int i = 0; i < moveClips.Length; i++)
        {
            moveClips[i] = new JsonMoveClip(clips[i]);
        }
    }
    public string toJson()
    {
        return JsonUtility.ToJson(this, true);
    }
    public void applyToClips(MoveClip[] mcs)
    {
        foreach (var jmc in moveClips)
        {
            jmc.apply(mcs);
        }
    }
}

public class MoveClip : MonoBehaviour
{
    public Frame[] frames;
    public int current = 0;
    public bool selectable = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void updateFrame(int i)
    {
        current = i;
        if (frames.Length == 0 ) { return; }
        current = Mathf.Clamp(current, 0, frames.Length - 1);
        this.transform.localPosition = frames[current].position;
        this.transform.localRotation = frames[current].rotation;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
