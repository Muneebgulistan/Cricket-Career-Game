using UnityEngine;

namespace CricketGame.Players
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Speeds (m/s)")]
        [SerializeField] private float walkSpeed = 2.5f;
        [SerializeField] private float runSpeed = 5.5f;
        [SerializeField] private float rotationSpeed = 12.0f;
        [SerializeField] private float gravity = 9.81f;

        [Header("Ground Detection")]
        [SerializeField] private float groundCheckDistance = 0.3f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Transform groundCheckPoint;

        [Header("State")]
        [SerializeField] private bool canMove = true;
        [SerializeField] private bool isGrounded = true;
        [SerializeField] private float currentSpeed = 0f;
        [SerializeField] private bool isRunning = false;

        private CharacterController characterController;
        private Vector2 virtualInput = Vector2.zero;
        private bool virtualSprint = false;
        private float verticalVelocity = 0f;

        public float CurrentSpeed { get { return currentSpeed; } }
        public bool IsGrounded { get { return isGrounded; } }
        public bool IsMoving { get { return currentSpeed > 0.05f; } }
        public bool IsRunning { get { return isRunning && IsMoving; } }

        public bool CanMove
        {
            get { return canMove; }
            set { canMove = value; }
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!canMove)
            {
                currentSpeed = 0f;
                return;
            }

            Vector2 input = ReadInput();
            ProcessMovement(input);
        }

        private Vector2 ReadInput()
        {
            // Virtual joystick input takes priority if provided
            if (virtualInput.sqrMagnitude > 0.01f)
            {
                return virtualInput;
            }

            // Keyboard input (WASD / Arrows)
            float h = 0f;
            float v = 0f;

            if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow)) v += 1f;
            if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow)) v -= 1f;
            if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow)) h += 1f;
            if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow)) h -= 1f;

            Vector2 keyboardInput = new Vector2(h, v);
            if (keyboardInput.sqrMagnitude > 1.0f)
            {
                keyboardInput = keyboardInput.normalized;
            }

            return keyboardInput;
        }

        private void ProcessMovement(Vector2 input)
        {
            bool wantsRun = UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift) || virtualSprint;
            float targetSpeed = wantsRun ? runSpeed : walkSpeed;

            Vector3 moveDir = new Vector3(input.x, 0f, input.y);

            if (moveDir.sqrMagnitude > 0.01f)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 10f);
                isRunning = wantsRun;

                // Smooth rotation towards movement direction
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * 15f);
                if (currentSpeed < 0.05f)
                {
                    currentSpeed = 0f;
                    isRunning = false;
                }
            }

            // Grounding & Gravity
            CheckGround();
            if (isGrounded)
            {
                verticalVelocity = -0.5f; // Small grounding force
            }
            else
            {
                verticalVelocity -= gravity * Time.deltaTime;
            }

            Vector3 velocity = moveDir.normalized * currentSpeed;
            velocity.y = verticalVelocity;

            if (characterController != null && characterController.enabled)
            {
                characterController.Move(velocity * Time.deltaTime);
            }
            else
            {
                // Fallback transform movement if CharacterController not attached
                transform.position += velocity * Time.deltaTime;
                if (transform.position.y < 0f)
                {
                    Vector3 pos = transform.position;
                    pos.y = 0f;
                    transform.position = pos;
                }
            }
        }

        private void CheckGround()
        {
            if (characterController != null)
            {
                isGrounded = characterController.isGrounded || transform.position.y <= 0.05f;
            }
            else
            {
                isGrounded = transform.position.y <= 0.05f;
            }
        }

        public void SetVirtualInput(Vector2 input, bool sprint)
        {
            virtualInput = input;
            virtualSprint = sprint;
        }
    }
}
