using UnityEngine;

public class ShoulderColorer : MonoBehaviour
{
    public MeshRenderer[] skin;
    [SerializeField] MeshRenderer sleeve;
    [SerializeField] Color sleeveColor = Color.yellow;
    [SerializeField] Color skinColor = Color.white;

    private void Awake() {
        sleeve.materials[0].color = sleeveColor;
        for (int i = 0; i < skin.Length; i++) {
            skin[i].materials[0].color = skinColor;
        }
    }
}
