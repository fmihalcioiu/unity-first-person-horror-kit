using com.first_person_horror_kit.Core;
using UnityEngine;

namespace com.first_person_horror_kit.Player
{
    public class MouseLook : MonoBehaviour
    {
        [SerializeField] private float mouseSensitivity = 2.5f;
        [SerializeField] private Transform playerBody;

        [Header("Look Smoothing")]
        [SerializeField] private float smoothSpeed = 10f;

        [Header("Optional")]
        [SerializeField] private HeadBob headBob;

        private float targetXRotation;
        private float currentXRotation;

        private float yRotation;
        private float targetYRotation;

        private Vector3 baseLocalPosition;

        private bool hasFocus;

        private void Start()
        {
            baseLocalPosition = transform.localPosition;

            if (headBob == null)
                headBob = GetComponent<HeadBob>();

            InitializeLookState();
            LockCursor();
        }

        private void InitializeLookState()
        {
            if (playerBody != null)
            {
                yRotation = playerBody.eulerAngles.y;
                targetYRotation = yRotation;
            }

            float pitch = transform.localEulerAngles.x;
            if (pitch > 180f)
                pitch -= 360f;

            currentXRotation = Mathf.Clamp(pitch, -85f, 85f);
            targetXRotation = currentXRotation;
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            hasFocus = false;
            Invoke(nameof(EnableInput), 0.1f);
        }

        private void EnableInput()
        {
            hasFocus = true;
        }

        private void Update()
        {
            if (GameplayState.isInputBlocked || !hasFocus)
                return;

            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            targetXRotation = Mathf.Clamp(targetXRotation - mouseY, -85f, 85f);
            targetYRotation += mouseX;

            currentXRotation = Mathf.Lerp(currentXRotation, targetXRotation, Time.deltaTime * smoothSpeed);
            yRotation = Mathf.Lerp(yRotation, targetYRotation, Time.deltaTime * smoothSpeed);
        }

        private void LateUpdate()
        {
            if (GameplayState.isInputBlocked || !hasFocus)
                return;

            if (playerBody == null)
                return;

            transform.localRotation = Quaternion.Euler(currentXRotation, 0f, 0f);

            playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);

            Vector3 headBobOffset = headBob != null ? headBob.CurrentOffset : Vector3.zero;
            transform.localPosition = baseLocalPosition + headBobOffset;
        }

        private void Reset()
        {
            if (playerBody == null && transform.parent != null)
                playerBody = transform.parent;

            if (headBob == null)
                headBob = GetComponent<HeadBob>();
        }

        private void OnValidate()
        {
            if (smoothSpeed < 0f)
                smoothSpeed = 0f;
        }
    }
}