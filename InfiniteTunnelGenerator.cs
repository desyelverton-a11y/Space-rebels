using System.Collections.Generic;
using UnityEngine;


namespace TGD.InfiniteTunnelGenerator
{
    /// <summary>
    /// Core script for generating procedural infinite tunnels.
    /// Handles mesh pooling, trajectory math, and object spawning.
    /// </summary>
    public class InfiniteTunnelGenerator : MonoBehaviour
    {
        public enum TunnelMode
        {
            Straight,
            SineWave,
            Spiral,
            [InspectorName("Bezier (Experimental)")] Bezier
        }

        [System.Serializable]
        public struct SpawnableLayer
        {
            public string name;
            public GameObject[] prefabs;
            public int countPerChunk;
            public float minRadius;
            public float maxRadius;
            public bool rotateRandomly;
            public Vector2 scaleRandomRange;
        }

        [HideInInspector] public TunnelMode generationMode = TunnelMode.Straight;

        [Header("Tunnel Geometry")]
        [Tooltip("Radius of the tunnel tube.")]
        public float radius = 100f;
        [Tooltip("Number of segments forming the circle. Higher is smoother but heavier.")]
        public int segmentsPerCircle = 64;
        [Tooltip("Enable or disable physical mesh colliders for the tunnel walls.")]
        public bool useCollider = true;
        public Material tunnelMaterial;

        [Header("Infinite Management")]
        [Tooltip("The transform to track for progress (Player or Camera).")]
        public GameObject spaceshipObj;
        public float chunkLength = 100f;
        public int waypointsPerChunk = 20;
        public int poolSize = 8;

        [Header("Sine Wave Settings")]
        public float sineAmplitude = 120f;
        public float sineFrequency = 0.01f;

        [Header("Spiral Settings")]
        public float spiralRadius = 110f;
        public float spiralFrequency = 0.01f;

        [Header("Bezier Settings (Dev In Progress)")]
        [Tooltip("Horizontal/Vertical randomness for the curve end point.")]
        public Vector2 bezierCurvatureRange = new Vector2(-40f, 40f);
        public float bezierSmoothness = 50f;

        [Header("Rendering")]
        [Tooltip("When enabled, chunk render queues are incremented front-to-back so that near tunnel " +
                 "sections occlude far ones. This creates a 'discovery' effect where the tunnel depth " +
                 "is hidden until the player flies through it.\n\n" +
                 "Disable this if you want the full tunnel depth to be visible at all times " +
                 "(e.g. for a glass tunnel, a top-down view, or a cinematic camera).")]
        public bool useDepthSorting = true;

        [Header("Spawnable Content")]
        public List<SpawnableLayer> spawnableLayers;

        [Header("Spawnable Visibility")]
        [Tooltip("Number of chunks AHEAD of the player's current chunk where spawned prefabs are visible.\n\n" +
                 "0 = current chunk only\n" +
                 "1 = current + 1 ahead\n" +
                 "3 = current + 3 ahead (recommended)\n\n" +
                 "Capped at poolSize - 1 at runtime. " +
                 "Prefab renderers beyond this limit are disabled to reduce draw calls. " +
                 "The tunnel mesh itself is always visible.")]
        [Min(0)]
        public int visibleChunksAhead = 3;

        // Internal State
        private List<GameObject> m_chunks = new List<GameObject>();
        private List<Mesh> m_meshes = new List<Mesh>();

        private Vector3 m_nextEntryPoint = Vector3.zero;
        private float m_totalDistanceTravelled = 0f;
        private int m_currentLeadingChunkIndex = 0;

        // Tracks which pool slot the player is currently in (avoids redundant visibility updates)
        private int m_playerChunkIndex = -1;

        // Bezier specific continuity
        private Vector3 m_bezierEndPos;
        private Vector3 m_bezierEndTangent;

        private void Start()
        {
            if (spaceshipObj == null)
            {
                Debug.LogWarning("[TunnelGenerator] Spaceship/Player Object is not assigned!");
                return;
            }
            InitializePool();
        }

        private void Update()
        {
            if (spaceshipObj != null)
                CheckVesselProgress();
        }

