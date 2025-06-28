using UnityEngine;
using UnityEngine.Video;

public class VideoLoopReverse : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    private bool isReversing = false;

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.Play();
    }

    void Update()
    {
        if (!videoPlayer.isPlaying)
        {
            if (!isReversing)
            {
                // 到正播末尾，开始倒放
                isReversing = true;
                videoPlayer.playbackSpeed = -1f;
                videoPlayer.frame = (long)videoPlayer.frameCount - 1;
                videoPlayer.Play();
            }
            else
            {
                // 到倒放开头，重新正放
                isReversing = false;
                videoPlayer.playbackSpeed = 1f;
                videoPlayer.frame = 0;
                videoPlayer.Play();
            }
        }
    }
}