using UnityEngine;
using SickDev.CommandSystem;

namespace SickDev.DevConsole.Example {
    public class Room6 : Room {
        void Start() {
            light.ChangeColor(Color.black);
            DevConsole.singleton.AddCommand(new ActionCommand<Color32>(FixLightColor) { className = "Example" });
        }

        void FixLightColor(Color32 rgbColor) {
            if(isComplete) {
                Debug.LogWarning("This light is already fixed");
                return;
            }
            if(rgbColor == Color.green)
                Complete();
            else
                Debug.LogWarning("Try fixing it with the green color instead");
        }
    }
}