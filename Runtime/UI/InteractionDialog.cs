using System;
using com.first_person_horror_kit.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.first_person_horror_kit.UI
{
    public class InteractionDialog : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Button secondaryButton;
        [SerializeField] private TextMeshProUGUI primaryText;
        [SerializeField] private TextMeshProUGUI secondaryText;

        [Header("Default Option Labels")]
        [SerializeField] private string defaultPrimaryLabel = "Confirm";
        [SerializeField] private string defaultSecondaryLabel = "Cancel";

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;
        [Range(0f, 1f)][SerializeField] private float hoverVolume = 1f;
        [Range(0f, 1f)][SerializeField] private float clickVolume = 1f;
        [Min(0f)][SerializeField] private float hoverSoundCooldown = 0.08f;

        [Header("Keyboard Shortcuts")]
        [SerializeField] private KeyCode primaryKey = KeyCode.Return;
        [SerializeField] private KeyCode secondaryKey = KeyCode.Escape;

        [Header("Selection")]
        [SerializeField] private Color normalTextColor = new Color(1f, 1f, 1f, 0.6f);
        [SerializeField] private Color highlightedTextColor = Color.white;
        [SerializeField] private KeyCode nextOptionKey = KeyCode.RightArrow;
        [SerializeField] private KeyCode previousOptionKey = KeyCode.LeftArrow;
        [SerializeField] private KeyCode altNextOptionKey = KeyCode.DownArrow;
        [SerializeField] private KeyCode altPreviousOptionKey = KeyCode.UpArrow;

        public static bool IsOpen { get; private set; }

        private Action onPrimaryAction;
        private Action onSecondaryAction;

        private CursorLockMode previousLockState;
        private bool previousCursorVisible;

        private bool isPrimarySelected = true;
        private float lastHoverSoundTime = -999f;

        private void Awake()
        {
            if (root == null)
                root = gameObject;

            if (primaryButton != null)
                primaryButton.onClick.AddListener(ConfirmPrimary);

            if (secondaryButton != null)
                secondaryButton.onClick.AddListener(ConfirmSecondary);

            if (primaryText == null && primaryButton != null)
                primaryText = primaryButton.GetComponentInChildren<TextMeshProUGUI>(true);

            if (secondaryText == null && secondaryButton != null)
                secondaryText = secondaryButton.GetComponentInChildren<TextMeshProUGUI>(true);

            AddHoverHandlers(primaryButton, true);
            AddHoverHandlers(secondaryButton, false);

            SetOptionLabels(defaultPrimaryLabel, defaultSecondaryLabel);
            RefreshSelectionVisuals();
            HideImmediate();
        }

        private void Update()
        {
            if (!IsOpen)
                return;

            if (Input.GetKeyDown(nextOptionKey) || Input.GetKeyDown(altNextOptionKey) || Input.GetKeyDown(KeyCode.Tab))
            {
                SetSelection(false, true);
                return;
            }

            if (Input.GetKeyDown(previousOptionKey) || Input.GetKeyDown(altPreviousOptionKey))
            {
                SetSelection(true, true);
                return;
            }

            if (Input.GetKeyDown(primaryKey))
            {
                if (isPrimarySelected)
                    ConfirmPrimary();
                else
                    ConfirmSecondary();

                return;
            }

            if (Input.GetKeyDown(secondaryKey))
            {
                ConfirmSecondary();
                return;
            }
        }

        public void Show(string message, Action onPrimary, Action onSecondary = null, string primaryLabel = null, string secondaryLabel = null)
        {
            if (root == null)
                return;

            onPrimaryAction = onPrimary;
            onSecondaryAction = onSecondary;

            if (messageText != null)
                messageText.text = message;

            SetOptionLabels(primaryLabel, secondaryLabel);

            previousLockState = Cursor.lockState;
            previousCursorVisible = Cursor.visible;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = !SoftwareCursor.IsRegistered;

            SetSelection(true, false);

            root.SetActive(true);
            IsOpen = true;
            GameplayState.SetInputBlocked(true);
        }

        public void ConfirmPrimary()
        {
            Action action = onPrimaryAction;
            Hide();
            action?.Invoke();
        }

        public void ConfirmSecondary()
        {
            PlayClickSound();

            Action action = onSecondaryAction;
            Hide();
            action?.Invoke();
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);

            Cursor.lockState = previousLockState;
            Cursor.visible = SoftwareCursor.IsRegistered ? false : previousCursorVisible;

            onPrimaryAction = null;
            onSecondaryAction = null;
            IsOpen = false;
            GameplayState.SetInputBlocked(false);
        }

        private void HideImmediate()
        {
            if (root != null)
                root.SetActive(false);

            onPrimaryAction = null;
            onSecondaryAction = null;
            IsOpen = false;
            GameplayState.SetInputBlocked(false);
        }

        private void SetOptionLabels(string primaryLabel, string secondaryLabel)
        {
            string resolvedPrimary = string.IsNullOrWhiteSpace(primaryLabel) ? defaultPrimaryLabel : primaryLabel;
            string resolvedSecondary = string.IsNullOrWhiteSpace(secondaryLabel) ? defaultSecondaryLabel : secondaryLabel;

            if (primaryText != null)
                primaryText.text = resolvedPrimary;

            if (secondaryText != null)
                secondaryText.text = resolvedSecondary;
        }

        private void RefreshSelectionVisuals()
        {
            if (primaryText != null)
                primaryText.color = isPrimarySelected ? highlightedTextColor : normalTextColor;

            if (secondaryText != null)
                secondaryText.color = isPrimarySelected ? normalTextColor : highlightedTextColor;
        }

        private void AddHoverHandlers(Button button, bool selectPrimary)
        {
            if (button == null)
                return;

            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener(_ => SetSelection(selectPrimary, true));
            trigger.triggers.Add(enterEntry);

            EventTrigger.Entry selectEntry = new EventTrigger.Entry();
            selectEntry.eventID = EventTriggerType.Select;
            selectEntry.callback.AddListener(_ => SetSelection(selectPrimary, true));
            trigger.triggers.Add(selectEntry);
        }

        private void SetSelection(bool selectPrimary, bool playHoverIfChanged)
        {
            bool changed = isPrimarySelected != selectPrimary;
            isPrimarySelected = selectPrimary;
            RefreshSelectionVisuals();

            if (changed && playHoverIfChanged)
                PlayHoverSound();
        }

        private void PlayHoverSound()
        {
            if (audioSource == null || hoverSound == null)
                return;

            float now = Time.unscaledTime;
            if (now - lastHoverSoundTime < hoverSoundCooldown)
                return;

            lastHoverSoundTime = now;
            audioSource.PlayOneShot(hoverSound, hoverVolume);
        }

        private void PlayClickSound()
        {
            if (audioSource == null || clickSound == null)
                return;

            audioSource.PlayOneShot(clickSound, clickVolume);
        }

        private void OnValidate()
        {
            if (root == null)
                root = gameObject;

            if (primaryText == null && primaryButton != null)
                primaryText = primaryButton.GetComponentInChildren<TextMeshProUGUI>(true);

            if (secondaryText == null && secondaryButton != null)
                secondaryText = secondaryButton.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        private void Reset()
        {
            if (root == null)
                root = gameObject;

            if (primaryText == null && primaryButton != null)
                primaryText = primaryButton.GetComponentInChildren<TextMeshProUGUI>(true);

            if (secondaryText == null && secondaryButton != null)
                secondaryText = secondaryButton.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }
}