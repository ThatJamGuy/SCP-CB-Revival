using UnityEngine;

namespace SickDev.DevConsole.Example {
    public abstract class Room : MonoBehaviour {
        [SerializeField]
        protected Room nextRoom;
        [SerializeField]
        GameObject invisibleWall;
        [SerializeField]
        new protected RoomLight light;
        [SerializeField]
        bool autoHide;

        protected bool isComplete { get; private set; }

        void Awake() {
            if(autoHide)
                gameObject.SetActive(false);
            light.TurnOff();
        }

        public void Show() {
            gameObject.SetActive(true);
        }

        protected void Complete() {
            invisibleWall.SetActive(false);
            if(nextRoom != null)
                nextRoom.Show();
            isComplete = true;
            light.TurnOn();
            DevConsole.singleton.Log("Awesome! You can now proceed on to the next room", new EntryOptions() { color = Color.green });
        }
    }
}