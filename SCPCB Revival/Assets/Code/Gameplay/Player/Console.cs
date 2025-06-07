using UnityEngine;
using IngameDebugConsole;

public class Console : MonoBehaviour
{
    [ConsoleMethod("noclip", "Activates noclip")]
    public static void ToggleNoclip() {
        // Implement this later
    }

    [ConsoleMethod("infiblink", "Disables the blink decrease functionality")]
    public static void InfiBlink() {
        // Implement this later
    }

    [ConsoleMethod("infistamina", "Disables stamina decrease functionality")]
    public static void InfiStamina() {
        // Implement this later
    }
}