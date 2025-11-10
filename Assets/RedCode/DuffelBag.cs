using UnityEngine;
using System.Collections.Generic;

namespace RedCard {
    public class DuffelBag : MonoBehaviour {

        public Coin coin;
        public List<RefEquipment> equipment = new List<RefEquipment>();

        private void Awake() {
            if (TryGetComponent(out MeshRenderer renderer)) {
                if (renderer.materials.Length > 0) renderer.materials[0].color = Color.black;
            }
            equipment.Add(RefEquipment.Coin);
            equipment.Add(RefEquipment.RedCard);
            equipment.Add(RefEquipment.YellowCard);
            equipment.Add(RefEquipment.SprayCan);
            equipment.Add(RefEquipment.Book);
            equipment.Add(RefEquipment.Watch);
            equipment.Add(RefEquipment.Whistle);

            coin = FindAnyObjectByType<Coin>();
            if (coin) coin.gameObject.SetActive(false);
        }
    }
}
