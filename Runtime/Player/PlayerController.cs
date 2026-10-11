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
        [SerializeField] private float terminalVelocity = -35f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.4f;
        [SerializeField] private LayerMask groundMask = ~0;

        [Header("Collision Stability")]
        [SerializeField] private LayerMask obstacleMask = ~0;
        [SerializeField] private float wallProbeDistance = 0.3f;

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
            Vector3 horizontalVelocity = ResolveWallSlide(move * currentSpeed);

            velocity.y += gravity * Time.deltaTime;
            if (velocity.y < terminalVelocity)
                velocity.y = terminalVelocity;

            Vector3 frameMotion = (horizontalVelocity + Vector3.up * velocity.y) * Time.deltaTime;
            CollisionFlags flags = controller.Move(frameMotion);

            if ((flags & CollisionFlags.Above) != 0 && velocity.y > 0f)
                velocity.y = 0f;

            if ((flags & CollisionFlags.Below) != 0 && velocity.y < 0f)
                velocity.y = -2f;
        }

        private Vector3 ResolveWallSlide(Vector3 horizontalVelocity)
        {
            if (horizontalVelocity.sqrMagnitude <= 0.0001f)
                return horizontalVelocity;

            Vector3 direction = horizontalVelocity.normalized;
            Vector3 origin = transform.position + controller.center;
            float radius = Mathf.Max(0.01f, controller.radius * 0.95f);
            float castDistance = wallProbeDistance + controller.skinWidth;

            if (Physics.SphereCast(origin, radius, direction, out RaycastHit hit, castDistance, obstacleMask, QueryTriggerInteraction.Ignore))
                return Vector3.ProjectOnPlane(horizontalVelocity, hit.normal);

            return horizontalVelocity;
        }

        private void OnValidate()
        {
            if (groundDistance < 0.01f)
                groundDistance = 0.01f;

            if (wallProbeDistance < 0.01f)
                wallProbeDistance = 0.01f;
        }
    }
}