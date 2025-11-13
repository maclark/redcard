using UnityEngine;
using System.Collections.Generic;

namespace RedCard {

    public enum Finger {
        Pinky,
        Ring,
        Middle,
        Index,
        Thumb
    }

    public class Arm : MonoBehaviour {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        // #TODO
        // renderers and shit
        public Chirality side;
        public bool isDominant = false;
        public List<Tattoo> tattoos = new List<Tattoo>();
        public float hairLength;
        public float hairDensity;
        public float hairCurlDegrees;
        public float radius;
        public Color[] nailColors = new Color[5];
        public float zStart = 1.5f;
        public float zEnd = 2f;
        public float phiThreshold = Mathf.PI;
        public float phiOffset = -Mathf.PI / 3f;

        [Header("ASSIGNATIONS")]
        public Transform limb;
        public MeshRenderer[] folicles = new MeshRenderer[0];
        public MeshRenderer[] nails = new MeshRenderer[0];
        public ShoulderColorer colorer;

        [Header("SETTINGS")]
        public Vector3 localLoweredPos;
        public Vector3 localRaisedPos;


        private void Awake() {
            radius = .5f * limb.transform.localScale.x;
            hairLength = .05f;
            hairDensity = 100f;
            hairCurlDegrees = 20f;
            SetHairColor(Color.magenta);
            UpdateHairDensity();
            UpdateHairLength();
        }


        public void UpdateHairLength() {
            for (int i = 0; i < folicles.Length; i++) {
                folicles[i].transform.localScale = new Vector3(.01f, hairLength, .01f);
            }
        }
        public void UpdateHairDensity() {
            print("updatehairdensity " + hairDensity);

            // circumference times height
            // we are ignoring sides of cylinder
            float surfaceArea = Mathf.PI * radius * (zEnd - zStart);
            print("surface area: " + surfaceArea);
            int folicleCount = Mathf.RoundToInt(surfaceArea * hairDensity);
            print("folicleCount " + folicleCount);
            print("radius " + radius);
            for (int i = 0; i < folicles.Length; i++) {
                if (i < folicleCount) {
                    float h = Random.Range(zStart, zEnd);
                    float phi = i * Mathf.PI * 2f / folicles.Length + phiOffset; 

                    float x = radius * Mathf.Cos(phi);
                    float z = radius * Mathf.Sin(phi);
                    Vector3 axialPoint = limb.position + limb.up * h;
                    Vector3 point = axialPoint + limb.right * x + limb.forward * z;
                    folicles[i].transform.position = point;
                    folicles[i].gameObject.SetActive(true);
                    folicles[i].transform.rotation = Quaternion.LookRotation(limb.up, point - axialPoint);
                    if (phi < phiThreshold) {
                        folicles[i].transform.Rotate(limb.up, hairCurlDegrees);
                    }
                    else {
                        folicles[i].transform.Rotate(limb.up, -hairCurlDegrees);
                    }
                }
                else {
                    folicles[i].gameObject.SetActive(false);
                }
            }
            if (folicles.Length < folicleCount) {
                Debug.LogWarning("out of folicles");
            }



            //float r = 0.5f * diameter;
            //float z = zStart;
            //float phi = Mathf.PI * .25f;
            //float hairGap = (zEnd - zStart) / (float)hairsPerRow;
            //// only cover two thirds of the arm
            //float dPhi = (2f / 3f) * Mathf.PI * 2f / rows;
            //float xz = Mathf.Min(.005f, hairLength);
            //for (int rowIndex = 0; rowIndex < rows; rowIndex++) {

            //    for (int hairIndex = 0; hairIndex < hairsPerRow; hairIndex++) {
            //        int index = rowIndex * hairsPerRow + hairIndex;
            //        if (index < folicles.Length) {
            //            MeshRenderer f = folicles[rowIndex * hairsPerRow + hairIndex];
            //            f.gameObject.SetActive(true);
            //            Vector3 pos = limb.transform.position + limb.transform.up * z;
            //            f.transform.position = pos;
            //            f.transform.localRotation = Quaternion.identity;
            //            f.transform.Rotate(limb.transform.up, phi * Mathf.Rad2Deg);
            //            f.transform.position += f.transform.up * r;
            //            f.transform.Rotate(limb.transform.up, -3 * dPhi * Mathf.Rad2Deg);
            //            f.transform.localScale = new Vector3(xz, hairLength, xz);
            //            z += hairGap;
            //        }
            //        else Debug.LogWarning("not enough folicles");
            //    }

            //    z = zStart + (rowIndex % 2 == 0 ? .025f : 0f);
            //    phi -= dPhi;
            //}

            //for (int i = rows * hairsPerRow; i < folicles.Length; i++) {
            //    folicles[i].gameObject.SetActive(false);
            //}
        }

        public void SetHairColor(Color c) { 
            for (int i = 0; i < folicles.Length; i++) {
                folicles[i].materials[0].color = c;
            }
        }
    }
}
