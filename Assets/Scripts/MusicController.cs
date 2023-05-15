using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MusicController : MonoBehaviour
{
    [SerializeField] private AudioClip[] music;
    [SerializeField] private AudioClip[] bridges;
    private AudioSource audioSource;
    private int whatSong = 0;
    private System.Random r = new System.Random();


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        shuffleArray(music);
        StartCoroutine("playSongs");
    }

    private IEnumerator playSongs()
    {
        bool isBridge = false;
        while (true)
        {
            if (whatSong >= music.Length)
            {
                whatSong = 0;
            }
            if (isBridge)
            {
                audioSource.clip = bridges[r.Next(0, bridges.Length)];
            }
            else
            {
                audioSource.clip = music[whatSong];
                whatSong += 1;
            }
            audioSource.Play();
            yield return new WaitForSecondsRealtime(audioSource.clip.length);
            isBridge = !isBridge;
        }
    }

    void shuffleArray(AudioClip[] clips)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            AudioClip temp = clips[i];
            int r = Random.Range(i, clips.Length);
            clips[i] = clips[r];
            clips[r] = temp;
        }
    }
}