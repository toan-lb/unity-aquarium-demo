using UnityEngine;

namespace Aquarium.Simulation
{
    /// <summary>
    /// Represents an individual fish swimming in the aquarium tank.
    /// Uses lightweight procedural movement with zero GC allocations in Update.
    /// Simulates a natural swimming oscillation (tail wiggle) and smooth boundary bounce.
    /// </summary>
    public class FishEntity : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _speed = 2.5f;
        [SerializeField] private float _turnSpeed = 2.5f;
        [SerializeField] private float _wiggleFrequency = 6.0f;
        [SerializeField] private float _wiggleAmplitude = 8.0f;

        [Header("Boundary Clamping")]
        [SerializeField] private Vector3 _boundsMin = new Vector3(-8f, -4f, -1f);
        [SerializeField] private Vector3 _boundsMax = new Vector3(8f, 4f, 1f);

        private Vector3 _targetDirection;
        private float _nextDirectionChangeTime;
        private float _swimCycleOffset;
        private MeshRenderer _meshRenderer;
        private static MaterialPropertyBlock s_propBlock;

        public string SpeciesId { get; private set; }

        private void Awake()
        {
            // Remove any collider to eliminate physics overhead for large flocks
            Component col = GetComponent("Collider");
            if (col != null)
            {
                Destroy(col);
            }

            _meshRenderer = GetComponent<MeshRenderer>();
            _swimCycleOffset = Random.Range(0f, 100f);
        }

        public void Initialize(string speciesId, float speed, Vector3 tankMin, Vector3 tankMax, Color speciesColor)
        {
            SpeciesId = speciesId;
            _speed = speed;
            _boundsMin = tankMin;
            _boundsMax = tankMax;

            // Apply color without instantiating new material (Zero GC)
            if (_meshRenderer != null)
            {
                s_propBlock ??= new MaterialPropertyBlock();
                _meshRenderer.GetPropertyBlock(s_propBlock);
                s_propBlock.SetColor("_Color", speciesColor);
                s_propBlock.SetColor("_BaseColor", speciesColor); // For URP Lit
                _meshRenderer.SetPropertyBlock(s_propBlock);
            }

            PickRandomDirection();
        }

        private void Update()
        {
            MoveAndWander(Time.deltaTime);
        }

        public void MoveAndWander(float deltaTime)
        {
            Vector3 pos = transform.position;

            // Steer away from aquarium glass walls
            bool nearBoundary = false;
            if (pos.x < _boundsMin.x && _targetDirection.x < 0) { _targetDirection.x = Mathf.Abs(_targetDirection.x); nearBoundary = true; }
            else if (pos.x > _boundsMax.x && _targetDirection.x > 0) { _targetDirection.x = -Mathf.Abs(_targetDirection.x); nearBoundary = true; }

            if (pos.y < _boundsMin.y && _targetDirection.y < 0) { _targetDirection.y = Mathf.Abs(_targetDirection.y); nearBoundary = true; }
            else if (pos.y > _boundsMax.y && _targetDirection.y > 0) { _targetDirection.y = -Mathf.Abs(_targetDirection.y); nearBoundary = true; }

            if (pos.z < _boundsMin.z && _targetDirection.z < 0) { _targetDirection.z = Mathf.Abs(_targetDirection.z); nearBoundary = true; }
            else if (pos.z > _boundsMax.z && _targetDirection.z > 0) { _targetDirection.z = -Mathf.Abs(_targetDirection.z); nearBoundary = true; }

            if (!nearBoundary && Time.time >= _nextDirectionChangeTime)
            {
                PickRandomDirection();
            }

            // Smooth rotation towards target direction
            if (_targetDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(_targetDirection, Vector3.up);

                // Add subtle lateral swimming oscillation (tail wag)
                float wiggle = Mathf.Sin((Time.time + _swimCycleOffset) * _wiggleFrequency) * _wiggleAmplitude;
                Quaternion wiggleRot = targetRot * Quaternion.Euler(0, wiggle, 0);

                transform.rotation = Quaternion.Slerp(transform.rotation, wiggleRot, _turnSpeed * deltaTime);
            }

            // Move forward
            transform.position += transform.forward * (_speed * deltaTime);
        }

        private void PickRandomDirection()
        {
            _targetDirection = Random.onUnitSphere;
            // Bias slightly towards horizontal movement (more natural fish swim)
            _targetDirection.y *= 0.4f;
            _targetDirection.Normalize();

            _nextDirectionChangeTime = Time.time + Random.Range(2.5f, 6.0f);
        }
    }
}
