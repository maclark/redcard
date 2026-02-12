using UnityEngine;

namespace RedCard {
    public class ManagerTactics : ScriptableObject {
        public SerializableCollection<TacticPresetTypes, TacticPreset> Presets;
    }
}
