using UnityEngine;

// Register the object this script is attached to based off some presets.
namespace scpcbr {
    public class RegisterThis : MonoBehaviour {
        public enum RegisterType {
            Player
        }

        public RegisterType type = RegisterType.Player;

        public bool playerToSubEmitters = true;

        #region Default Methods
        private void Awake() {
            switch (type) {
                case RegisterType.Player:
                    if(playerToSubEmitters) {
                        SubtitleEmitter.RegisterListener(transform);
                    }
                    break;
                default:
                    Debug.LogError($"Unknown RegisterType: {type}");
                    break;
            }
        }
        #endregion
    }
}