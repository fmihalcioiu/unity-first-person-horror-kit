using com.first_person_horror_kit.Core;
using UnityEngine;

namespace com.first_person_horror_kit.Systems
{
    public class InteractionController : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField] private float interactDistance = 3f;
        [SerializeField] private LayerMask interactLayer = ~0;
        [SerializeField] private Transform rayOrigin;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private readonly RaycastHit[] raycastHits = new RaycastHit[32];

        private void Update()
        {
            if (GameplayState.isInputBlocked)
                return;

            if (!Input.GetKeyDown(interactKey))
                return;

            if (!TryGetNearestInteractable(out IInteractable interactable))
                return;

            interactable.Interact();
        }

        private bool TryGetNearestInteractable(out IInteractable interactable)
        {
            interactable = null;

            if (rayOrigin == null)
                return false;

            Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

            int hitCount = Physics.RaycastNonAlloc(
                ray,
                raycastHits,
                interactDistance,
                ~0,
                QueryTriggerInteraction.Ignore);

            if (hitCount <= 0)
                return false;

            RaycastHit nearestHit = default;
            float nearestDistance = float.MaxValue;
            bool foundHit = false;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = raycastHits[i];
                if (hit.collider == null)
                    continue;

                if (hit.collider.transform.root == transform.root)
                    continue;

                if (!IsLayerInMask(hit.collider.gameObject.layer, interactLayer))
                    continue;

                if (hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    nearestHit = hit;
                    foundHit = true;
                }
            }

            if (!foundHit)
                return false;

            IInteractable nearestInteractable = nearestHit.collider.GetComponentInParent<IInteractable>();
            if (nearestInteractable == null)
                return false;

            interactable = nearestInteractable;
            return true;
        }

        private void Reset()
        {
            if (rayOrigin == null)
                rayOrigin = transform;
        }

        private void OnValidate()
        {
            if (interactDistance < 0.01f)
                interactDistance = 0.01f;
        }

        private static bool IsLayerInMask(int layer, LayerMask mask)
        {
            return ((1 << layer) & mask.value) != 0;
        }
    }
}