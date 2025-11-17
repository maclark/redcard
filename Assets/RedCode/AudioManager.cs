using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace RedCard {

    public enum AudioGroup {
        SFX,
        Voices,
        Music,
    }

    public class AudioManager : MonoBehaviour {

        public AudioMixer mixer;
        public AudioMixerGroup sfxGroup;
        public AudioMixerGroup voicesGroup;
        public AudioMixerGroup musicGroup;
        public AudioSource sfxAso;
        public AudioSource musicAso;

        public static AudioManager am;

        private List<AudioSource> sfxPool = new List<AudioSource>();
        private int sfxIndex = 0;

        void Awake() {
            if (am != null && am != this) {
                Destroy(gameObject);
                return;
            }

            am = this;
            Debug.Assert(sfxAso);
            DontDestroyOnLoad(gameObject);
            //GrowSFXPool(20);
        }

        public static void PlaySFXOneShot(AudioClip clip) {
            if (am) am.sfxAso.PlayOneShot(clip);
        }

        public void Play(AudioClip clip, AudioGroup group) {
            AudioSource aso = null;
            switch (group) {
                case AudioGroup.SFX:
                    aso = sfxPool[sfxIndex];
                    sfxIndex = (sfxIndex + 1) % sfxPool.Count;
                    break;
                case AudioGroup.Voices:
                    break;
                case AudioGroup.Music:
                    break;
            }

            if (aso) {

            }
            else Debug.LogError("no audio source found");
        }


        private AudioSource GrowSFXPool(int toAdd) {
            for (int i = 0; i < toAdd; i++) {
                var src = gameObject.AddComponent<AudioSource>();
                src.outputAudioMixerGroup = sfxGroup;
                sfxPool.Add(src);
            }

            AudioSource aso = sfxPool[sfxIndex];
            sfxIndex = (sfxIndex + 1) % sfxPool.Count;
            return aso;
        }
    }
}
