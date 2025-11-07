using UnityEngine;
using SickDev.CommandSystem;

namespace SickDev.DevConsole.Example {
    public class Room3 : Room {
        void OnEnable() {
            DevConsole.singleton.AddCommand(new ActionCommand<int>(OpenRoom) { className = "Example" });
        }

        void OpenRoom(int number) {
            if(number < 4)
                Debug.LogError("That room is already open");
            else if(number > 4)
                Debug.LogError("Do you really wanna break the tutorial? ¬¬");
            else if(!isComplete)
                Complete();
            else
                Debug.LogWarning("You have already opened that room. Move on!");
        }
    }
}