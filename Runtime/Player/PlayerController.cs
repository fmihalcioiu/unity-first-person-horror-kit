using com.first_person_horror_kit.Core;
using UnityEngine;

namespace com.first_person_horror_kit.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float speed = 5f;
        [SerializeField] private float sprintMultiplier = 1.8f;
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField] private float gravity = -9.81f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.4f;
        [SerializeField] private LayerMask groundMask = ~0;

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;

        public bool IsSprinting { get; private set; }
        public float SprintMultiplier => sprintMultiplier;

        public Vector2 MoveInput { get; private set; }
        public float MoveMagnitude { get; private set; }

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (GameplayState.isInputBlocked)
                return;

            if (controller == null)
                return;

            isGrounded = groundCheck != null
                ? Physics.CheckSphere(groundCheck.position, groundDistance, groundMask, QueryTriggerInteraction.Ignore)
                : controller.isGrounded;

            if (isGrounded && velocity.y < 0f)
                velocity.y = -2f;

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            MoveInput = new Vector2(x, z);
            MoveMagnitude = Mathf.Clamp01(MoveInput.magnitude);

            Vector3 localInput = new Vector3(x, 0f, z);
            localInput = Vector3.ClampMagnitude(localInput, 1f);

            Vector3 move = transform.TransformDirection(localInput);
            bool isMoving = move.sqrMagnitude > 0.001f;

            IsSprinting = isMoving && Input.GetKey(sprintKey);

            float currentSpeed = IsSprinting ? speed * sprintMultiplier : speed;
            Vector3 horizontalVelocity = move * currentSpeed;

            velocity.y += gravity * Time.deltaTime;

            Vector3 frameMotion = (horizontalVelocity + Vector3.up * velocity.y) * Time.deltaTime;
            controller.Move(frameMotion);
        }

        private void OnValidate()
        {
            if (groundDistance < 0.01f)
                groundDistance = 0.01f;
        }
    }
}