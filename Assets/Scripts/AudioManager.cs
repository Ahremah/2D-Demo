using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public Sound[] sounds;

    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.loop = s.loop;
            s.source.playOnAwake = false;

            ApplyVolume(s);
        }
    }

    private void Start()
    {
        Play("Music");
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound not found: " + name);
            return;
        }

        float categoryVolume = s.isMusic ? musicVolume : sfxVolume;

        s.source.volume = s.volume * categoryVolume * masterVolume;
        s.source.pitch = s.pitch;

        if (s.isMusic)
        {
            if (!s.source.isPlaying)
                s.source.Play();

            return;
        }


        s.source.PlayOneShot(s.clip, s.source.volume);
    }

    private void ApplyVolume(Sound s)
    {
        float categoryVolume = s.isMusic ? musicVolume : sfxVolume;

        s.source.volume = s.volume * categoryVolume * masterVolume;
        s.source.pitch = s.pitch;
    }


    public void SetMasterVolume(float v) => masterVolume = v;
    public void SetSFXVolume(float v) => sfxVolume = v;
    public void SetMusicVolume(float v) => musicVolume = v;
}