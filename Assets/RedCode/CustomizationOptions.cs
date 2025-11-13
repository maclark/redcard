using UnityEngine;

[CreateAssetMenu(fileName = "NewRefCustomizationOptions", menuName = "ScriptableObjects/RefereeCustomizationOptions")]
public class CustomizationOptions : ScriptableObject
{
    // skin hair muscle nails tattoos
    public Color[] skinSwatchColors;
    public Color[] skinMeshColors;
    public Color[] hairSwatchColors;
    public Color[] hairMeshColors;
    public Color[] nailSwatchColors;
    public Color[] nailMeshColors;
    public Sprite[] tattoos;
    public Texture2D[] tattooTextures;
}
