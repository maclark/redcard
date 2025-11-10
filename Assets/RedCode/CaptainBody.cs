using UnityEngine;

namespace RedCard {

    public class CaptainBody : MonoBehaviour {
        public int teamID = 0;
        public bool hasBall;
        public Transform eyes;

        Vector3 initialDirection;

        private void Awake() {
            initialDirection = transform.forward;
        }


        public void SetEyeColor(Color c) {
            MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < mrs.Length; i++) {
                mrs[i].materials[0].color = c;
            }
        }

        public void FocusTransform(CoinFlipProtocol protocol, Transform target) {
            Vector3 toTarget = target.position - eyes.position;
            Vector3 planarToTarget = new Vector3(toTarget.x, 0f, toTarget.z);
            float planarDistance = toTarget.magnitude;
            Vector3 planarDir = toTarget / planarDistance;


            Vector3 faceThisDirection;
            Vector3 lookThisDirection;
            if (planarDistance < protocol.eyeRange) {
                faceThisDirection = toTarget;
                lookThisDirection = toTarget;
            }
            else {
                faceThisDirection = initialDirection;
                lookThisDirection = faceThisDirection;
            } 

            float angleChange = Vector3.SignedAngle(transform.forward, faceThisDirection, Vector3.up);
            float fullPossibleTurn = Mathf.Sign(angleChange) * Time.deltaTime * protocol.captainTurnSpeed;
            float turn = Mathf.Abs(fullPossibleTurn) > Mathf.Abs(angleChange) ? angleChange : fullPossibleTurn; 
            transform.Rotate(Vector3.up, turn, Space.World);
            // #TODO it's just snapping right now
            Debug.DrawLine(eyes.transform.position, eyes.transform.position + lookThisDirection, Color.magenta);
            eyes.rotation = Quaternion.LookRotation(lookThisDirection, Vector3.up);
        }
    }
}
