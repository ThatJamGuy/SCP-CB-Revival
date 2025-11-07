namespace SickDev.DevConsole.Example {
    public class Room10 : Room {
        bool lastActive;

        void Update() {
            if(!lastActive && nextRoom.gameObject.activeSelf) {
                lastActive = true;
                Complete();
            }
        }
    }
}