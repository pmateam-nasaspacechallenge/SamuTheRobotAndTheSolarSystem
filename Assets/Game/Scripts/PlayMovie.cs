using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayMovie : MonoBehaviour
{
    public RawImage image;
    public VideoClip videoToPlay;

    private VideoPlayer videoPlayer;
    private VideoSource videoSource;

    private AudioSource audioSource;
    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;
        StartCoroutine(playVideo());
    }

    IEnumerator playVideo()
    {
        videoPlayer = gameObject.AddComponent<VideoPlayer>();

        audioSource = gameObject.AddComponent<AudioSource>();

        videoPlayer.playOnAwake = false;
        audioSource.playOnAwake = false;
        audioSource.Pause();

        videoPlayer.source = VideoSource.VideoClip;
        // videoPlayer.url = "C:\\Users\\Utilizador\\Desktop\\videoTest\\Assets\\Beer Bubbles.mp4";

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetTargetAudioSource(0, audioSource);
        videoPlayer.clip = videoToPlay;
        videoPlayer.Prepare();

        WaitForSeconds waitTime = new WaitForSeconds(1);
        while (!videoPlayer.isPrepared)
        {
            Debug.Log("Preparing Video");

            yield return waitTime;
            break;
        }

        Debug.Log("Done Preparing Video");

        image.texture = videoPlayer.texture;

        videoPlayer.Play();

        audioSource.Play();

        Debug.Log("Playing Video");

        while (videoPlayer.isPlaying)
        {
            Debug.Log("Video Time: " + Mathf.FloorToInt((float)videoPlayer.time));
            yield return null;
        }

        Debug.Log("Done Playing Video");


    }
}
