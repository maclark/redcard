using UnityEngine;

namespace RedCard {
    [System.Serializable]
    public class ArmData {
        public bool isDominant;

        public int skinColorIndex;

        public float hairThickness;
        public float hairLength;
        public float hairCurl;
        public int hairColorIndex;

        public int muscleSize;

        public float nailLength;
        public int[] nailColorIndices;

        // #TODO tattoo data
        public static void SaveArms(ArmData leftData, ArmData rightData) {
            PlayerPrefs.SetInt("LeftArmIsDominant", leftData.isDominant ? 1 : 0);
            PlayerPrefs.SetInt("RightArmIsDominant", rightData.isDominant ? 1 : 0);

            // without less of generality...
            PlayerPrefs.SetInt("SkinColorIndex", leftData.skinColorIndex);
            PlayerPrefs.SetFloat("HairThickness", leftData.hairThickness);
            PlayerPrefs.SetFloat("HairLength", leftData.hairLength);
            PlayerPrefs.SetFloat("HairCurl", leftData.hairCurl);
            PlayerPrefs.SetInt("HairColorIndex", leftData.hairColorIndex);
            PlayerPrefs.SetInt("MuslceSize", leftData.muscleSize);
            PlayerPrefs.SetFloat("NailLength", leftData.nailLength);

            bool valid = true;
            if (leftData.nailColorIndices == null || leftData.nailColorIndices.Length != 5) {
                Debug.LogError("bad left nail color data");
                valid = false;
            }
            if (rightData.nailColorIndices == null || rightData.nailColorIndices.Length != 5) {
                Debug.LogError("bad right nail color data");
                valid = false;
            }
            if (valid) {
                for (int i = 0; i < 5; i++) {
                    PlayerPrefs.SetInt("LeftNailColorIndex_" + i, leftData.nailColorIndices[i]);
                    PlayerPrefs.SetInt("RightNailColorIndex_" + i, rightData.nailColorIndices[i]);
                }
            }
        }

        public static ArmData LoadArms(Chirality side, bool rightHanded) {
            ArmData newArms = new ArmData();

            if (PlayerPrefs.HasKey("SkinColorIndex")) {
                newArms.skinColorIndex = PlayerPrefs.GetInt("SkinColorIndex");
                newArms.hairThickness = PlayerPrefs.GetFloat("HairThickness");
                newArms.hairLength = PlayerPrefs.GetFloat("HairLength");
                newArms.hairCurl = PlayerPrefs.GetFloat("HairCurl");
                newArms.hairColorIndex = PlayerPrefs.GetInt("HairColorIndex");
                newArms.muscleSize = PlayerPrefs.GetInt("MuslceSize");
                newArms.nailLength = PlayerPrefs.GetFloat("NailLength");

                if (side == Chirality.Left) {
                    newArms.isDominant = PlayerPrefs.GetInt("LeftArmIsDominant") == 1;
                    newArms.nailColorIndices = new int[5];
                    for (int i = 0; i < 5; i++) {
                        newArms.nailColorIndices[i] = PlayerPrefs.GetInt("LeftNailColorIndex_" + i);
                    }
                }
                else {
                    newArms.isDominant = PlayerPrefs.GetInt("RightArmIsDominant") == 1;
                    newArms.nailColorIndices = new int[5];
                    for (int i = 0; i < 5; i++) {
                        newArms.nailColorIndices[i] = PlayerPrefs.GetInt("RightNailColorIndex_" + i);
                    }
                }
                // #TODO tattoo data
            }
            else {
                // set initial values!
                if (rightHanded && side == Chirality.Right) newArms.isDominant = true;
                else if (!rightHanded && side == Chirality.Left) newArms.isDominant = true;
                else newArms.isDominant = false;

                CustomizationOptions cops = RedMatch.Match.customizationOptions;
                newArms.skinColorIndex = 1;// Random.Range(0, cops.skinMeshColors.Length);
                newArms.hairThickness = 1500; // idk
                newArms.hairLength = .015f; // idk
                newArms.hairCurl = 60f;
                newArms.hairColorIndex = 0; // Random.Range(0, cops.hairMeshColors.Length);
                newArms.muscleSize = 1; // normal
                newArms.nailLength = 425f; // short
                newArms.nailColorIndices = new int[5]; // all zeroes for clear
                // #TODO tattoo data
            }


            return newArms;
        }


        public static void ClearArmData() {
            PlayerPrefs.DeleteKey("RightArmIsDominant");
            PlayerPrefs.DeleteKey("LeftArmIsDominant");

            // without less of generality...
            PlayerPrefs.DeleteKey("SkinColorIndex");
            PlayerPrefs.DeleteKey("HairThickness");
            PlayerPrefs.DeleteKey("HairLength");
            PlayerPrefs.DeleteKey("HairCurl");
            PlayerPrefs.DeleteKey("HairColorIndex");
            PlayerPrefs.DeleteKey("MuslceSize");
            PlayerPrefs.DeleteKey("NailLength");

            for (int i = 0; i < 5; i++) {
                PlayerPrefs.DeleteKey("LeftNailColorIndex_" + i);
                PlayerPrefs.DeleteKey("RightNailColorIndex_" + i);
            }

            //
            // #TATTOOS #TODO
        }
    }
}
