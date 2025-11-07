using UnityEngine;
using SickDev.CommandSystem;

namespace SickDev.DevConsole.Example {
    public class Room9 : Room {
        string password = "DevConsole Rocks";

        void OnEnable() {
            DevConsole.singleton.AddCommand(new ActionCommand<string>(OpenRoomWithPassword) { className = "Example" });
            DevConsole.singleton.AddCommand(new FuncCommand<string>(GetRoomPassword) { className = "Example" });
        }

        void OpenRoomWithPassword(string password) {
            if(isComplete) {
                Debug.LogWarning("Great! But you already did this. Try something new...");
                return;
            }

            if(password != this.password) {
                Debug.LogWarning("Nope. That's not the password for this room. Try using GetRoomPassword first");
                return;
            }
            Complete();
        }

        string GetRoomPassword() {
            return "The password is: " + password;
        }
    }
}