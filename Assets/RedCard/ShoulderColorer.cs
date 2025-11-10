using UnityEngine;

public class ShoulderColorer : MonoBehaviour
{
    [SerializeField] MeshRenderer sleeve;
    [SerializeField] MeshRenderer[] skin;
    [SerializeField] Color sleeveColor = Color.yellow;
    [SerializeField] Color skinColor = Color.white;

    private void Awake() {
        sleeve.materials[0].color = sleeveColor;
        for (int i = 0; i < skin.Length; i++) {
            skin[i].materials[0].color = skinColor;
        }
    }
}
