using UnityEngine;

[System.Serializable]
public class ArmData
{
    public bool leftArmIsDominant;

    public int skinColorIndex;

    public float hairThickness;
    public float hairLength;
    public float hairCurl;
    public int hairColorIndex;

    public int muscleSize;

    public float nailLength;
    public int[] leftNailColorIndices;
    public int[] rightNailColorIndices;

    // #TODO tattoo data

    public static void SaveArms(ArmData data) {
        PlayerPrefs.SetInt("LeftArmIsDominant", data.leftArmIsDominant ? 1 : 0);

        PlayerPrefs.SetInt("SkinColorIndex", data.skinColorIndex);

        PlayerPrefs.SetFloat("HairThickness", data.hairThickness);
        PlayerPrefs.SetFloat("HairLength", data.hairLength);
        PlayerPrefs.SetFloat("HairCurl", data.hairCurl);
        PlayerPrefs.SetInt("HairColorIndex", data.hairColorIndex);

        PlayerPrefs.SetInt("MuslceSize", data.muscleSize);

        PlayerPrefs.SetFloat("NailLength", data.nailLength);
        bool valid = true;
        if (data.leftNailColorIndices == null || data.leftNailColorIndices.Length != 5) {
            Debug.LogError("bad left nail color data");
            valid = false;
        }
        if (data.rightNailColorIndices == null || data.rightNailColorIndices.Length != 5) {
            Debug.LogError("bad right nail color data");
            valid = false;
        }
        if (valid) {
            for (int i = 0; i < 5; i++) {
                PlayerPrefs.SetInt("LeftNailColorIndex_" + i, data.leftNailColorIndices[i]);
                PlayerPrefs.SetInt("RightNailColorIndex_" + i, data.rightNailColorIndices[i]);
            }
        }
    }

    public static ArmData LoadArms() {
        ArmData newArms = new ArmData();

        newArms.leftArmIsDominant = PlayerPrefs.GetInt("LeftArmIsDominant") == 1;

        newArms.skinColorIndex = PlayerPrefs.GetInt("SkinColorIndex");

        newArms.hairThickness = PlayerPrefs.GetFloat("HairThickness");
        newArms.hairLength = PlayerPrefs.GetFloat("HairLength");
        newArms.hairCurl = PlayerPrefs.GetFloat("HairCurl");
        newArms.hairColorIndex = PlayerPrefs.GetInt("HairColorIndex");

        newArms.muscleSize = PlayerPrefs.GetInt("MuslceSize");

        newArms.nailLength = PlayerPrefs.GetFloat("NailLength");
        newArms.leftNailColorIndices = new int[5];
        newArms.rightNailColorIndices = new int[5];
        for (int i = 0; i < 5; i++) {
            newArms.leftNailColorIndices[i] = PlayerPrefs.GetInt("LeftNailColorIndex_" + i);
            newArms.rightNailColorIndices[i] = PlayerPrefs.GetInt("RightLeftNailColorIndex_" + i);
        }

        // #TODO tattoo data

        return newArms;
    }
}
