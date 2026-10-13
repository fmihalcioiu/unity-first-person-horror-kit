using System.Collections.Generic;
using UnityEngine;

namespace com.first_person_horror_kit.UI
{
    public class SoftwareCursor : MonoBehaviour
    {
        private static readonly List<SoftwareCursor> instances = new List<SoftwareCursor>();

        public static bool IsRegistered
        {
            get
            {
                for (int i = 0; i < instances.Count; i++)
                {
                    SoftwareCursor cursor = instances[i];
                    if (cursor != null && cursor.isActiveAndEnabled)
                        return true;
                }

                return false;
            }
        }

        public static SoftwareCursor Instance
        {
            get
            {
                for (int i = 0; i < instances.Count; i++)
                {
                    SoftwareCursor cursor = instances[i];
                    if (cursor != null && cursor.isActiveAndEnabled)
                        return cursor;
                }

                for (int i = 0; i < instances.Count; i++)
                {
                    SoftwareCursor cursor = instances[i];
                    if (cursor != null)
                        return cursor;
                }

                return null;
            }
        }

        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform cursorRect;
        [Tooltip("Top-level cursor-only root under the same Canvas. This object is moved to the end of siblings.")]
        [SerializeField] private RectTransform cursorRoot;

        [Header("Behavior")]
        [SerializeField] private bool hideWhenLocked = true;
        [SerializeField] private bool clampToCanvas = true;
        [SerializeField] private bool keepOnTopWhileVisible = true;
        [SerializeField] private Vector2 hotspotPixels = Vector2.zero;

        private Camera eventCamera;

        private void Awake()
        {
            RegisterInstance();

            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();

            if (cursorRoot == null && cursorRect != null)
                cursorRoot = cursorRect.parent as RectTransform;

            RefreshCamera();
        }

        private void OnEnable()
        {
            RegisterInstance();
            Cursor.visible = false;
            RefreshCamera();
        }

        private void OnDisable()
        {
            if (cursorRect != null)
                cursorRect.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            UnregisterInstance();
        }

        public static void SetExclusive(SoftwareCursor target)
        {
            for (int i = 0; i < instances.Count; i++)
            {
                SoftwareCursor cursor = instances[i];
                if (cursor == null)
                    continue;

                bool shouldEnable = target != null && ReferenceEquals(cursor, target);
                if (cursor.enabled != shouldEnable)
                    cursor.enabled = shouldEnable;
            }

            if (target != null)
                target.RefreshCamera();
        }

        public void RefreshCamera()
        {
            if (canvas == null)
            {
                eventCamera = null;
                return;
            }

            eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : (canvas.worldCamera != null ? canvas.worldCamera : Camera.main);
        }

        private void LateUpdate()
        {
            if (canvas == null || cursorRect == null)
                return;

            bool shouldShow = !hideWhenLocked || Cursor.lockState != CursorLockMode.Locked;
            if (cursorRect.gameObject.activeSelf != shouldShow)
                cursorRect.gameObject.SetActive(shouldShow);

            if (!shouldShow)
                return;

            if (keepOnTopWhileVisible && cursorRoot != null)
                cursorRoot.SetAsLastSibling();

            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect == null)
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    Input.mousePosition,
                    eventCamera,
                    out Vector2 localPoint))
                return;

            localPoint += hotspotPixels;

            if (clampToCanvas)
            {
                Vector2 halfCanvas = canvasRect.rect.size * 0.5f;
                Vector2 cursorSize = cursorRect.rect.size;

                float minX = -halfCanvas.x + (cursorSize.x * cursorRect.pivot.x);
                float maxX = halfCanvas.x - (cursorSize.x * (1f - cursorRect.pivot.x));
                float minY = -halfCanvas.y + (cursorSize.y * cursorRect.pivot.y);
                float maxY = halfCanvas.y - (cursorSize.y * (1f - cursorRect.pivot.y));

                localPoint.x = Mathf.Clamp(localPoint.x, minX, maxX);
                localPoint.y = Mathf.Clamp(localPoint.y, minY, maxY);
            }

            cursorRect.anchoredPosition = localPoint;
        }

        private void RegisterInstance()
        {
            if (!instances.Contains(this))
                instances.Add(this);
        }

        private void UnregisterInstance()
        {
            instances.Remove(this);
        }

        private void OnValidate()
        {
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();
        }

        private void Reset()
        {
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();

            if (cursorRoot == null && cursorRect != null)
                cursorRoot = cursorRect.parent as RectTransform;
        }
    }
}