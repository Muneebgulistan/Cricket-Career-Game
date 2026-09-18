using UnityEngine;

namespace CricketGame.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource crowdSource;

        private float masterVolume = 1.0f;
        private float musicVolume = 0.8f;
        private float sfxVolume = 1.0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource != null && clip != null)
            {
                musicSource.clip = clip;
                musicSource.loop = loop;
                musicSource.volume = masterVolume * musicVolume;
                musicSource.Play();
            }
        }

        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip, masterVolume * sfxVolume * volumeScale);
            }
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            if (musicSource != null) musicSource.volume = masterVolume * musicVolume;
        }
    }
}
