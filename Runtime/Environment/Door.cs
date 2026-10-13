using com.first_person_horror_kit.Core;
using UnityEngine;

namespace com.first_person_horror_kit.Environment
{
    public class Door : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Door";
        [SerializeField] private Animator animator;

        [Header("Open State")]
        [SerializeField] private bool objectOpen;
        [SerializeField] private GameObject animateAdditional;

        [Header("Lock")]
        [SerializeField] private bool isLocked;
        [SerializeField] private string lockedPrompt = "Locked";

        [Header("Audio")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip lockedSound;

        private BoxCollider doorCollider;
        private AudioSource audioSource;

        private Door additionalDoor;
        private Animator additionalAnimator;

        private bool colliderCurrentlyEnabled = true;

        private void Awake()
        {
            ResolveComponents();
        }

        private void Start()
        {
            ApplyInitialState();
        }

        private void Update()
        {
            SyncCollisionWithAnimator();
        }

        public void Interact()
        {
            if (isLocked)
            {
                PlayLockedSound();
                return;
            }

            if (animator == null)
                return;

            if (IsAnimatorBusy(animator))
                return;

            bool nextOpenState = !objectOpen;
            SetDoorState(nextOpenState, true);

            if (additionalDoor != null && additionalDoor != this)
            {
                additionalDoor.SetDoorStateFromLinked(nextOpenState);
            }
            else if (additionalAnimator != null)
            {
                if (!IsAnimatorBusy(additionalAnimator))
                    additionalAnimator.Play(nextOpenState ? "Open" : "Close", 0, 0f);
            }
        }

        private void SetDoorState(bool open, bool playAudio)
        {
            objectOpen = open;

            if (animator != null)
                animator.Play(open ? "Open" : "Close", 0, 0f);

            if (playAudio)
                PlaySound(open);
        }

        private void SetDoorStateFromLinked(bool open)
        {
            if (animator == null || objectOpen == open || IsAnimatorBusy(animator))
                return;

            SetDoorState(open, false);
        }

        private void ApplyInitialState()
        {
            if (animator == null)
                return;

            if (objectOpen)
                animator.Play("Open", 0, 1f);
            else
                animator.Play("Close", 0, 1f);
        }

        private void SyncCollisionWithAnimator()
        {
            if (doorCollider == null)
                return;

            bool shouldEnableCollider = !IsAnimatorBusy(animator);
            if (shouldEnableCollider == colliderCurrentlyEnabled)
                return;

            doorCollider.enabled = shouldEnableCollider;
            colliderCurrentlyEnabled = shouldEnableCollider;
        }

        private bool IsAnimatorBusy(Animator targetAnimator)
        {
            if (targetAnimator == null)
                return false;

            const int layer = 0;

            if (targetAnimator.IsInTransition(layer))
                return true;

            AnimatorStateInfo state = targetAnimator.GetCurrentAnimatorStateInfo(layer);
            return !state.loop && state.normalizedTime < 1f;
        }

        private void PlaySound(bool opening)
        {
            if (audioSource == null)
                return;

            if (opening && openSound != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(openSound);
            }
            else if (!opening && closeSound != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(closeSound);
            }
        }

        private void PlayLockedSound()
        {
            if (audioSource == null || lockedSound == null)
                return;

            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(lockedSound);
        }

        public string GetPrompt()
        {
            if (isLocked)
                return string.IsNullOrWhiteSpace(lockedPrompt) ? "Locked" : lockedPrompt;

            return objectOpen ? "[E] CLOSE" : "[E] OPEN";
        }

        public string GetDisplayName()
        {
            return displayName;
        }

        public void SetLocked(bool locked)
        {
            isLocked = locked;
        }

        public bool IsLocked()
        {
            return isLocked;
        }

        private void ResolveComponents()
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            if (animator == null)
                animator = GetComponentInParent<Animator>();

            if (doorCollider == null)
                doorCollider = GetComponent<BoxCollider>();

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            additionalDoor = null;
            additionalAnimator = null;

            if (animateAdditional != null)
            {
                additionalDoor = animateAdditional.GetComponent<Door>();
                additionalAnimator = animateAdditional.GetComponent<Animator>();
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(displayName))
                Debug.LogWarning($"{name}: displayName is empty.", this);

            ResolveComponents();
        }

        private void Reset()
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            if (animator == null)
                animator = GetComponentInParent<Animator>();

            if (doorCollider == null)
                doorCollider = GetComponent<BoxCollider>();

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }
    }
}