        /// <summary>
        /// Creates the initial pool of chunks and meshes.
        /// </summary>
        private void InitializePool()
        {
            m_chunks.Clear();
            m_meshes.Clear();
            m_nextEntryPoint = transform.position;
            m_totalDistanceTravelled = 0f;
            m_playerChunkIndex = -1;

            m_bezierEndPos = transform.position;
            m_bezierEndTangent = transform.forward * bezierSmoothness;

            for (int i = 0; i < poolSize; i++)
            {
                GameObject chunk = new GameObject($"Chunk_{i}");
                chunk.transform.SetParent(this.transform);
                chunk.transform.localPosition = Vector3.zero;
                chunk.transform.localRotation = Quaternion.identity;

                chunk.AddComponent<MeshFilter>();
                MeshRenderer mr = chunk.AddComponent<MeshRenderer>();
                mr.material = tunnelMaterial;

                // Conditional Collider addition
                if (useCollider)
                {
                    chunk.AddComponent<MeshCollider>();
                }

                Mesh mesh = new Mesh { name = $"Mesh_Chunk_{i}" };
                chunk.GetComponent<MeshFilter>().mesh = mesh;

                m_chunks.Add(chunk);
                m_meshes.Add(mesh);

                GenerateChunk(i);
            }
            m_currentLeadingChunkIndex = poolSize - 1;
            UpdateRenderOrder();
        }

        /// <summary>
        /// Calculates the ideal center position of the tunnel at a specific Z distance.
        /// </summary>
        public Vector3 GetTunnelPositionAtZ(float z)
        {
            Vector3 offset = Vector3.zero;
            switch (generationMode)
            {
                case TunnelMode.SineWave:
                    offset.x = Mathf.Sin(z * sineFrequency) * sineAmplitude;
                    break;
                case TunnelMode.Spiral:
                    offset.x = Mathf.Cos(z * spiralFrequency) * spiralRadius;
                    offset.y = Mathf.Sin(z * spiralFrequency) * spiralRadius;
                    break;
            }
            return transform.position + (transform.rotation * (offset + Vector3.forward * z));
        }

        private void GenerateChunk(int index)
        {
            Mesh m = m_meshes[index];
            m.Clear();

            Vector3[] wpPositions = new Vector3[waypointsPerChunk + 1];
            Quaternion[] wpRotations = new Quaternion[waypointsPerChunk + 1];

            if (generationMode == TunnelMode.Bezier)
                CalculateBezierPath(wpPositions, wpRotations);
            else
                CalculateMathPath(wpPositions, wpRotations);

            BuildMesh(m, wpPositions, wpRotations, index);

            m_totalDistanceTravelled += chunkLength;
            m_nextEntryPoint += transform.forward * chunkLength;

            SpawnObjectsForChunk(index, wpPositions, wpRotations);
        }

        private void CalculateMathPath(Vector3[] pos, Quaternion[] rot)
        {
            float step = chunkLength / waypointsPerChunk;

            for (int i = 0; i <= waypointsPerChunk; i++)
            {
                float localDist = i * step;
                float absoluteDist = m_totalDistanceTravelled + localDist;
                Vector3 basePos = transform.position + (transform.forward * absoluteDist);

                if (generationMode == TunnelMode.SineWave)
                {
                    float offset = Mathf.Sin(absoluteDist * sineFrequency) * sineAmplitude;
                    pos[i] = basePos + (transform.right * offset);
                    float deriv = Mathf.Cos(absoluteDist * sineFrequency) * sineAmplitude * sineFrequency;
                    rot[i] = Quaternion.LookRotation((transform.forward + transform.right * deriv).normalized);
                }
                else if (generationMode == TunnelMode.Spiral)
                {
                    float angle = absoluteDist * spiralFrequency;
                    Vector3 spiralOffset = (transform.right * Mathf.Cos(angle) + transform.up * Mathf.Sin(angle)) * spiralRadius;
                    pos[i] = basePos + spiralOffset;
                    Vector3 spin = (-transform.right * Mathf.Sin(angle) + transform.up * Mathf.Cos(angle)) * spiralRadius * spiralFrequency;
                    rot[i] = Quaternion.LookRotation((transform.forward + spin).normalized);
                }
                else
                {
                    pos[i] = basePos;
                    rot[i] = transform.rotation;
                }
            }
        }

