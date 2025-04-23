using UnityEngine;
namespace GameEnvironment
{
    public class VegetationSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject[] _bladePrefabs;
        [SerializeField] private GameObject[] _bushPrefabs;
        [SerializeField] private GameObject[] _TreePrefabs;

        [Header("Counts")]
        [SerializeField] private int _bladeCount = 5000;
        [SerializeField] private int _bushCount = 1000;
        [SerializeField] private int _treeCount = 500;

        [Header("Noise Settings")]
        [SerializeField] private float _noiseScaleBlades = 0.4f;
        [SerializeField] private float _placementThresholdBlades = 0.5f;
        [SerializeField] private float _noiseScaleBushes = 0.7f;
        [SerializeField] private float _placementThresholdBushes = 0.65f;
        [SerializeField] private float _noiseScaleTrees = 0.9f;
        [SerializeField] private float _placementThreshholdTrees = 0.75f;

        private Bounds _groundBounds;
        private Transform _vegetationContainer;

        private void Start()
        {
            InitBounds();
            GenerateAllVegetation();
        }

        private void InitBounds()
        {
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                Debug.LogError("VegetationSpawner requires a MeshCollider.");
                return;
            }

            Mesh mesh = meshCollider.sharedMesh;
            if (mesh == null)
            {
                Debug.LogError("MeshCollider has no mesh assigned.");
                return;
            }

            Vector3 min = transform.TransformPoint(mesh.bounds.min);
            Vector3 max = transform.TransformPoint(mesh.bounds.max);

            Vector3 size = max - min;
            Vector3 center = (min + max) / 2f;

            _groundBounds = new Bounds(center, size);

            Transform existing = transform.Find("Vegetation");
            _vegetationContainer = existing != null ? existing : new GameObject("Vegetation").transform;
            _vegetationContainer.parent = transform;
        }

        private void GenerateAllVegetation()
        {
            foreach (Transform child in _vegetationContainer)
            {
                Destroy(child.gameObject);
            }

            SpawnVegetation(_bladePrefabs, _bladeCount, _noiseScaleBlades, _placementThresholdBlades);
            SpawnVegetation(_bushPrefabs, _bushCount, _noiseScaleBushes, _placementThresholdBushes);
            SpawnVegetation(_TreePrefabs, _treeCount, _noiseScaleTrees, _placementThreshholdTrees);
        }

        private void SpawnVegetation(GameObject[] prefabs, int count, float noiseScale, float placementThreshold)
        {
            int placed = 0;
            int maxAttempts = count * 5;

            float xOffset = Random.Range(0f, 1000f);
            float zOffset = Random.Range(0f, 1000f);

            for (int i = 0; i < maxAttempts && placed < count; i++)
            {
                Vector3 pos = GetRandomPointOnGround();
                float noiseValue = Mathf.PerlinNoise((pos.x + xOffset) * noiseScale, (pos.z + zOffset) * noiseScale);

                if (noiseValue < placementThreshold)
                    continue;

                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), _vegetationContainer);
                placed++;
            }
        }

        private Vector3 GetRandomPointOnGround()
        {
            float x = Random.Range(_groundBounds.min.x, _groundBounds.max.x);
            float z = Random.Range(_groundBounds.min.z, _groundBounds.max.z);
            float y = transform.position.y;

            return new Vector3(x, y, z);
        }
    }
}
