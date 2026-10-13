using System;
using System.Collections.Generic;
using com.first_person_horror_kit.Core;
using UnityEngine;

namespace com.first_person_horror_kit.Environment
{
    [RequireComponent(typeof(AudioSource))]
    public class LightSwitch : MonoBehaviour, IInteractable, IInteractionPromptSource
    {
        [Header("Identity")]
        [SerializeField] private string displayName = "Light Switch";
        [SerializeField] private string interactKeyLabel = "E";
        [SerializeField] private string switchId = "house.hallway";

        [Header("State")]
        [SerializeField] private bool startOn = true;

        [Header("Targets")]
        [SerializeField] private Light[] targetLights;
        [Tooltip("Renderers that contain emissive materials to toggle.")]
        [SerializeField] private Renderer[] emissiveRenderers;

        [Header("Rotation")]
        [SerializeField] private bool rotateObjectOnToggle = true;
        [Tooltip("Optional. Assign only the lever/switch child you want rotated.")]
        [SerializeField] private Transform switchChild;
        [SerializeField] private float offLocalZRotationOffset = 180f;

        [Header("Audio")]
        [SerializeField] private AudioClip turnOnSound;
        [SerializeField] private AudioClip turnOffSound;
        [SerializeField] private Vector2 clickPitchRange = new Vector2(0.98f, 1.02f);

        private readonly List<EmissiveMaterialState> emissiveMaterials = new List<EmissiveMaterialState>();
        private readonly List<LightRuntimeState> lightStates = new List<LightRuntimeState>();

        private AudioSource audioSource;
        private bool isOn;
        private bool canRotateSwitchVisual;
        private Quaternion baseChildLocalRotation;

        public string SwitchId => switchId;

        public event Action PromptChanged;

        private struct EmissiveMaterialState
        {
            public Material Material;
            public Color OnEmissionColor;
            public bool EmissionKeywordEnabled;
        }

        private struct LightRuntimeState
        {
            public Light Light;
            public float OnIntensity;
        }

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            canRotateSwitchVisual = rotateObjectOnToggle && switchChild != null;
            if (canRotateSwitchVisual)
                baseChildLocalRotation = switchChild.localRotation;

            CacheLightStates();
            isOn = startOn;
            ApplyRotation();
        }

        private void Start()
        {
            CacheEmissiveMaterials();
            ApplyState();
        }

        public void Interact()
        {
            if (GameplayState.isInputBlocked)
                return;

            SetIsOn(!isOn, true);
        }

        public void SetIsOn(bool turnOn, bool playSound)
        {
            if (isOn == turnOn)
                return;

            isOn = turnOn;
            ApplyState();

            if (playSound)
                PlaySwitchSound(isOn);

            PromptChanged?.Invoke();
        }

        public string GetPrompt()
        {
            return isOn
                ? $"[{interactKeyLabel}] TURN OFF"
                : $"[{interactKeyLabel}] TURN ON";
        }

        public string GetDisplayName()
        {
            return displayName;
        }

        private void ApplyState()
        {
            for (int i = 0; i < lightStates.Count; i++)
            {
                LightRuntimeState state = lightStates[i];
                if (state.Light == null)
                    continue;

                if (isOn)
                {
                    state.Light.enabled = true;
                    state.Light.intensity = state.OnIntensity;
                }
                else
                {
                    state.Light.intensity = 0f;
                    state.Light.enabled = false;
                }
            }

            for (int i = 0; i < emissiveMaterials.Count; i++)
            {
                EmissiveMaterialState state = emissiveMaterials[i];
                if (state.Material == null)
                    continue;

                if (isOn)
                {
                    state.Material.SetColor("_EmissionColor", state.OnEmissionColor);

                    if (state.EmissionKeywordEnabled || state.OnEmissionColor.maxColorComponent > 0.0001f)
                        state.Material.EnableKeyword("_EMISSION");
                }
                else
                {
                    state.Material.SetColor("_EmissionColor", Color.black);
                    state.Material.DisableKeyword("_EMISSION");
                }
            }

            ApplyRotation();
        }

        private void ApplyRotation()
        {
            if (!canRotateSwitchVisual)
                return;

            switchChild.localRotation = isOn
                ? baseChildLocalRotation
                : baseChildLocalRotation * Quaternion.Euler(0f, 0f, offLocalZRotationOffset);
        }

        private void CacheLightStates()
        {
            lightStates.Clear();

            if (targetLights == null)
                return;

            for (int i = 0; i < targetLights.Length; i++)
            {
                Light lightTarget = targetLights[i];
                if (lightTarget == null)
                    continue;

                LightRuntimeState state = new LightRuntimeState
                {
                    Light = lightTarget,
                    OnIntensity = Mathf.Max(0f, lightTarget.intensity)
                };

                lightStates.Add(state);
            }
        }

        private void CacheEmissiveMaterials()
        {
            emissiveMaterials.Clear();

            if (emissiveRenderers == null)
                return;

            for (int i = 0; i < emissiveRenderers.Length; i++)
            {
                Renderer rendererTarget = emissiveRenderers[i];
                if (rendererTarget == null)
                    continue;

                Material[] materials = rendererTarget.materials;
                for (int m = 0; m < materials.Length; m++)
                {
                    Material mat = materials[m];
                    if (mat == null)
                        continue;

                    if (!mat.HasProperty("_EmissionColor"))
                        continue;

                    EmissiveMaterialState entry = new EmissiveMaterialState
                    {
                        Material = mat,
                        OnEmissionColor = mat.GetColor("_EmissionColor"),
                        EmissionKeywordEnabled = mat.IsKeywordEnabled("_EMISSION")
                    };

                    emissiveMaterials.Add(entry);
                }
            }
        }

        private void PlaySwitchSound(bool turnedOn)
        {
            if (audioSource == null)
                return;

            AudioClip clip = turnedOn ? turnOnSound : turnOffSound;
            if (clip == null)
                return;

            audioSource.pitch = UnityEngine.Random.Range(clickPitchRange.x, clickPitchRange.y);
            audioSource.PlayOneShot(clip);
        }

        private void Reset()
        {
            if (switchChild == null)
                switchChild = transform;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(displayName))
                Debug.LogWarning($"{name}: displayName is empty.", this);

            if (string.IsNullOrWhiteSpace(switchId))
                Debug.LogWarning($"{name}: switchId is empty.", this);
        }
    }
}