using UnityEngine;

namespace RedCard {

    public class MenuWhistleBody : MonoBehaviour {
        public AudioClip[] clunks = new AudioClip[0];

        private void OnCollisionEnter(Collision collision) {
            if (clunks.Length > 0) AudioManager.am.sfxAso.PlayOneShot(clunks[Random.Range(0, clunks.Length)]);
            else Debug.LogWarning("missing clunks on menu whistle " + name);
        }
    }
}
