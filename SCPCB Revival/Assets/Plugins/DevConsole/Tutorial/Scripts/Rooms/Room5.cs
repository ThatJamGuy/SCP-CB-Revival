using SickDev.CommandSystem;
using UnityEngine;

namespace SickDev.DevConsole.Example {
    public class Room5 : Room {
        void OnEnable() {
            DevConsole.singleton.AddCommand(new ActionCommand<string>(OpenRoom) { className = "Example" });
        }

        void OpenRoom(string name) {
            if(name != nextRoom.name) {
                Debug.LogError("Wrong room name, try openning \"" + nextRoom.name + "\"");
                return;
            }
            if(isComplete) {
                Debug.LogWarning("Great! But you already did this. Try something new...");
                return;
            }
            Complete();
        }
    }
}