using UnityEngine;

namespace SickDev.DevConsole.Example {
    public class RoomLight : MonoBehaviour {
        [SerializeField]
        new MeshRenderer renderer;
        [SerializeField]
        Color offColor;
        [SerializeField]
        Color onColor;

        public void TurnOn() {
            ChangeColor(onColor);
        }

        public void TurnOff() {
            ChangeColor(offColor);
        }

        public void ChangeColor(Color color) {
            renderer.material.SetColor("_EmissionColor", color);
            renderer.material.SetColor("_Color", color);
        }
    }
}