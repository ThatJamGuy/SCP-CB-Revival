using UnityEngine;

namespace scpcbr {
    public class SubtitleListener : MonoBehaviour {
        public bool registerAsPlayer = true;
        void OnEnable() { if (registerAsPlayer) SubtitleEmitter.RegisterListener(transform); }
        void OnDisable() { if (registerAsPlayer) SubtitleEmitter.UnregisterListener(transform); }
    }
}