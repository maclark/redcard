using UnityEngine;

public class RefBodyCam : MonoBehaviour
{
    public Transform target;
    public float walkAimOffset = 10f;
    public Vector3 offset = Vector3.zero;
    public float mouseSensitivity = 1.0f;

    private void Start() {
        
    }

    private void LateUpdate() {
        if (target) {
            transform.position = target.position + target.up * offset.y + target.right * offset.x + target.forward * offset.z;
            //transform.rotation = target.rotation;
        }
    }
}
