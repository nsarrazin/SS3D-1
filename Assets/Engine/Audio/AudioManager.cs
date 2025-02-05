using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class AudioManager : MonoBehaviour
    {
        //Welcome to the Singleton, baby.
        public static AudioManager Instance { get; private set; }
        
        [Tooltip("The audio source to be spawned.")]
        public GameObject audioSourcePrefab;
        [Tooltip("The number of audio sources to start out with.")]
        public int minAudioSources = 30;
        [Tooltip("Any more than this will be considered too many audio sources.")]
        public int maxAudioSources = 100;
        [Tooltip("How often (in minutes) do we purge unused audio sources?")]
        [SerializeField]private int purgeFrequency = 30;
        [Tooltip("The full list of audio sources -- just a helpful indicator of how many there are.")]
        public List<AudioSource> audioSources;

        private void Awake()
        {
            InitializeSingleton();
        }
        
        private void Start()
        {
            CreateAudioSourceIndex();
            StartCoroutine(PurgeCountdown());
        }

        /// <summary>
        /// Grabs an unused audio source and plays an audio clip at the desired location.
        /// </summary>
        public void PlayAudioSource(AudioClip audioClip, Vector3 position)
        {
            PlayAudioSource(audioClip, position, gameObject);
        }

        /// <summary>
        /// Grabs a free audio source and parents it to a specific object before playing it.
        /// </summary>
        public void PlayAudioSource(AudioClip audioClip, GameObject parent)
        {
            PlayAudioSource(audioClip, parent.transform.position, parent);
        }
    
        /// <summary>
        /// Plays a sound clip at a position, parent, with specific volume, pitch, and ranges.
        /// Volume, pitch, and ranges are optional.
        /// </summary>
        public void PlayAudioSource(AudioClip audioClip, Vector3 position, GameObject parent, float volume = 0.7f, float pitch = 1f, float minRange = 1f, float maxRange = 500f)
        {
            var audioSource = FindAvailableAudioSource();
            audioSource.gameObject.transform.position = position;
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.minDistance = minRange;
            audioSource.maxDistance = maxRange;
            //If we want to attach the audio source to something specific, do that. Otherwise, detach it from any parents.
            //This is useful for things that are obviously creating the sound, like a mouse's squeak -- we don't want the mouse to leave the squeak behind as it travels, but a flying soda can making a sound at the site of impact is probably fine.
            audioSource.transform.parent = parent == null ? null : parent.transform;
            audioSource.Play();
        }

        /// <summary>
        /// Plays a sound clip using a specific audio source at a position, parent, with specific volume, pitch, and ranges.
        /// Volume, pitch, and ranges are optional. 
        /// </summary>
        public void PlayAudioSourceSpecific(AudioSource audioSource, AudioClip audioClip, Vector3 position, GameObject parent, float volume = 0.7f, float pitch = 1f, float minRange = 1f, float maxRange = 500f)
        {
            audioSource.gameObject.transform.position = position;
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.minDistance = minRange;
            audioSource.maxDistance = maxRange;
            //If we want to attach the audio source to something specific, do that. Otherwise, detach it from any parents.
            //This is useful for things that are obviously creating the sound, like a mouse's squeak -- we don't want the mouse to leave the squeak behind as it travels, but a flying soda can making a sound at the site of impact is probably fine.
            audioSource.transform.parent = parent == null ? null : parent.transform;
            audioSource.Play();            
        }

        /// <summary>
        /// Finds an available audio source in the list of available audio sources. If one does not exist, it creates one.
        /// </summary>
        public AudioSource FindAvailableAudioSource()
        {
            AudioSource validSource = null;
            
            //If there are no audio sources in our list, fix that.
            if (audioSources.Count == 0)
            {
                CreateNewAudioSource();
            }
            
            //Check the list for an audio source that isn't being used.
            foreach (AudioSource source in audioSources)
            {
                if (!source.isPlaying)
                {
                    //If we found one, exit the foreach loop.
                    validSource = source;
                    break;
                }
            }

            //If we have gone through the list and there's no available ones...
            if (validSource == null)
            {
                CreateNewAudioSource();
                //And let's use it.
                validSource = audioSources[audioSources.Count - 1];
            }
            
            return validSource;
        }

        /// <summary>
        /// Creates a list of audio sources, and instantiates our minimum number.
        /// </summary>
        private void CreateAudioSourceIndex()
        {
            audioSources = new List<AudioSource>();
            
            //Instantiate the minimum number of audio sources.
            for (int i = 0; i < minAudioSources; i++)
            {
                CreateNewAudioSource();
            }
        }
        
        /// <summary>
        /// Creates a new audio source and adds it to the list.
        /// </summary>
        private void CreateNewAudioSource()
        {
            var newAudioSource = Instantiate(audioSourcePrefab, gameObject.transform.position, Quaternion.identity).GetComponent<AudioSource>();
            newAudioSource.transform.parent = gameObject.transform;
            audioSources.Add(newAudioSource);
        }
        
        /// <summary>
        /// Destroys all audio sources that aren't currently playing and removes them from the list.
        /// </summary>
        public void PurgeUnusedAudioSources()
        {
            foreach (var source in audioSources)
            {
                //Check that the audio source is idle, and we have more than our minimum number.
                if (!source.isPlaying && audioSources.Count > minAudioSources)
                {
                    audioSources.Remove(source);
                    Destroy(source.gameObject);
                }
            }
            //Restart purge countdown!
            StartCoroutine(PurgeCountdown());
        }

        /// <summary>
        /// Makes a Singleton occur, and makes sure there's no others like it. Badass!
        /// </summary>
        private void InitializeSingleton()
        {
            // We don't want duplicates because this is a motherfuckin' Singleton, baby.
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Waits a specific amount of time before purging unused audio sources.
        /// </summary>
        IEnumerator PurgeCountdown()
        {
            // We want our wait period in minutes, so multiply by 60.
            yield return new WaitForSeconds(purgeFrequency * 60);
            
            //Before we purge them, do we have too many audio sources?
            if (audioSources.Count > maxAudioSources)
            {
                // Now that it's time to purge them, eliminate all of those pesky audio sources.
                Debug.Log(purgeFrequency + " minute(s) have elapsed. There are " + audioSources.Count + "/" + maxAudioSources + " audio sources in our list. Purging audio sources...");
                PurgeUnusedAudioSources();
                
            }
            else
            {
                Debug.Log(purgeFrequency + " minute(s) have elapsed. There are " + audioSources.Count + "/" + maxAudioSources + " audio sources in our list. No purge necessary.");
                StartCoroutine(PurgeCountdown());
            }
        }
        
    }

