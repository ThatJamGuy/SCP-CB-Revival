namespace SickDev.DevConsole.Example {
    public class Room11 : Room {
        bool lastActive;

        void Update() {
            Room12 room = FindObjectOfType<Room12>();
            if(!lastActive && room != null) {
                lastActive = true;
                Complete();
            }
        }
    }
}