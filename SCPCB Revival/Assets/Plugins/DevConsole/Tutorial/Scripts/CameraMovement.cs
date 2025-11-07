using UnityEngine;

namespace SickDev.DevConsole.Example {
    [ExecuteInEditMode]
    public class CameraMovement : MonoBehaviour {
        [SerializeField]
        Transform target;
        [SerializeField]
        [Range(0, 1)]
        float speed;

        Camera _cameraComponent;
        public Camera cameraComponent {
            get {
                if(_cameraComponent == null)
                    _cameraComponent = GetComponent<Camera>();
                return _cameraComponent;
            }
        }

        void LateUpdate() {
            transform.position = new Vector3(Mathf.Lerp(transform.position.x, target.position.x, speed*Time.deltaTime), transform.position.y, transform.position.z);
        }
    }
}