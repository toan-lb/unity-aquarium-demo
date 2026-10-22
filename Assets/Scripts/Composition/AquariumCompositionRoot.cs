using UnityEngine;
using Aquarium.Domain;
using Aquarium.Infrastructure;
using Aquarium.Presentation;
using Aquarium.Simulation;

namespace Aquarium.Composition
{
    /// <summary>
    /// The single Composition Root for the Aquarium scene.
    /// Wires all dependencies in Awake() using Pure C# Dependency Injection.
    /// Connects the simulation passive income to the domain wallet.
    /// Zero third-party framework overhead.
    /// </summary>
    public class AquariumCompositionRoot : MonoBehaviour
    {
        [Header("Starting Economy")]
        [SerializeField] private int _defaultStartingGold = 200;
        [SerializeField] private bool _resetSaveDataOnStart = false;

        [Header("Scene References")]
        [SerializeField] private FishSpawner _spawner;
        [SerializeField] private CurrencyHUD _currencyHUD;
        [SerializeField] private FishShopView _shopView;

        private ICurrencyStorage _storage;
        private ICurrencyWallet _wallet;
        private FishShopPresenter _shopPresenter;

        private void Awake()
        {
            if (_resetSaveDataOnStart)
            {
                PlayerPrefs.DeleteKey("AQUARIUM_PLAYER_GOLD");
                PlayerPrefs.Save();
            }

            // Fallback: Auto-resolve spawner if unassigned
            if (_spawner == null)
            {
                _spawner = FindFirstObjectByType<FishSpawner>();
            }

            // 1. Initialize Persistence Infrastructure
            _storage = new PlayerPrefsCurrencyStorage();

            // 2. Initialize Pure C# Domain Wallet
            _wallet = new CurrencyWallet(_defaultStartingGold, _storage);

            // 3. Connect Simulation Passive Income to Domain Wallet
            if (_spawner != null)
            {
                _spawner.OnPassiveIncomeEarned += OnPassiveGoldEarned;
            }

            // 4. Initialize Presentation Layer (Constructor Injection)
            _shopPresenter = new FishShopPresenter(_wallet, OnFishSpawnRequested);

            // 5. Inject Dependencies into UI Views
            if (_currencyHUD != null)
            {
                _currencyHUD.Initialize(_wallet, _spawner);
            }

            if (_shopView != null)
            {
                _shopView.Initialize(_shopPresenter, _wallet);
            }

            Debug.Log($"[AquariumCompositionRoot] Systems wired successfully. Gold: {_wallet.Balance}");
        }

        private void OnDestroy()
        {
            if (_spawner != null)
            {
                _spawner.OnPassiveIncomeEarned -= OnPassiveGoldEarned;
            }
        }

        private void OnPassiveGoldEarned(int amount)
        {
            if (_wallet != null)
            {
                _wallet.Add(amount);
            }
        }

        private void OnFishSpawnRequested(string fishId)
        {
            if (_spawner != null)
            {
                _spawner.SpawnFish(fishId);
            }
            else
            {
                Debug.LogWarning($"[AquariumCompositionRoot] Fish {fishId} purchased but FishSpawner is missing!");
            }
        }
    }
}
