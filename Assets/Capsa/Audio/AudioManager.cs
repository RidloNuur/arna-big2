using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Arna.Runtime;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] musics;
    public AudioClip[] sounds;

    public AudioMixer mixer;
    public AudioSource musicSource;
    public AudioSource soundSource;

    private int currentMusic = -1;

    public static bool IsMuted { get; private set; }

    public const int CARD_FLIP = 0;
    public const int BUTTON_CLICK = 1;

    private const string MASTER_VOLUME = "Master Volume";


    private void Awake()
    {
        RuntimeManager.SetAudioManager(this);
        musicSource.loop = true;
        SetMusic(0);
    }

    public void SetMusic(int id)
    {
        if (id == currentMusic || id >= musics.Length)
            return;

        musicSource.Stop();
        musicSource.clip = musics[id];
        musicSource.Play();
        currentMusic = id;
    }

    public void PlaySound(int id)
    {
        if (id < sounds.Length)
            soundSource.PlayOneShot(sounds[id]);
    }

    public bool ToggleMute()
    {
        IsMuted = !IsMuted;
        mixer.SetFloat(MASTER_VOLUME, IsMuted ? -80 : 0);
        return IsMuted;
    }
}
