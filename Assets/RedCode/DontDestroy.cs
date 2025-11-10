using UnityEngine;

namespace RedCard
{

    public class DontDestroy : MonoBehaviour {

        void Awake() {
            DontDestroyOnLoad(gameObject);
        }

    }

}
