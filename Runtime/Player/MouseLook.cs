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

        [Header("Strafe Tilt")]
        [SerializeField] private float maxTiltAngle = 3f;
        [SerializeField] private float tiltSmooth = 8f;

        [Header("Shake Intensity")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private float idleShakeAmount = 0.04f;
        [SerializeField] private float moveShakeAmount = 0.12f;
        [SerializeField] private float sprintShakeAmount = 0.10f;
        [SerializeField] private float shakeBlendSpeed = 4f;

        [Header("Shake Frequency (Slower = More Realistic)")]
        [SerializeField] private float idleShakeFrequency = 1.4f;
        [SerializeField] private float moveShakeFrequency = 2.2f;
        [SerializeField] private float sprintShakeFrequency = 2.8f;

        [Header("Strafe Influence")]
        [SerializeField] private float strafeRollShake = 1.25f;
        [SerializeField] private float strafePositionShake = 0.0022f;
        [SerializeField] private float strafeShakeSmooth = 6f;

        [Header("Shake Smoothing")]
        [SerializeField] private float rotationShakeSmooth = 5f;
        [SerializeField] private float positionShakeSmooth = 6f;
        [SerializeField] private float basePositionShake = 0.0018f;

        [Header("Optional")]
        [SerializeField] private HeadBob headBob;

        private float targetXRotation;
        private float currentXRotation;

        private float yRotation;
        private float targetYRotation;

        private float currentTilt;
        private float currentShakeAmount;
        private float currentShakeFrequency;
        private float currentStrafeInfluence;

        private Vector3 baseLocalPosition;
        private Vector3 currentRotationShake;
        private Vector3 currentPositionShake;

        private bool hasFocus;
        private float noiseTime;

        private float seedX;
        private float seedY;
        private float seedZ;

        private Vector2 frameMoveInput;
        private float frameMoveMagnitude;
        private bool frameSprint;

        private void Start()
        {
            baseLocalPosition = transform.localPosition;

            if (playerController == null)
                playerController = GetComponentInParent<PlayerController>();

            if (headBob == null)
                headBob = GetComponent<HeadBob>();

            seedX = Random.value * 100f;
            seedY = Random.value * 100f;
            seedZ = Random.value * 100f;

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

            if (playerController == null)
                return;

            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            targetXRotation = Mathf.Clamp(targetXRotation - mouseY, -85f, 85f);
            targetYRotation += mouseX;

            currentXRotation = Mathf.Lerp(currentXRotation, targetXRotation, Time.deltaTime * smoothSpeed);
            yRotation = Mathf.Lerp(yRotation, targetYRotation, Time.deltaTime * smoothSpeed);

            frameMoveInput = playerController.MoveInput;
            frameMoveMagnitude = playerController.MoveMagnitude;
            frameSprint = playerController.IsSprinting;

            currentTilt = Mathf.Lerp(currentTilt, -frameMoveInput.x * maxTiltAngle, Time.deltaTime * tiltSmooth);

            float targetShakeAmount = idleShakeAmount + (frameMoveMagnitude * moveShakeAmount) + (frameSprint ? sprintShakeAmount : 0f);
            currentShakeAmount = Mathf.Lerp(currentShakeAmount, targetShakeAmount, Time.deltaTime * shakeBlendSpeed);

            float targetFrequency = frameSprint
                ? sprintShakeFrequency
                : Mathf.Lerp(idleShakeFrequency, moveShakeFrequency, frameMoveMagnitude);

            currentShakeFrequency = Mathf.Lerp(currentShakeFrequency, targetFrequency, Time.deltaTime * shakeBlendSpeed);
            noiseTime += Time.deltaTime * currentShakeFrequency;

            float noiseX = (Mathf.PerlinNoise(seedX, noiseTime) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(seedY, noiseTime * 1.07f) - 0.5f) * 2f;
            float noiseZ = (Mathf.PerlinNoise(seedZ, noiseTime * 0.93f) - 0.5f) * 2f;

            currentStrafeInfluence = Mathf.Lerp(currentStrafeInfluence, frameMoveInput.x, Time.deltaTime * strafeShakeSmooth);

            Vector3 targetRotationShake = new Vector3(
                noiseX * currentShakeAmount,
                noiseY * currentShakeAmount * 0.25f,
                (noiseZ * currentShakeAmount * 0.55f) - (currentStrafeInfluence * strafeRollShake));

            currentRotationShake = Vector3.Lerp(currentRotationShake, targetRotationShake, Time.deltaTime * rotationShakeSmooth);
        }

        private void LateUpdate()
        {
            if (GameplayState.isInputBlocked || !hasFocus)
                return;

            if (playerBody == null)
                return;

            transform.localRotation = Quaternion.Euler(
                currentXRotation + currentRotationShake.x,
                currentRotationShake.y,
                currentTilt + currentRotationShake.z);

            playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);

            float noiseX = (Mathf.PerlinNoise(seedX, noiseTime) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(seedY, noiseTime * 1.07f) - 0.5f) * 2f;
            float positionalAmplitude = basePositionShake + (frameMoveMagnitude * basePositionShake * 0.8f);

            Vector3 targetPositionShake = new Vector3(
                (noiseY * positionalAmplitude) + (currentStrafeInfluence * strafePositionShake),
                noiseX * positionalAmplitude * 0.85f,
                0f);

            currentPositionShake = Vector3.Lerp(currentPositionShake, targetPositionShake, Time.deltaTime * positionShakeSmooth);

            Vector3 headBobOffset = headBob != null ? headBob.CurrentOffset : Vector3.zero;
            transform.localPosition = baseLocalPosition + currentPositionShake + headBobOffset;
        }

        private void Reset()
        {
            if (playerController == null)
                playerController = GetComponentInParent<PlayerController>();

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