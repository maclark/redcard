using UnityEngine;

public class IncreaseSolverIteration : MonoBehaviour
{
    public int iterations = 8;
    public int velocityIterations = 4;


    private void Awake() {
        if (TryGetComponent(out Rigidbody rb)) {
            rb.solverIterations = iterations;
            rb.solverVelocityIterations = velocityIterations;
        }
    }
}