        private void CalculateBezierPath(Vector3[] pos, Quaternion[] rot)
        {
            Vector3 p0 = m_bezierEndPos;
            Vector3 p1 = p0 + m_bezierEndTangent;
            Vector3 p3 = p0 + (transform.forward * chunkLength) +
                         (transform.right * Random.Range(bezierCurvatureRange.x, bezierCurvatureRange.y)) +
                         (transform.up * Random.Range(bezierCurvatureRange.x, bezierCurvatureRange.y));
            Vector3 p2 = p3 - (p3 - p0).normalized * bezierSmoothness;

            for (int i = 0; i <= waypointsPerChunk; i++)
            {
                float t = (float)i / waypointsPerChunk;
                pos[i] = GetBezierPoint(t, p0, p1, p2, p3);
                rot[i] = Quaternion.LookRotation(GetBezierDerivative(t, p0, p1, p2, p3).normalized);
            }

            m_bezierEndPos = pos[waypointsPerChunk];
            m_bezierEndTangent = (pos[waypointsPerChunk] - pos[waypointsPerChunk - 1]).normalized * bezierSmoothness;
        }

        private void BuildMesh(Mesh m, Vector3[] wpPositions, Quaternion[] wpRotations, int index)
        {
            int verticesPerRing = segmentsPerCircle + 1;
            Vector3[] vertices = new Vector3[(waypointsPerChunk + 1) * verticesPerRing];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[waypointsPerChunk * segmentsPerCircle * 6];

            int vIdx = 0;
            for (int i = 0; i <= waypointsPerChunk; i++)
            {
                for (int j = 0; j <= segmentsPerCircle; j++)
                {
                    float u = (float)j / segmentsPerCircle;
                    float angle = u * 360f;
                    Vector3 ringPoint = wpRotations[i] * (Quaternion.Euler(0, 0, angle) * Vector3.right * radius);
                    vertices[vIdx] = m_chunks[index].transform.InverseTransformPoint(wpPositions[i] + ringPoint);
                    uvs[vIdx] = new Vector2(u, (m_totalDistanceTravelled + i * (chunkLength / waypointsPerChunk)) / radius);
                    vIdx++;
                }
            }

            int tIdx = 0;
            for (int i = 0; i < waypointsPerChunk; i++)
            {
                for (int j = 0; j < segmentsPerCircle; j++)
                {
                    int curr = i * verticesPerRing + j;
                    int next = curr + verticesPerRing;
                    triangles[tIdx++] = curr; triangles[tIdx++] = next; triangles[tIdx++] = curr + 1;
                    triangles[tIdx++] = next; triangles[tIdx++] = next + 1; triangles[tIdx++] = curr + 1;
                }
            }

            m.vertices = vertices;
            m.triangles = triangles;
            m.uv = uvs;
            m.RecalculateNormals();

            // Update collider if used
            if (useCollider)
            {
                MeshCollider mc = m_chunks[index].GetComponent<MeshCollider>();
                if (mc != null) mc.sharedMesh = m;
            }
        }

        private void CheckVesselProgress()
        {
            float vesselZ = transform.InverseTransformPoint(spaceshipObj.transform.position).z;

            // ── Spawnable visibility ──────────────────────────────────────────
            // Determine which pool slot the player currently occupies
            int currentChunkIndex = Mathf.Max(0, Mathf.FloorToInt(vesselZ / chunkLength) % poolSize);

            // Only refresh visibility when the player crosses a chunk boundary
            if (currentChunkIndex != m_playerChunkIndex)
            {
                m_playerChunkIndex = currentChunkIndex;
                UpdateSpawnableVisibility(currentChunkIndex);
            }

            // ── Chunk recycling ───────────────────────────────────────────────
            float generationThreshold = m_totalDistanceTravelled - (chunkLength * (poolSize - 2));

            if (vesselZ > generationThreshold)
            {
                int oldestIndex = (m_currentLeadingChunkIndex + 1) % poolSize;
                GenerateChunk(oldestIndex);
                m_currentLeadingChunkIndex = oldestIndex;
                UpdateRenderOrder();

                // Newly spawned prefabs have renderers enabled by default.
                // Apply visibility rules immediately so far-ahead chunks are
                // correctly hidden without waiting for the next boundary crossing.
                if (m_playerChunkIndex >= 0)
                    UpdateSpawnableVisibility(m_playerChunkIndex);
            }
        }

