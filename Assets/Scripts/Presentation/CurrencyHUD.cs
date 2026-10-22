using UnityEngine;
using UnityEngine.UI;
using Aquarium.Domain;
using Aquarium.Simulation;

namespace Aquarium.Presentation
{
    /// <summary>
    /// UI View displaying player's current gold balance and real-time simulator stats.
    /// Tracks active fish count and real-time FPS with zero GC allocations in Update.
    /// Manages event subscription cleanly in OnEnable/OnDisable to prevent delegate memory leaks.
    /// </summary>
    public class CurrencyHUD : MonoBehaviour
    {
        [Header("UI Text References")]
        [SerializeField] private Text _balanceText;
        [SerializeField] private Text _statsText;

        [Header("Format Settings")]
        [SerializeField] private string _balanceFormat = "Gold: {0}";
        [SerializeField] private string _statsFormat = "Active Fish: {0} | FPS: {1}";

        private ICurrencyWallet _wallet;
        private FishSpawner _spawner;

        private float _fpsAccumulator;
        private int _fpsFrameCount;
        private float _nextFpsUpdateTime;
        private int _currentFps = 60;

        public void Initialize(ICurrencyWallet wallet, FishSpawner spawner = null)
        {
            _wallet = wallet;
            _spawner = spawner;

            if (_wallet != null)
            {
                UpdateBalanceDisplay(_wallet.Balance);
            }
            SubscribeToEvents();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            // Lightweight FPS calculation
            _fpsAccumulator += Time.unscaledDeltaTime;
            _fpsFrameCount++;

            if (Time.unscaledTime >= _nextFpsUpdateTime)
            {
                _currentFps = Mathf.RoundToInt(_fpsFrameCount / _fpsAccumulator);
                _fpsAccumulator = 0f;
                _fpsFrameCount = 0;
                _nextFpsUpdateTime = Time.unscaledTime + 0.5f;

                UpdateStatsDisplay();
            }
        }

        private void SubscribeToEvents()
        {
            if (_wallet != null)
            {
                _wallet.OnBalanceChanged -= UpdateBalanceDisplay;
                _wallet.OnBalanceChanged += UpdateBalanceDisplay;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_wallet != null)
            {
                _wallet.OnBalanceChanged -= UpdateBalanceDisplay;
            }
        }

        private void UpdateBalanceDisplay(int balance)
        {
            if (_balanceText != null)
            {
                _balanceText.text = string.Format(_balanceFormat, balance);
            }
        }

        private void UpdateStatsDisplay()
        {
            if (_statsText != null)
            {
                int fishCount = _spawner != null ? _spawner.ActiveFishCount : 0;
                _statsText.text = string.Format(_statsFormat, fishCount, _currentFps);
            }
        }
    }
}
