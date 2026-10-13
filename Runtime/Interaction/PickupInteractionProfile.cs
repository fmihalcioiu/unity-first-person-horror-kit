using UnityEngine;

namespace com.first_person_horror_kit.Interaction
{
    [CreateAssetMenu(fileName = "PickupInteractionProfile", menuName = "com.first_person_horror_kit/Interaction/Pickup Interaction Profile")]
    public class PickupInteractionProfile : ScriptableObject
    {
        [SerializeField] private string prompt = "[E] PICK UP";
        [SerializeField] private string confirmationPromptFormat = "Pick up {0}?";
        [SerializeField] private string primaryOptionText = "Take";
        [SerializeField] private string secondaryOptionText = "Leave";

        public string Prompt => prompt;
        public string PrimaryOptionText => primaryOptionText;
        public string SecondaryOptionText => secondaryOptionText;

        public string BuildConfirmationPrompt(string displayName)
        {
            if (string.IsNullOrWhiteSpace(confirmationPromptFormat))
                return null;

            if (confirmationPromptFormat.Contains("{0}"))
                return string.Format(confirmationPromptFormat, displayName);

            return confirmationPromptFormat;
        }
    }
}