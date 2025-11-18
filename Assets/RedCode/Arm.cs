using UnityEngine;

namespace RedCard {

    public class Arm : MonoBehaviour {

        [Header("ASSIGNATIONS")]
        public Transform limb;
        public MeshRenderer[] folicles = new MeshRenderer[0];
        public MeshRenderer[] nails = new MeshRenderer[0];
        public ShoulderColorer colorer;

        [Header("SETTINGS")]
        public Chirality side;
        public Vector3 localLoweredPos;
        public Vector3 localRaisedPos;
        public float zStart = 1.5f;
        public float zEnd = 2f;
        public float phiThreshold = Mathf.PI;
        public float phiOffset = -Mathf.PI / 3f;

        [Header("VARS")]
        public ArmData data;

        public void Init() {
            SetSkinColor(data.skinColorIndex);
            SetHairColor(data.hairColorIndex);
            UpdateMuscle(); // must be done before hair density so radius is correctly calculated 
            UpdateHairDensity();
            UpdateHairLength();
            UpdateNails();
            //UpdateTattoos();
        }

        public void SetSkinColor(int index) {
            print("setting skin color " + index);
            data.skinColorIndex = index;
            Color c = RedMatch.Match.customizationOptions.GetSkinMeshColor(index);
            for (int i = 0; i < colorer.skin.Length; i++) {
                colorer.skin[i].materials[0].color = c;
                colorer.skin[i].materials[0].color = c;
            }
        }

        public void UpdateNails() {
            //
        }

        public void UpdateHairLength() {
            for (int i = 0; i < folicles.Length; i++) {
                folicles[i].transform.localScale = new Vector3(.0065f, data.hairLength, .0065f);
            }
        }
        
        public void UpdateHairDensity() {
            // circumference times height
            // we are ignoring sides of cylinder
            float radius = limb.localScale.x / 2f; // yes, i checked, the cylinder x value is the diameter
            float surfaceArea = Mathf.PI * radius * (zEnd - zStart);
            int folicleCount = Mathf.RoundToInt(surfaceArea * data.hairThickness);
            print("folicount " + folicleCount);
            for (int i = 0; i < folicles.Length; i++) {
                if (i < folicleCount) {
                    float h = Random.Range(zStart, zEnd);
                    //float phi = i * Mathf.PI * 2f / folicles.Length + phiOffset; 
                    float phi = Random.Range(0, Mathf.PI * 2);// i * Mathf.PI * 2f / folicles.Length + phiOffset; 

                    float x = radius * Mathf.Cos(phi);
                    float z = radius * Mathf.Sin(phi);
                    Vector3 axialPoint = limb.position + limb.up * h;
                    Vector3 point = axialPoint + limb.right * x + limb.forward * z;
                    folicles[i].transform.position = point;
                    folicles[i].gameObject.SetActive(true);
                    folicles[i].transform.rotation = Quaternion.LookRotation(limb.up, point - axialPoint);
                    folicles[i].transform.Rotate(limb.up, data.hairCurl);
                    //if (phi < phiThreshold) {
                    //    folicles[i].transform.Rotate(limb.up, data.hairCurl);
                    //}
                    //else {
                    //    folicles[i].transform.Rotate(limb.up, -data.hairCurl);
                    //}
                }
                else {
                    folicles[i].gameObject.SetActive(false);
                }
            }

            if (folicles.Length < folicleCount) {
                Debug.LogWarning("out of folicles");
            }
        }

        public void SetHairColor(int index) {
            data.hairColorIndex = index;
            Color c = RedMatch.Match.customizationOptions.GetHairMeshColor(index);
            for (int i = 0; i < folicles.Length; i++) {
                folicles[i].materials[0].color = c;
            }
        }

        // if this changes, then UpdateHair needs to be called
        public void UpdateMuscle() {

            float armDiameter = .175f;
            if (data.muscleSize == 1) armDiameter = .25f;
            else if (data.muscleSize == 2) armDiameter = .38f;

            limb.localScale = new Vector3(armDiameter, 1f, armDiameter);
        }
    }
}
