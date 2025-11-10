using UnityEngine;
using System.Collections.Generic;

namespace RedCard {

    public class FoamSprayParticles : MonoBehaviour {

        public AnimationCurve distanceToScaleCurve;
        public float minScale = .025f;
        public float maxScale = .15f;
        public float maxScaleAfterRandom = .2f;
        public float maxSqrDistance = 1.25f;
        public float scaleRandomness = .1f;
        public float penetration = .25f;
        private RefControls refControls;
        private ParticleSystem ps;
        private List<ParticleCollisionEvent> particleEvents = new List<ParticleCollisionEvent>();

        private void Awake() {
            refControls = FindAnyObjectByType<RefControls>();
            ps = GetComponent<ParticleSystem>();
            Debug.Assert(refControls != null);
            Debug.Assert(ps != null);
        }

        private void OnParticleCollision(GameObject other) {
            int numEvents = ps.GetCollisionEvents(other, particleEvents);
            for (int i = 0; i < numEvents; ++i) {

                Vector3 point = particleEvents[i].intersection;
                Vector3 velocity = particleEvents[i].velocity;
                float sqrDist = (point - transform.position).sqrMagnitude;

                float targetScale = Mathf.Lerp(maxScale, minScale, distanceToScaleCurve.Evaluate(sqrDist / maxSqrDistance));
                targetScale = targetScale * (1 + Random.Range(-scaleRandomness, scaleRandomness));
                point += velocity.normalized *  penetration * targetScale;
                refControls.SpawnFoamBlob(point, Mathf.Lerp(maxScale, minScale, sqrDist / maxSqrDistance));

                //print("hit: " + other.name + ", at sqrDistance: " + sqrDist + ", targetScale: " + targetScale);
            }
        }
    }

}
