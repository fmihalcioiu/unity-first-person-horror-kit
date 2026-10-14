using UnityEngine;

namespace com.first_person_horror_kit.Player
{
    [DisallowMultipleComponent]
    public class FlashlightRealism : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Light targetLight;

        [Header("Intensity Noise")]
        [Range(0f, 0.25f)][SerializeField] private float intensityVariation = 0.05f;
        [SerializeField] private float intensityNoiseSpeed = 2.2f;

        [Header("Beam Angle Noise")]
        [Range(0f, 2f)][SerializeField] private float angleVariation = 0.35f;
        [SerializeField] private float angleNoiseSpeed = 1.6f;

        [Header("Smoothing")]
        [SerializeField] private float smoothSpeed = 10f;

        private float baseIntensity;
        private float baseSpotAngle;
        private float seedA;
        private float seedB;

        private void Start()
        {
            if (targetLight == null)
                targetLight = GetComponent<Light>();

            if (targetLight == null)
                targetLight = GetComponentInChildren<Light>();

            if (targetLight == null)
                return;

            baseIntensity = targetLight.intensity;
            baseSpotAngle = targetLight.spotAngle;
            seedA = Random.value * 100f;
            seedB = Random.value * 100f;
        }

        private void LateUpdate()
        {
            if (targetLight == null || !targetLight.enabled)
                return;

            float t = Time.time;
            float n1 = Mathf.PerlinNoise(seedA, t * intensityNoiseSpeed) * 2f - 1f;
            float n2 = Mathf.PerlinNoise(seedB, t * angleNoiseSpeed) * 2f - 1f;

            float targetIntensity = baseIntensity * (1f + n1 * intensityVariation);
            float targetAngle = baseSpotAngle + n2 * angleVariation;

            targetIntensity = Mathf.Max(0f, targetIntensity);
            targetAngle = Mathf.Clamp(targetAngle, 1f, 179f);

            targetLight.intensity = Mathf.Lerp(targetLight.intensity, targetIntensity, Time.deltaTime * smoothSpeed);
            targetLight.spotAngle = Mathf.Lerp(targetLight.spotAngle, targetAngle, Time.deltaTime * smoothSpeed);
        }

        private void Reset()
        {
            if (targetLight == null)
                targetLight = GetComponent<Light>();

            if (targetLight == null)
                targetLight = GetComponentInChildren<Light>(true);
        }
    }
}