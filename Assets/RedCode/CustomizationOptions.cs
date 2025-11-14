using UnityEngine;

[CreateAssetMenu(fileName = "NewRefCustomizationOptions", menuName = "ScriptableObjects/RefereeCustomizationOptions")]
public class CustomizationOptions : ScriptableObject
{
    // skin hair muscle nails tattoos
    public Color[] skinSwatchColors;
    [SerializeField] Color[] skinMeshColors;
    public Color[] hairSwatchColors;
    [SerializeField] Color[] hairMeshColors;
    public Color[] nailSwatchColors;
    [SerializeField] Color[] nailMeshColors;
    public Sprite[] tattoos;
    public Texture2D[] tattooTextures;

    public Color GetSkinMeshColor(int index) {
        return skinSwatchColors[index];
    }

    public Color GetHairMeshColor(int index) {
        return hairSwatchColors[index];
    }

    public Color GetNailMeshColor(int index) {
        return nailSwatchColors[index];
    }
}
