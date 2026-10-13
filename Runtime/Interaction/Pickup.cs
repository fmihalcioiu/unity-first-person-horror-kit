using com.first_person_horror_kit.Core;
using UnityEngine;
using UnityEngine.Events;

namespace com.first_person_horror_kit.Interaction
{
    public class Pickup : MonoBehaviour, IInteractable, IConfirmableInteractable
    {
        [SerializeField] private string displayName = "Test Item";

        [Header("Interaction Profile")]
        [SerializeField] private PickupInteractionProfile interactionProfile;

        [Header("Confirmation")]
        [SerializeField] private bool requireConfirmation = true;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip pickupSound;

        [Header("Events")]
        [SerializeField] private UnityEvent onPickedUp;

        public void Interact()
        {
            if (GameplayState.isInputBlocked)
                return;

            Debug.Log($"Picked up {GetDisplayName()}");

            if (audioSource != null && pickupSound != null)
                audioSource.PlayOneShot(pickupSound);

            onPickedUp?.Invoke();

            Destroy(gameObject);
        }

        public string GetPrompt()
        {
            return interactionProfile != null ? interactionProfile.Prompt : "[E] PICK UP";
        }

        public string GetDisplayName()
        {
            return displayName;
        }

        public string GetConfirmationPrompt()
        {
            if (!requireConfirmation)
                return null;

            if (interactionProfile == null)
                return $"Pick up {GetDisplayName()}?";

            return interactionProfile.BuildConfirmationPrompt(GetDisplayName());
        }

        public string GetPrimaryOptionText()
        {
            return interactionProfile != null ? interactionProfile.PrimaryOptionText : "Take";
        }

        public string GetSecondaryOptionText()
        {
            return interactionProfile != null ? interactionProfile.SecondaryOptionText : "Leave";
        }

        private void Reset()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(displayName))
                Debug.LogWarning($"{name}: displayName is empty.", this);

            if (interactionProfile == null)
                Debug.LogWarning($"{name}: interactionProfile is not assigned.", this);
        }
    }
}