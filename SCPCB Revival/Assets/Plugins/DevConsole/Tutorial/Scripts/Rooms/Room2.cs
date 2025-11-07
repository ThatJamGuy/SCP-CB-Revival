using SickDev.CommandSystem;

namespace SickDev.DevConsole.Example {
    public class Room2 : Room {
        Command command;

        void OnEnable() {
            DevConsole.singleton.AddCommand(command = new ActionCommand(OpenRoom3) { className = "Example" });
        }

        void OpenRoom3() {
            Complete();
            DevConsole.singleton.RemoveCommand(command);
        }
    }
}