        /// <summary>
        /// Enables spawned-prefab renderers within <see cref="visibleChunksAhead"/> slots
        /// ahead of the player's current chunk. Chunks beyond that limit have their spawned
        /// prefab renderers disabled to save draw calls. The tunnel mesh itself is unaffected.
        /// </summary>
        private void UpdateSpawnableVisibility(int activeChunkIndex)
        {
            int clampedAhead = Mathf.Clamp(visibleChunksAhead, 0, poolSize - 1);

            for (int i = 0; i < m_chunks.Count; i++)
            {
                // Circular offset between this slot and the player's current slot.
                // offset=0 → current chunk, offset=1 → 1 ahead, etc.
                int offset = (i - activeChunkIndex + poolSize) % poolSize;

                // visibleChunksAhead=3 → visible if offset is 0,1,2,3 (current + 3 ahead)
                bool visible = offset <= clampedAhead;
                SetSpawnableRenderersInChunk(m_chunks[i], visible);
            }
        }

        /// <summary>
        /// Toggles all Renderer components on every direct child of the chunk (spawned prefabs).
        /// The chunk's own MeshRenderer (tunnel geometry) is intentionally left untouched.
        /// </summary>
        private void SetSpawnableRenderersInChunk(GameObject chunk, bool visible)
        {
            if (chunk == null) return;

            foreach (Transform child in chunk.transform)
            {
                foreach (Renderer r in child.GetComponentsInChildren<Renderer>(includeInactive: true))
                    r.enabled = visible;
            }
        }

        private void UpdateRenderOrder()
        {
            int firstChunkIndex = (m_currentLeadingChunkIndex + 1) % poolSize;
            for (int i = 0; i < poolSize; i++)
            {
                int actualIndex = (firstChunkIndex + i) % poolSize;
                Renderer r = m_chunks[actualIndex].GetComponent<Renderer>();
                if (r == null) continue;

                // Accessing .material (not .sharedMaterial) creates a per-instance material,
                // which is required anyway since each chunk needs its own renderQueue value.
                // Setting renderQueue to -1 resets to the shader's default, removing any override.
                r.material.renderQueue = useDepthSorting ? 3000 + i : -1;
            }
        }

        private void SpawnObjectsForChunk(int index, Vector3[] waypoints, Quaternion[] rotations)
        {
            Transform t = m_chunks[index].transform;
            foreach (Transform child in t) Destroy(child.gameObject);

            foreach (var layer in spawnableLayers)
            {
                if (layer.prefabs == null || layer.prefabs.Length == 0) continue;
                for (int i = 0; i < layer.countPerChunk; i++)
                {
                    int wpIdx = Random.Range(1, waypoints.Length - 1);
                    float angle = Random.Range(0f, 360f);
                    Vector3 offset = rotations[wpIdx] * (Quaternion.Euler(0, 0, angle) * Vector3.right * Random.Range(layer.minRadius, layer.maxRadius));
                    GameObject inst = Instantiate(layer.prefabs[Random.Range(0, layer.prefabs.Length)], waypoints[wpIdx] + offset, layer.rotateRandomly ? Random.rotation : Quaternion.identity);
                    inst.transform.SetParent(t);
                    inst.transform.localScale = Vector3.one * Random.Range(layer.scaleRandomRange.x, layer.scaleRandomRange.y);
                }
            }
        }

        private Vector3 GetBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            return (u * u * u * p0) + (3 * u * u * t * p1) + (3 * u * t * t * p2) + (t * t * t * p3);
        }

        private Vector3 GetBezierDerivative(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            return 3 * u * u * (p1 - p0) + 6 * u * t * (p2 - p1) + 3 * t * t * (p3 - p2);
        }
    }
}
