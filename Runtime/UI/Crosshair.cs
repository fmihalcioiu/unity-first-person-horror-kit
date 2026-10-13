using com.first_person_horror_kit.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.first_person_horror_kit.UI
{
    public class Crosshair : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image crosshairImage;
        [SerializeField] private TextMeshProUGUI promptText;

        [Header("Colors")]
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color interactColor = Color.green;

        [Header("Settings")]
        [SerializeField] private float interactDistance = 3f;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private LayerMask interactMask = ~0;

        private IInteractable currentInteractable;

        private void Start()
        {
            HidePrompt();

            if (crosshairImage != null)
                crosshairImage.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (crosshairImage == null)
                return;

            if (GameplayState.isInputBlocked)
            {
                currentInteractable = null;
                crosshairImage.color = defaultColor;
                HidePrompt();
                return;
            }

            if (playerCamera == null)
                return;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            currentInteractable = null;

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Ignore))
                currentInteractable = hit.collider.GetComponentInParent<IInteractable>();

            if (currentInteractable != null)
            {
                crosshairImage.color = interactColor;
                ShowPrompt(currentInteractable.GetPrompt());
            }
            else
            {
                crosshairImage.color = defaultColor;
                HidePrompt();
            }
        }

        private void ShowPrompt(string text)
        {
            if (promptText == null)
                return;

            promptText.text = text;
            promptText.gameObject.SetActive(true);
        }

        private void HidePrompt()
        {
            if (promptText == null)
                return;

            promptText.gameObject.SetActive(false);
        }

        public void TryInteract()
        {
            if (currentInteractable != null)
                currentInteractable.Interact();
        }

        private void Reset()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        private void OnValidate()
        {
            if (interactDistance < 0.01f)
                interactDistance = 0.01f;
        }
    }
}