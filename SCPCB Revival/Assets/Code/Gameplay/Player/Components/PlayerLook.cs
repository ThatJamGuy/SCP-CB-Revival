using UnityEngine;
using UnityEngine.InputSystem;

namespace scpcbr {
    public class PlayerLook : MonoBehaviour {
        [Header("Input")]
        public InputActionAsset playerControls;

        [Header("References")]
        [SerializeField] private PlayerBase playerBase;
        public Transform playerBody;
        public Transform head;

        [Header("Look Settings")]
        public float sensitivity = 1f;
        public float verticalClamp = 80f;

        private InputAction lookAction;
        private Vector2 lookInput;
        private float xRotation;

        #region EnableAndDisable
        private void OnEnable() {
            var actionMap = playerControls.FindActionMap("Player", true);
            lookAction = actionMap.FindAction("Look", true);

            lookAction.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
            lookAction.canceled += _ => lookInput = Vector2.zero;

            playerControls.Enable();
        }

        private void OnDisable() {
            lookAction.performed -= ctx => lookInput = ctx.ReadValue<Vector2>();
            lookAction.canceled -= _ => lookInput = Vector2.zero;

            playerControls.Disable();
        }
        #endregion

        #region Default Methods
        private void Start() {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update() {
            RotateView();
        }
        #endregion

        #region Private Methods
        private void RotateView() {
            if (lookInput == Vector2.zero || !playerBase.allowInput) return;

            Vector2 delta = lookInput * sensitivity;

            xRotation -= delta.y;
            xRotation = Mathf.Clamp(xRotation, -verticalClamp, verticalClamp);

            head.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * delta.x);
        }
        #endregion
    }
}