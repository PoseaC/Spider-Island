using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string soundName; //name for the sound for searching in the array
        public AudioClip clip; //the actual sound
        [Range(0f, 1f)] public float volume; //volume for the sound
        [Range(.1f, 3f)] public float pitch; //pitch of the sound
        public bool loop; //if the sound should loop or not

        [HideInInspector] //no need to show the source in the inspector, it is added within code
        public AudioSource source; //source to play the sound
    }

    public Sound[] sounds; //an array of sounds to play
    public static AudioManager instance; //we use only one audio manager, otherwise background music will cut between scene transitions
    private void Awake()
    {
        if (instance == null) //check if we don't already have an audio manager, if not we assign one
            instance = this;
        else
        {
            Destroy(gameObject); //if we do we destroy the other audio manager as to not have two different sound sources
            return;
        }
        DontDestroyOnLoad(gameObject); //tell the inspector to keep this object between scenes
        foreach(Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop; //assign the audio source all atributes we have
        }
    }

    public void ChangeSFXVolume()
    {
        foreach (Sound sound in sounds)
        {
            sound.source.volume = PlayerPrefs.GetFloat("SfxVolume", 0.3f);
        }
    }
    public void Play(string name)
    {
        Sound sound = Array.Find<Sound>(sounds, sounds => sounds.soundName == name); //search the list of sounds for the one we need
        if (sound == null)
        {
            Debug.LogWarning("Sound " + name + " not found"); //throw a warning if the sound wasn't found
            return;
        }
        sound.source.Play(); //play the sound
    }
}
