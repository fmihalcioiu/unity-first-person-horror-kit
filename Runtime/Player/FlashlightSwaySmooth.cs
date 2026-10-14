using com.first_person_horror_kit.Core;
using UnityEngine;

namespace com.first_person_horror_kit.Player
{
    [DefaultExecutionOrder(1200)]
    public class FlashlightSwaySmooth : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform flashlightModel;
        [SerializeField] private Transform playerCamera;
        [SerializeField] private Flashlight flashlightController;
        [SerializeField] private CameraZoom cameraZoom;

        [Header("Follow Delay")]
        [SerializeField] private float positionDelay = 0.08f;
        [SerializeField] private float rotationFollowSpeed = 10f;
        [SerializeField] private bool suspendWhileZoomScriptMovesFlashlight = true;

        private Vector3 cameraToFlashlightPosOffset;
        private Quaternion cameraToFlashlightRotOffset;

        private Vector3 smoothedCameraPosition;
        private Vector3 smoothVelocity;
        private Quaternion smoothedCameraRotation;

        private void Start()
        {
            if (flashlightController == null)
                flashlightController = GetComponentInParent<Flashlight>();

            if (cameraZoom == null)
                cameraZoom = GetComponentInParent<CameraZoom>();

            if (flashlightModel == null || playerCamera == null)
                return;

            RefreshOffsetsFromCurrentPose();

            smoothedCameraPosition = playerCamera.position;
            smoothedCameraRotation = playerCamera.rotation;
        }

        public void RefreshOffsetsFromCurrentPose()
        {
            if (flashlightModel == null || playerCamera == null)
                return;

            cameraToFlashlightPosOffset = Quaternion.Inverse(playerCamera.rotation) * (flashlightModel.position - playerCamera.position);
            cameraToFlashlightRotOffset = Quaternion.Inverse(playerCamera.rotation) * flashlightModel.rotation;
        }

        private void LateUpdate()
        {
            if (GameplayState.isInputBlocked)
                return;

            if (flashlightModel == null || playerCamera == null)
                return;

            if (suspendWhileZoomScriptMovesFlashlight && cameraZoom != null && cameraZoom.IsDrivingFlashlightPose)
                return;

            smoothedCameraPosition = Vector3.SmoothDamp(
                smoothedCameraPosition,
                playerCamera.position,
                ref smoothVelocity,
                Mathf.Max(0.0001f, positionDelay));

            float rotT = 1f - Mathf.Exp(-rotationFollowSpeed * Time.deltaTime);
            smoothedCameraRotation = Quaternion.Slerp(smoothedCameraRotation, playerCamera.rotation, rotT);

            Vector3 targetPosition = smoothedCameraPosition + smoothedCameraRotation * cameraToFlashlightPosOffset;
            Quaternion targetRotation = smoothedCameraRotation * cameraToFlashlightRotOffset;

            flashlightModel.SetPositionAndRotation(targetPosition, targetRotation);
        }

        private void Reset()
        {
            if (flashlightController == null)
                flashlightController = GetComponentInParent<Flashlight>();

            if (cameraZoom == null)
                cameraZoom = GetComponentInParent<CameraZoom>();

            if (playerCamera == null && Camera.main != null)
                playerCamera = Camera.main.transform;

            if (flashlightModel == null && transform.childCount > 0)
                flashlightModel = transform.GetChild(0);
        }
    }
}