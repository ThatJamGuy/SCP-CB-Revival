using UnityEngine;
using SickDev.CommandSystem;

namespace SickDev.DevConsole.Example {
    public class Room4 : Room {
        void OnEnable() {
            DevConsole.singleton.AddCommand(new ActionCommand<int, int>(OpenRoom) { className = "Example" });
        }

        void OpenRoom(int number1, int number2) {
            if(isComplete) {
                Debug.LogWarning("Great! But you already did this. Try something new...");
                return;
            }

            int sum = number1 + number2;
            if(sum != 5) {
                Debug.LogError(number1 + " + " + number2 + " = " + sum + " != 5");
                return;
            }

            Complete();
        }
    }
}