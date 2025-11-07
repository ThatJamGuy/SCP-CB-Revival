namespace SickDev.DevConsole.Example {
    public class Room1 : Room {
        bool lastWasOpen;

        void Update() {
            if(!lastWasOpen && DevConsole.singleton.isOpen) {
                lastWasOpen = true;
                Complete();
            }
        }
    }
}