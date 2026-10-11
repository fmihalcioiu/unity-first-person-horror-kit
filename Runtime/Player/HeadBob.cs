using com.first_person_horror_kit.Core;
using UnityEngine;

namespace com.first_person_horror_kit.Player
{
    public class HeadBob : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerController playerController;

        [Header("Walk Bob")]
        [SerializeField] private float walkBobSpeed = 8.5f;
        [SerializeField] private float walkBobAmount = 0.045f;

        [Header("Sprint Bob")]
        [SerializeField] private float sprintBobSpeed = 12f;
        [SerializeField] private float sprintBobAmount = 0.07f;

        [Header("Side Motion")]
        [SerializeField] private float sideBobAmount = 0.03f;

        [Header("Idle Breathing")]
        [SerializeField] private float idleBreathSpeed = 1.2f;
        [SerializeField] private float idleBreathAmount = 0.008f;

        [Header("Smoothing")]
        [SerializeField] private float positionLerpSpeed = 12f;

        public Vector3 CurrentOffset => currentOffset;

        private float bobTimer;
        private Vector3 currentOffset;

        private void Start()
        {
            if (characterController == null)
                characterController = GetComponentInParent<CharacterController>();

            if (playerController == null)
                playerController = GetComponentInParent<PlayerController>();
        }

        private void LateUpdate()
        {
            if (GameplayState.isInputBlocked || characterController == null)
                return;

            if (playerController == null)
                return;

            bool grounded = characterController.isGrounded;
            float inputMagnitude = playerController.MoveMagnitude;
            bool isMoving = grounded && inputMagnitude > 0.05f;
            bool sprinting = playerController.IsSprinting;

            float bobSpeed = sprinting ? sprintBobSpeed : walkBobSpeed;
            float bobAmount = sprinting ? sprintBobAmount : walkBobAmount;
            float sideAmount = sideBobAmount;

            Vector3 targetOffset = Vector3.zero;

            if (isMoving)
            {
                bobTimer += Time.deltaTime * bobSpeed * inputMagnitude;
                bobTimer = Mathf.Repeat(bobTimer, Mathf.PI * 4f); // keep timer in a small range to avoid float drift
                targetOffset += new Vector3(
                    Mathf.Cos(bobTimer * 0.5f) * sideAmount,
                    Mathf.Sin(bobTimer) * bobAmount,
                    0f);
            }
            else
            {
                bobTimer += Time.deltaTime * idleBreathSpeed;
                targetOffset += new Vector3(0f, Mathf.Sin(bobTimer) * idleBreathAmount, 0f);
            }

            currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * positionLerpSpeed);
        }

        private void Reset()
        {
            if (characterController == null)
                characterController = GetComponentInParent<CharacterController>();

            if (playerController == null)
                playerController = GetComponentInParent<PlayerController>();
        }
    }
}