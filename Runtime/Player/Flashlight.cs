using System.Collections;
using com.first_person_horror_kit.Core;
using UnityEngine;

namespace com.first_person_horror_kit.Player
{
    public class Flashlight : MonoBehaviour
    {
        [Header("Flashlight")]
        [SerializeField] private Light flashlight;
        [SerializeField] private KeyCode toggleKey = KeyCode.F;
        [SerializeField] private bool startOn = false;

        [Header("Timing")]
        [SerializeField] private float lightFadeDuration = 0.08f;

        [Header("Warmup")]
        [Tooltip("Keeps the Light component enabled and uses intensity 0 as OFF state.")]
        [SerializeField] private bool keepLightEnabled = true;
        [Tooltip("Warms spotlight shadow resources during startup while light intensity is 0.")]
        [SerializeField] private bool warmShadowsOnStart = true;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip turnOnSound;
        [SerializeField] private AudioClip turnOffSound;

        public bool IsOn => isOn;
        public bool IsAnimating => fadeRoutine != null;

        private bool isOn;
        private float maxLightIntensity;
        private Coroutine fadeRoutine;
        private LightShadows initialShadowMode;

        private void Awake()
        {
            if (flashlight == null)
                return;

            maxLightIntensity = flashlight.intensity;
            initialShadowMode = flashlight.shadows;

            isOn = startOn;

            flashlight.intensity = isOn ? maxLightIntensity : 0f;
            flashlight.enabled = keepLightEnabled || isOn;
        }

        private IEnumerator Start()
        {
            if (turnOnSound != null)
                turnOnSound.LoadAudioData();

            if (turnOffSound != null)
                turnOffSound.LoadAudioData();

            if (flashlight == null)
                yield break;

            if (!keepLightEnabled || !warmShadowsOnStart || initialShadowMode == LightShadows.None)
                yield break;

            LightShadows previous = flashlight.shadows;
            flashlight.shadows = LightShadows.None;
            yield return null;
            flashlight.shadows = previous;
            yield return null;

            if (!isOn)
                flashlight.intensity = 0f;
        }

        private void Update()
        {
            if (GameplayState.isInputBlocked)
                return;

            if (Input.GetKeyDown(toggleKey))
                SetFlashlightState(!isOn);
        }

        private void SetFlashlightState(bool turnOn)
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(FadeFlashlight(turnOn));
            PlaySound(turnOn);
        }

        private IEnumerator FadeFlashlight(bool turnOn)
        {
            isOn = turnOn;

            if (flashlight == null)
            {
                fadeRoutine = null;
                yield break;
            }

            if (!flashlight.enabled)
                flashlight.enabled = true;

            float startIntensity = flashlight.intensity;
            float targetIntensity = turnOn ? maxLightIntensity : 0f;

            float t = 0f;
            float duration = Mathf.Max(lightFadeDuration, 0.0001f);

            while (t < duration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Clamp01(t / duration);
                flashlight.intensity = Mathf.Lerp(startIntensity, targetIntensity, alpha);
                yield return null;
            }

            flashlight.intensity = targetIntensity;

            if (!turnOn && !keepLightEnabled)
                flashlight.enabled = false;

            fadeRoutine = null;
        }

        private void PlaySound(bool isTurningOn)
        {
            if (audioSource == null)
                return;

            if (isTurningOn && turnOnSound != null)
                audioSource.PlayOneShot(turnOnSound);
            else if (!isTurningOn && turnOffSound != null)
                audioSource.PlayOneShot(turnOffSound);
        }

        private void OnValidate()
        {
            if (lightFadeDuration < 0f)
                lightFadeDuration = 0f;
        }

        private void Reset()
        {
            if (flashlight == null)
                flashlight = GetComponentInChildren<Light>(true);

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }
    }
}