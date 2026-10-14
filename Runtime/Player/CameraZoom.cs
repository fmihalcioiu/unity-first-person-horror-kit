using com.first_person_horror_kit.Core;
using UnityEngine;

namespace com.first_person_horror_kit.Player
{
    [RequireComponent(typeof(Camera))]
    public class CameraZoom : MonoBehaviour
    {
        [Header("Zoom")]
        [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;
        [SerializeField] private float zoomedFov = 35f;
        [SerializeField] private float zoomSpeed = 12f;

        [Header("Zoom Easing")]
        [SerializeField] private AnimationCurve zoomEase = null;
        [SerializeField] private float minTransitionDuration = 0.05f;

        [Header("Look Sensitivity While Zoomed")]
        [SerializeField] private MouseLook mouseLook;
        [Range(0.1f, 1f)] [SerializeField] private float zoomSensitivityMultiplier = 0.45f;
        [SerializeField] private float sensitivityBlendSpeed = 10f;

        [Header("Flashlight Model Hide While Zoomed")]
        [Tooltip("Parent transform of flashlight model to move in/out of frame.")]
        [SerializeField] private Transform flashlightZoomRoot;
        [SerializeField] private Vector3 flashlightShownLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 flashlightHiddenLocalPosition = new Vector3(0f, -0.35f, 0.2f);
        [SerializeField] private float flashlightMoveSpeed = 12f;

        [Header("Flashlight While Zoomed")]
        [SerializeField] private bool moveFlashlightWhileZoomed = false;

        [Header("Focus Pulse After Settle")]
        [SerializeField] private bool enableFocusPulse = true;
        [SerializeField] private float focusPulseAmplitude = 0.25f;
        [SerializeField] private float focusPulseDuration = 0.18f;
        [SerializeField] private float focusPulseCycles = 1.5f;
        [SerializeField] private float focusPulseDamping = 3.5f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip zoomInSound;
        [SerializeField] private AudioClip zoomOutSound;

        public bool IsDrivingFlashlightPose =>
            moveFlashlightWhileZoomed
            && flashlightZoomRoot != null
            && (targetZoomed || transitionActive);

        private Camera cam;
        private float defaultFov;
        private float currentSensitivityMultiplier = 1f;

        private bool targetZoomed;
        private bool transitionActive;
        private float transitionStartFov;
        private float transitionTargetFov;
        private float transitionDuration;
        private float transitionElapsed;

        private float baseFov;
        private float pulseTimer = -1f;

        private void Start()
        {
            cam = GetComponent<Camera>();
            defaultFov = cam.fieldOfView;
            baseFov = defaultFov;
            targetZoomed = false;

            transitionStartFov = defaultFov;
            transitionTargetFov = defaultFov;
            transitionDuration = 0f;
            transitionElapsed = 0f;
            transitionActive = false;

            cam.fieldOfView = defaultFov;

            if (flashlightZoomRoot != null)
                flashlightShownLocalPosition = flashlightZoomRoot.localPosition;

            if (zoomEase == null || zoomEase.length == 0)
            {
                zoomEase = new AnimationCurve(
                    new Keyframe(0f, 0f, 2.2f, 2.2f),
                    new Keyframe(1f, 1f, 0f, 0f));
            }
        }

        private void Update()
        {
            if (GameplayState.isInputBlocked)
                return;

            bool wantsZoom = Input.GetKey(zoomKey);

            if (wantsZoom != targetZoomed)
            {
                targetZoomed = wantsZoom;
                StartTransition(targetZoomed ? zoomedFov : defaultFov);
                PlayZoomSound(targetZoomed);
            }

            UpdateZoomTransition();
            UpdateSensitivity(targetZoomed);
            UpdateFlashlightZoomPose();

            cam.fieldOfView = baseFov + GetFocusPulseOffset();
        }

        private void StartTransition(float newTargetFov)
        {
            transitionStartFov = baseFov;
            transitionTargetFov = newTargetFov;

            float distance = Mathf.Abs(transitionTargetFov - transitionStartFov);
            transitionDuration = Mathf.Max(minTransitionDuration, distance / Mathf.Max(zoomSpeed, 0.0001f));
            transitionElapsed = 0f;
            transitionActive = true;
        }

        private void UpdateZoomTransition()
        {
            if (!transitionActive)
                return;

            transitionElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(transitionElapsed / transitionDuration);
            float eased = Mathf.Clamp01(zoomEase.Evaluate(t));

            baseFov = Mathf.Lerp(transitionStartFov, transitionTargetFov, eased);

            if (t >= 1f)
            {
                transitionActive = false;
                baseFov = transitionTargetFov;

                if (enableFocusPulse)
                    pulseTimer = 0f;

                if (audioSource != null && audioSource.isPlaying)
                    audioSource.Stop();
            }
        }

        private void UpdateSensitivity(bool zoomingTarget)
        {
            float targetSensitivityMultiplier = zoomingTarget ? zoomSensitivityMultiplier : 1f;
            currentSensitivityMultiplier = Mathf.MoveTowards(
                currentSensitivityMultiplier,
                targetSensitivityMultiplier,
                sensitivityBlendSpeed * Time.deltaTime);

            if (mouseLook != null)
                mouseLook.SetSensitivityMultiplier(currentSensitivityMultiplier);
        }

        private void UpdateFlashlightZoomPose()
        {
            if (flashlightZoomRoot == null)
                return;

            if (!moveFlashlightWhileZoomed)
            {
                flashlightZoomRoot.localPosition = flashlightShownLocalPosition;
                return;
            }

            bool shouldHide = targetZoomed || transitionActive;
            Vector3 targetPos = shouldHide ? flashlightHiddenLocalPosition : flashlightShownLocalPosition;

            flashlightZoomRoot.localPosition = Vector3.Lerp(
                flashlightZoomRoot.localPosition,
                targetPos,
                Time.deltaTime * flashlightMoveSpeed);
        }

        private float GetFocusPulseOffset()
        {
            if (!enableFocusPulse || pulseTimer < 0f)
                return 0f;

            pulseTimer += Time.deltaTime;
            if (pulseTimer >= focusPulseDuration)
            {
                pulseTimer = -1f;
                return 0f;
            }

            float t = pulseTimer / Mathf.Max(focusPulseDuration, 0.0001f);
            float wave = Mathf.Sin(t * Mathf.PI * focusPulseCycles);
            float envelope = Mathf.Exp(-focusPulseDamping * t);

            return wave * envelope * focusPulseAmplitude;
        }

        private void PlayZoomSound(bool zoomingIn)
        {
            if (audioSource == null)
                return;

            AudioClip clip = zoomingIn ? zoomInSound : zoomOutSound;
            if (clip == null)
                return;

            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.loop = false;
            audioSource.Play();
        }

        private void Reset()
        {
            if (mouseLook == null)
                mouseLook = GetComponent<MouseLook>();

            if (mouseLook == null)
                mouseLook = GetComponentInParent<MouseLook>();

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }
    }
}