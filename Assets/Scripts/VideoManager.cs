using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoManager : MonoBehaviour
{
    public UnityEngine.Video.VideoPlayer player;
    public AnimManager anim;
    public UnityEngine.UI.Text sizeText;
    public string videoName = "";
    // Start is called before the first frame update
    void Start()
    {
        if (videoName.Length >0 )
        {
            player.url = System.IO.Path.Combine(Application.streamingAssetsPath, videoName);
            player.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //player.Stop();
        //player.Play();
        if (sizeText != null) { sizeText.text = "Player Size:" + this.transform.localScale.x; }
        if (player == null) { return; }
        player.time = anim.currentFrame * 1.0f / anim.frameRate;
        //player.StepForward();
    }

    public void toggle()
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }

    public void sizeUp()
    {
        
        this.transform.localScale = new Vector3(this.transform.localScale.x + 0.1f, this.transform.localScale.y + 0.1f, this.transform.localScale.z + 0.1f);
        if (player == null) { this.GetComponent<CharacterController>().Move(new Vector3(0, 0.2f, 0)); }
    }

    public void sizeDown()
    {
        this.transform.localScale = new Vector3(this.transform.localScale.x - 0.1f, this.transform.localScale.y - 0.1f, this.transform.localScale.z - 0.1f);
        if (player == null) { this.GetComponent<CharacterController>().Move(new Vector3(0, 0.2f, 0)); }

    }
}
