using UnityEngine;

namespace SickDev.DevConsole.Example {
    public class PlayerMovement : MonoBehaviour {
        [SerializeField]
        new Camera camera;
        [SerializeField]
        float speed;

        CharacterController _characterController;
        CharacterController characterController {
            get {
                if(_characterController == null)
                    _characterController = GetComponent<CharacterController>();
                return _characterController;
            }
        }

        void Update() {
            RotateToMouse();
            MoveBasedOnInput();
        }

        void RotateToMouse() {
            Ray mouseRay = camera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, transform.position);
            float distance;
            if(!plane.Raycast(mouseRay, out distance))
                return;

            Vector3 worldMouse = mouseRay.origin + mouseRay.direction * distance;
            transform.LookAt(worldMouse);
        }

        void MoveBasedOnInput() {
            float horizontalMovement = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? -1 : 
                Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
            float verticalMovement = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? -1 :
                Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0;
            characterController.SimpleMove(new Vector3(horizontalMovement, 0, verticalMovement).normalized * speed);
        }
    }
}