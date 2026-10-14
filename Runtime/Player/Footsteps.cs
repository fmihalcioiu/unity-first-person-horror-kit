using System.Collections.Generic;
using com.first_person_horror_kit.Core;
using UnityEngine;

namespace com.first_person_horror_kit.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class Footsteps : MonoBehaviour
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource footstepSource;

        [Header("Profile (Required)")]
        [SerializeField] private FootstepProfile profile;

        [Header("Surface Detection")]
        [SerializeField] private LayerMask surfaceMask = ~0;
        [SerializeField] private float surfaceCheckPadding = 0.25f;
        [SerializeField] private float surfaceRefreshInterval = 0.1f;

        [Header("Step Timing")]
        [SerializeField] private float walkStepInterval = 0.5f;
        [SerializeField] private float sprintStepInterval = 0.32f;

        [Header("Pitch")]
        [SerializeField] private Vector2 walkPitchRange = new Vector2(0.9f, 1.1f);
        [SerializeField] private Vector2 sprintPitchRange = new Vector2(1.0f, 1.2f);

        private CharacterController controller;
        private PlayerController playerController;
        private float stepTimer;
        private float surfaceRefreshTimer;
        private int lastClipIndex = -1;

        private PhysicsMaterial currentGroundMaterial;
        private TerrainLayer currentTerrainLayer;

        private readonly Dictionary<PhysicsMaterial, SurfaceFootstepSet> surfaceSetByMaterial = new Dictionary<PhysicsMaterial, SurfaceFootstepSet>();
        private readonly Dictionary<TerrainLayer, TerrainFootstepSet> terrainSetByLayer = new Dictionary<TerrainLayer, TerrainFootstepSet>();

        private Terrain cachedTerrain;
        private TerrainData cachedTerrainData;
        private int cachedMapX = -1;
        private int cachedMapZ = -1;
        private TerrainLayer cachedDominantLayer;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            playerController = GetComponent<PlayerController>();

            if (footstepSource == null)
                footstepSource = GetComponent<AudioSource>();

            RebuildLookups();
        }

        private void Start()
        {
            stepTimer = walkStepInterval;
            surfaceRefreshTimer = 0f;
        }

        private void Update()
        {
            if (GameplayState.isInputBlocked)
                return;

            if (profile == null || controller == null || footstepSource == null)
                return;

            if (playerController == null)
                return;

            Vector2 moveInput = playerController.MoveInput;
            bool isMoving = Mathf.Abs(moveInput.x) > 0.1f || Mathf.Abs(moveInput.y) > 0.1f;
            bool isSprinting = playerController.IsSprinting;
            float currentInterval = isSprinting ? sprintStepInterval : walkStepInterval;

            if (controller.isGrounded && isMoving)
            {
                surfaceRefreshTimer -= Time.deltaTime;
                if (surfaceRefreshTimer <= 0f)
                {
                    UpdateGroundSurface();
                    surfaceRefreshTimer = surfaceRefreshInterval;
                }

                stepTimer -= Time.deltaTime;
                if (stepTimer <= 0f)
                {
                    PlayFootstep(isSprinting);
                    stepTimer = currentInterval;
                }
            }
            else
            {
                stepTimer = currentInterval;
                surfaceRefreshTimer = 0f;
                currentGroundMaterial = null;
                currentTerrainLayer = null;
                ClearTerrainSampleCache();
            }
        }

        private void UpdateGroundSurface()
        {
            Vector3 origin = transform.position + controller.center + Vector3.up * 0.05f;
            float castDistance = (controller.height * 0.5f) + surfaceCheckPadding;

            if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, castDistance, surfaceMask, QueryTriggerInteraction.Ignore))
            {
                currentGroundMaterial = null;
                currentTerrainLayer = null;
                return;
            }

            currentGroundMaterial = hit.collider.sharedMaterial;
            currentTerrainLayer = null;

            Terrain terrain = hit.collider.GetComponent<Terrain>();
            if (terrain == null)
                terrain = hit.collider.GetComponentInParent<Terrain>();

            if (terrain != null)
                currentTerrainLayer = GetDominantTerrainLayer(terrain, hit.point);
        }

        private TerrainLayer GetDominantTerrainLayer(Terrain terrain, Vector3 worldPosition)
        {
            if (terrain == null)
                return null;

            TerrainData terrainData = terrain.terrainData;
            if (terrainData == null || terrainData.terrainLayers == null || terrainData.terrainLayers.Length == 0)
                return null;

            Vector3 terrainLocalPos = worldPosition - terrain.transform.position;
            float normalizedX = terrainData.size.x > 0f ? terrainLocalPos.x / terrainData.size.x : 0f;
            float normalizedZ = terrainData.size.z > 0f ? terrainLocalPos.z / terrainData.size.z : 0f;

            int mapX = Mathf.Clamp(Mathf.RoundToInt(normalizedX * (terrainData.alphamapWidth - 1)), 0, terrainData.alphamapWidth - 1);
            int mapZ = Mathf.Clamp(Mathf.RoundToInt(normalizedZ * (terrainData.alphamapHeight - 1)), 0, terrainData.alphamapHeight - 1);

            if (cachedTerrain == terrain && cachedTerrainData == terrainData && cachedMapX == mapX && cachedMapZ == mapZ)
                return cachedDominantLayer;

            float[,,] splat = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
            if (splat == null)
                return null;

            int dominantLayerIndex = -1;
            float dominantWeight = -1f;

            for (int i = 0; i < terrainData.terrainLayers.Length; i++)
            {
                float weight = splat[0, 0, i];
                if (weight > dominantWeight)
                {
                    dominantWeight = weight;
                    dominantLayerIndex = i;
                }
            }

            TerrainLayer dominant = null;
            if (dominantLayerIndex >= 0 && dominantLayerIndex < terrainData.terrainLayers.Length)
                dominant = terrainData.terrainLayers[dominantLayerIndex];

            cachedTerrain = terrain;
            cachedTerrainData = terrainData;
            cachedMapX = mapX;
            cachedMapZ = mapZ;
            cachedDominantLayer = dominant;

            return dominant;
        }

        private void PlayFootstep(bool isSprinting)
        {
            AudioClip[] activeClips = ResolveClipsForCurrentSurface(isSprinting);
            if (activeClips == null || activeClips.Length == 0)
                return;

            int index = Random.Range(0, activeClips.Length);
            if (activeClips.Length > 1 && index == lastClipIndex)
                index = (index + 1) % activeClips.Length;

            lastClipIndex = index;

            Vector2 pitchRange = isSprinting ? sprintPitchRange : walkPitchRange;
            footstepSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
            footstepSource.PlayOneShot(activeClips[index]);
        }

        private AudioClip[] ResolveClipsForCurrentSurface(bool isSprinting)
        {
            TerrainFootstepSet terrainSet = GetTerrainSet(currentTerrainLayer);
            AudioClip[] terrainClips = ResolveSetClips(
                terrainSet != null ? terrainSet.WalkClips : null,
                terrainSet != null ? terrainSet.SprintClips : null,
                isSprinting);

            if (terrainClips != null && terrainClips.Length > 0)
                return terrainClips;

            SurfaceFootstepSet surfaceSet = GetSurfaceSet(currentGroundMaterial);
            AudioClip[] surfaceClips = ResolveSetClips(
                surfaceSet != null ? surfaceSet.WalkClips : null,
                surfaceSet != null ? surfaceSet.SprintClips : null,
                isSprinting);

            if (surfaceClips != null && surfaceClips.Length > 0)
                return surfaceClips;

            return ResolveSetClips(profile.WalkFootstepClips, profile.SprintFootstepClips, isSprinting);
        }

        private AudioClip[] ResolveSetClips(AudioClip[] walkClips, AudioClip[] sprintClips, bool isSprinting)
        {
            AudioClip[] clips = isSprinting ? sprintClips : walkClips;

            if ((clips == null || clips.Length == 0) && isSprinting)
                clips = walkClips;

            return clips;
        }

        private SurfaceFootstepSet GetSurfaceSet(PhysicsMaterial material)
        {
            if (material == null || surfaceSetByMaterial.Count == 0)
                return null;

            surfaceSetByMaterial.TryGetValue(material, out SurfaceFootstepSet set);
            return set;
        }

        private TerrainFootstepSet GetTerrainSet(TerrainLayer layer)
        {
            if (layer == null || terrainSetByLayer.Count == 0)
                return null;

            terrainSetByLayer.TryGetValue(layer, out TerrainFootstepSet set);
            return set;
        }

        private void RebuildLookups()
        {
            surfaceSetByMaterial.Clear();
            terrainSetByLayer.Clear();

            if (profile == null)
                return;

            SurfaceFootstepSet[] surfaceSets = profile.SurfaceFootsteps;
            if (surfaceSets != null)
            {
                for (int i = 0; i < surfaceSets.Length; i++)
                {
                    SurfaceFootstepSet set = surfaceSets[i];
                    if (set == null || set.PhysicsMaterial == null)
                        continue;

                    surfaceSetByMaterial[set.PhysicsMaterial] = set;
                }
            }

            TerrainFootstepSet[] terrainSets = profile.TerrainFootsteps;
            if (terrainSets != null)
            {
                for (int i = 0; i < terrainSets.Length; i++)
                {
                    TerrainFootstepSet set = terrainSets[i];
                    if (set == null || set.TerrainLayers == null)
                        continue;

                    for (int j = 0; j < set.TerrainLayers.Length; j++)
                    {
                        TerrainLayer layer = set.TerrainLayers[j];
                        if (layer == null)
                            continue;

                        terrainSetByLayer[layer] = set;
                    }
                }
            }
        }

        private void ClearTerrainSampleCache()
        {
            cachedTerrain = null;
            cachedTerrainData = null;
            cachedMapX = -1;
            cachedMapZ = -1;
            cachedDominantLayer = null;
        }

        private void Reset()
        {
            if (footstepSource == null)
                footstepSource = GetComponent<AudioSource>();

            if (playerController == null)
                playerController = GetComponent<PlayerController>();
        }

        private void OnValidate()
        {
            if (walkStepInterval < 0.01f)
                walkStepInterval = 0.01f;

            if (sprintStepInterval < 0.01f)
                sprintStepInterval = 0.01f;

            if (surfaceRefreshInterval < 0.01f)
                surfaceRefreshInterval = 0.01f;

            if (profile == null)
                Debug.LogWarning($"{name}: Footstep profile is not assigned.", this);

            RebuildLookups();
        }
    }
}