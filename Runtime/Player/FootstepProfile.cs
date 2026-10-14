using UnityEngine;

namespace com.first_person_horror_kit.Player
{
    [System.Serializable]
    public class SurfaceFootstepSet
    {
        [SerializeField] private string displayName;
        [SerializeField] private PhysicsMaterial physicsMaterial;
        [SerializeField] private AudioClip[] walkClips;
        [SerializeField] private AudioClip[] sprintClips;

        public string DisplayName => displayName;
        public PhysicsMaterial PhysicsMaterial => physicsMaterial;
        public AudioClip[] WalkClips => walkClips;
        public AudioClip[] SprintClips => sprintClips;
    }

    [System.Serializable]
    public class TerrainFootstepSet
    {
        [SerializeField] private string displayName;
        [SerializeField] private TerrainLayer[] terrainLayers;
        [SerializeField] private AudioClip[] walkClips;
        [SerializeField] private AudioClip[] sprintClips;

        public string DisplayName => displayName;
        public TerrainLayer[] TerrainLayers => terrainLayers;
        public AudioClip[] WalkClips => walkClips;
        public AudioClip[] SprintClips => sprintClips;
    }

    [CreateAssetMenu(fileName = "FootstepProfile", menuName = "com.first_person_horror_kit/Audio/Footstep Profile")]
    public class FootstepProfile : ScriptableObject
    {
        [Header("Default Clips")]
        [SerializeField] private AudioClip[] walkFootstepClips;
        [SerializeField] private AudioClip[] sprintFootstepClips;

        [Header("Surface Overrides (by PhysicsMaterial)")]
        [SerializeField] private SurfaceFootstepSet[] surfaceFootsteps;

        [Header("Terrain Overrides (grouped TerrainLayers)")]
        [SerializeField] private TerrainFootstepSet[] terrainFootsteps;

        public AudioClip[] WalkFootstepClips => walkFootstepClips;
        public AudioClip[] SprintFootstepClips => sprintFootstepClips;
        public SurfaceFootstepSet[] SurfaceFootsteps => surfaceFootsteps;
        public TerrainFootstepSet[] TerrainFootsteps => terrainFootsteps;
    }
}