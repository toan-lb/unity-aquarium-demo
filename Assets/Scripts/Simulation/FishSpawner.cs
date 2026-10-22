using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Aquarium.Simulation
{
    /// <summary>
    /// Coordinates entity instantiation and object pooling for aquarium fish.
    /// Manages zero-alloc object recycling to sustain 60 FPS under load with 100+ fish.
    /// Features ambient starting population and passive income generation.
    /// </summary>
    public class FishSpawner : MonoBehaviour
    {
        [Header("Tank Boundaries")]
        [SerializeField] private Vector3 _tankBoundsMin = new Vector3(-8.5f, -4.2f, -1.5f);
        [SerializeField] private Vector3 _tankBoundsMax = new Vector3(8.5f, 4.2f, 1.5f);

        [Header("Ambient Population")]
        [SerializeField] private int _initialAmbientFish = 30;

        [Header("Prefabs / Fallback")]
        [SerializeField] private GameObject _defaultFishPrefab;

        private readonly List<FishEntity> _activeFish = new List<FishEntity>();
        private readonly Queue<FishEntity> _pool = new Queue<FishEntity>();

        private float _nextIncomeTickTime;
        public event Action<int> OnPassiveIncomeEarned;

        public int ActiveFishCount => _activeFish.Count;

        private void Start()
        {
            // Populate aquarium with an ambient school of fish so it feels lively immediately
            SpawnBatch("fish_ambient", _initialAmbientFish);
        }

        private void Update()
        {
            // Passive income generation: Active fish produce gold over time
            if (Time.time >= _nextIncomeTickTime)
            {
                _nextIncomeTickTime = Time.time + 3.0f; // Every 3 seconds
                if (_activeFish.Count > 0)
                {
                    // 1 gold for every 5 fish (minimum 1 gold if at least 1 fish exists)
                    int earnings = Mathf.Max(1, _activeFish.Count / 5);
                    OnPassiveIncomeEarned?.Invoke(earnings);
                }
            }
        }

        public void SpawnBatch(string speciesId, int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnFish(speciesId);
            }
        }

        public FishEntity SpawnFish(string speciesId)
        {
            FishEntity fish;
            if (_pool.Count > 0)
            {
                fish = _pool.Dequeue();
                fish.gameObject.SetActive(true);
            }
            else
            {
                GameObject go;
                if (_defaultFishPrefab != null)
                {
                    go = Instantiate(_defaultFishPrefab, transform);
                }
                else
                {
                    // Procedural primitive fallback (horizontal streamlined fish capsule)
                    go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    go.transform.SetParent(transform);

                    // Destroy collider to eliminate physics overhead for 100+ fish
                    Component col = go.GetComponent("Collider");
                    if (col != null) Destroy(col);

                    go.transform.localScale = GetScaleForSpecies(speciesId);
                    go.name = $"Fish_{speciesId}_{_activeFish.Count + 1}";
                }

                fish = go.GetComponent<FishEntity>();
                if (fish == null)
                {
                    fish = go.AddComponent<FishEntity>();
                }
            }

            // Spawn at random position within aquarium bounds
            Vector3 randomPos = new Vector3(
                Random.Range(_tankBoundsMin.x * 0.85f, _tankBoundsMax.x * 0.85f),
                Random.Range(_tankBoundsMin.y * 0.85f, _tankBoundsMax.y * 0.85f),
                Random.Range(_tankBoundsMin.z * 0.85f, _tankBoundsMax.z * 0.85f)
            );
            fish.transform.position = randomPos;

            float speed = GetSpeedForSpecies(speciesId);
            Color color = GetColorForSpecies(speciesId);
            fish.Initialize(speciesId, speed, _tankBoundsMin, _tankBoundsMax, color);

            _activeFish.Add(fish);
            return fish;
        }

        public void DespawnFish(FishEntity fish)
        {
            if (fish == null || !_activeFish.Contains(fish)) return;

            _activeFish.Remove(fish);
            fish.gameObject.SetActive(false);
            _pool.Enqueue(fish);
        }

        private Vector3 GetScaleForSpecies(string speciesId)
        {
            return speciesId switch
            {
                "fish_tetra" => new Vector3(0.18f, 0.18f, 0.5f),
                "fish_ambient" => new Vector3(0.14f, 0.14f, 0.38f),
                _ => new Vector3(0.26f, 0.26f, 0.7f) // Default Guppy
            };
        }

        private float GetSpeedForSpecies(string speciesId)
        {
            return speciesId switch
            {
                "fish_tetra" => 3.6f,
                "fish_ambient" => 2.8f,
                _ => 2.2f // Guppy
            };
        }

        private Color GetColorForSpecies(string speciesId)
        {
            return speciesId switch
            {
                "fish_tetra" => new Color(0.1f, 0.88f, 1.0f),      // Vibrant Neon Cyan
                "fish_ambient" => new Color(0.35f, 0.95f, 0.65f),  // Aquamarine
                _ => new Color(1.0f, 0.62f, 0.15f)                 // Golden Orange Guppy
            };
        }
    }
}
