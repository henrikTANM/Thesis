using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundFX : MonoBehaviour
{
    [SerializeField] private List<Audio> audioClips;
    [SerializeField] private AudioSource audioSource;

    public static SoundFX instance;
    public enum AudioType
    {
        GAME_BUTTON,
        SIDE_BUTTON,
        MENU_OPEN,
        MENU_SELECT,
        MENU_ENTER,
        MENU_EXIT,
        MENU_ACTION,
        MENU_MINUS,
        MENU_PLUS,
        NOTIFICATION,
        WARNING
    }

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public static void PlayAudioClip(AudioType audioType, float volume = 1.0f)
    {
        AudioClip audioclip = instance.audioClips.Find(audio => audio.audioType.Equals(audioType)).audioClip;
        instance.audioSource.PlayOneShot(audioclip, volume);
    }

    [Serializable]
    public class Audio
    {
        public AudioClip audioClip;
        public AudioType audioType;
    }
}
