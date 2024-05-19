using UnityEngine;
using UnityEngine.UIElements;

namespace demo
{
    public class CameraMoveController : MonoBehaviour
    {
        public KeyCode moveFwd = KeyCode.W;
        public KeyCode moveBck = KeyCode.S;
        public KeyCode moveLeft = KeyCode.A;
        public KeyCode moveRight = KeyCode.D;
        public KeyCode moveUp = KeyCode.Q;
        public KeyCode moveDown = KeyCode.E;
        public KeyCode speedUp = KeyCode.LeftShift;
        public MouseButton mouseLookButton = MouseButton.RightMouse;

        public float moveSpeed = 10f;
        public float speedUpAcceleration = 2f;
        public float rotationSpeed = 70f;

        private float yaw;
        private float pitch;

        private Camera mainCamera;

        private void Start()
        {
            mainCamera = GetComponent<Camera>();
            yaw = mainCamera.transform.eulerAngles.y;
            pitch = mainCamera.transform.eulerAngles.x;
        }

        private void Update()
        {
            var move = Vector3.zero;

            var actualMoveSpeed = Input.GetKey(speedUp) ? moveSpeed * speedUpAcceleration : moveSpeed;
            
            if (Input.GetKey(moveFwd))
            {
                move += oxz(mainCamera.transform.forward) * (actualMoveSpeed * Time.fixedDeltaTime);
            }

            if (Input.GetKey(moveBck))
            {
                move -= oxz(mainCamera.transform.forward) * (actualMoveSpeed * Time.fixedDeltaTime);
            }

            if (Input.GetKey(moveLeft))
            {
                move -= oxz(mainCamera.transform.right) * (actualMoveSpeed * Time.fixedDeltaTime);
            }

            if (Input.GetKey(moveRight))
            {
                move += oxz(mainCamera.transform.right) * (actualMoveSpeed * Time.fixedDeltaTime);
            }

            if (Input.GetKey(moveDown))
            {
                move.y -= actualMoveSpeed * Time.fixedDeltaTime;
            }

            if (Input.GetKey(moveUp))
            {
                move.y += actualMoveSpeed * Time.fixedDeltaTime;
            }

            mainCamera.transform.position += move;

            if (Input.GetMouseButton((int)mouseLookButton))
            {
                pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.fixedDeltaTime;
                yaw += Input.GetAxis("Mouse X") * rotationSpeed * Time.fixedDeltaTime;
                mainCamera.transform.eulerAngles = new Vector3(pitch, yaw, 0f);
            }
        }

        private Vector3 oxz(Vector3 src)
        {
            return new Vector3(src.x, 0f, src.z).normalized;
        }
    }
}