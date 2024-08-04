using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    void Update()
    {
        checkToggleMute();
    }

    private void checkToggleMute()
    {
        if (!Input.GetKeyDown(KeyCode.M))
        {
            return;
        }

        TMP_InputField[] inputFields = GameObject.FindObjectsOfType<TMP_InputField>();

        if (inputFields.Any(field => field.isFocused))
        {
            return;
        }

        if (audioSource.volume > 0)
        {
            audioSource.volume = 0;
        }
        else
        {
            audioSource.volume = 1;
        }

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
            int r = UnityEngine.Random.Range(i, clips.Length);
            clips[i] = clips[r];
            clips[r] = temp;
        }
    }
}