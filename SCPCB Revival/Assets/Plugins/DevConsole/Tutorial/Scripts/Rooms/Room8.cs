using UnityEngine;
using SickDev.CommandSystem;

namespace SickDev.DevConsole.Example {
    public class Room8 : Room {
        void OnEnable() {
            DevConsole.singleton.AddCommand(new ActionCommand<float[]>(OpenRoomUsingArray) { className = "Example" });
        }

        void OpenRoomUsingArray(float[] numbers) {
            if(isComplete) {
                Debug.LogWarning("Great! But you already did this. Try something new...");
                return;
            }

            float sum = 0;
            for(int i = 0; i < numbers.Length; i++)
                sum += numbers[i];

            if(sum != 10) {
                Debug.LogError("The sum of the numbers must be equal to 10, not " + sum);
                return;
            }

            Complete();
        }
    }
}