using UnityEngine;

namespace scpcbr {
    public class ReferenceHolder : MonoBehaviour {
        public static ReferenceHolder Instance { get; private set; }

        void Awake() {
            Instance = this;
        }
    }
}