using UnityEngine;

namespace RedCard {

    public class Coin : MonoBehaviour {

        public enum State {
            Flipping,
            HeadsUp,
            TailsUp,
            OnItsSide,
            TurningOverToHeadsUp,
            TurningOverToTailsUp,
        }

        public AnimationCurve turnOverCurve;
        public float turnOverOverSpeed = 360f;
        public float turnOverAngle;
        public Rigidbody rb;
        public AudioSource aso;
        public State state = State.Flipping;
        public int flipCount = 0;

        public Vector3 tossedFrom;
        public Vector3 tossedDir;
        public float tossedMagnitude;
        public float previousAngle;
        public float accumulatedRotation;
        public bool justFlipped = false;

        private void Awake() {
            Debug.Assert(rb);
            rb.maxAngularVelocity = 200f;
        }

        private void Update() {


            Debug.DrawLine(tossedFrom, tossedFrom + tossedDir * tossedMagnitude, Color.green);

            switch (state) {
                case State.Flipping:
                case State.OnItsSide:
                    if (!justFlipped && Mathf.Approximately(rb.linearVelocity.sqrMagnitude, 0f) && Mathf.Approximately(rb.angularVelocity.sqrMagnitude, 0f)) {
                        float dot = Vector3.Dot(Vector3.up, transform.up);
                        if (Mathf.Abs(dot) < .05f) {
                            if (state != State.OnItsSide) Debug.LogWarning("coin its side!");
                            state = State.OnItsSide;
                        }
                        else if (dot > 0f) {
                            print("landed heads up, flips: " + flipCount);
                            state = State.HeadsUp;
                        }
                        else {
                            print("landed tails up, flips: " + flipCount);
                            state = State.TailsUp;
                        }
                    }
                    else {
                        float currentAngle = transform.localRotation.eulerAngles.x;

                        // Calculate signed delta between frames
                        float delta = Mathf.DeltaAngle(previousAngle, currentAngle);
                        accumulatedRotation += Mathf.Abs(delta);

                        if (accumulatedRotation >= 180f) {
                            int newFlips = Mathf.FloorToInt(accumulatedRotation / 180f);
                            flipCount += newFlips;
                            accumulatedRotation -= newFlips * 180f;
                        }

                        previousAngle = currentAngle;
                    }

                    if (transform.position.y < 0f) {
                        Debug.LogWarning("coin fell to below y=0, " + transform.position);
                        transform.position = new Vector3(0f, .1f, 0f);
                    }

                    justFlipped = false; // bc first frame after flipping, the rb hasn't updated yet
                    break;

                case State.TurningOverToHeadsUp:
                    turnOverAngle -= Time.deltaTime * turnOverOverSpeed;
                    float t = 180f - turnOverAngle;
                    float angle0 = Mathf.Lerp(180f, 0f, turnOverCurve.Evaluate(t / 180f));
                    if (turnOverAngle < 0f) {
                        turnOverAngle = 360f;
                        state = State.HeadsUp;
                        print("turned over to heads up");
                    }
                    transform.localRotation = Quaternion.Euler(angle0, 0f, 0f);
                    break;
                case State.TurningOverToTailsUp:
                    turnOverAngle -= Time.deltaTime * turnOverOverSpeed;
                    if (turnOverAngle < 180f) {
                        turnOverAngle = 180f;
                        state = State.TailsUp;
                        print("turned over to tails up");
                    }
                    float adj = 360f - turnOverAngle;
                    // now adj goes from 0 to 180
                    // as toa goes from 360 to 180
                    // t goes from 0 to 1
                    float angle1 = Mathf.Lerp(360f, 180f, turnOverCurve.Evaluate(adj / 180f));
                    transform.localRotation = Quaternion.Euler(angle1, 0f, 0f);
                    break;
                case State.HeadsUp:
                    turnOverAngle = 360f;
                    transform.localRotation = Quaternion.Euler(turnOverAngle, 0f, 0f);
                    //#nocommit
                    //enabled = false;
                    break;
                case State.TailsUp:
                    turnOverAngle = 180f;
                    transform.localRotation = Quaternion.Euler(turnOverAngle, 0f, 0f);
                    //#nocommit
                    //enabled = false;
                    break;
                default:
                    Debug.LogError("unhandled coin state " + state);
                    break;
            }
        }
    }
}
