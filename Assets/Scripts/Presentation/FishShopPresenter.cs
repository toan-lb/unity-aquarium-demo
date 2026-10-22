using System;
using Aquarium.Domain;

namespace Aquarium.Presentation
{
    /// <summary>
    /// Pure presenter coordinating fish purchase transactions.
    /// Receives ICurrencyWallet via Constructor Injection.
    /// Decoupled from UGUI Canvas, completely testable via NUnit.
    /// Supports single purchases and bulk batch purchases.
    /// </summary>
    public class FishShopPresenter
    {
        private readonly ICurrencyWallet _wallet;
        private readonly Action<string> _onFishSpawned;

        public event Action<string> OnPurchaseSucceeded;
        public event Action<string, string> OnPurchaseFailed;

        public FishShopPresenter(ICurrencyWallet wallet, Action<string> onFishSpawned)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _onFishSpawned = onFishSpawned;
        }

        public bool TryPurchaseFish(string fishId, int cost)
        {
            if (string.IsNullOrEmpty(fishId))
            {
                OnPurchaseFailed?.Invoke(fishId, "Fish ID cannot be null or empty.");
                return false;
            }

            if (cost <= 0)
            {
                OnPurchaseFailed?.Invoke(fishId, "Cost must be greater than zero.");
                return false;
            }

            if (!_wallet.TrySpend(cost))
            {
                OnPurchaseFailed?.Invoke(fishId, $"Insufficient gold ({_wallet.Balance}/{cost}).");
                return false;
            }

            // Successful deduction: Notify simulation & observers
            _onFishSpawned?.Invoke(fishId);
            OnPurchaseSucceeded?.Invoke(fishId);
            return true;
        }

        public bool TryPurchaseFishBatch(string fishId, int count, int totalCost)
        {
            if (string.IsNullOrEmpty(fishId) || count <= 0 || totalCost <= 0)
            {
                OnPurchaseFailed?.Invoke(fishId, "Invalid batch purchase parameters.");
                return false;
            }

            if (!_wallet.TrySpend(totalCost))
            {
                OnPurchaseFailed?.Invoke(fishId, $"Insufficient gold for school of {count} fish ({_wallet.Balance}/{totalCost}).");
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                _onFishSpawned?.Invoke(fishId);
            }

            OnPurchaseSucceeded?.Invoke($"{count}x {fishId}");
            return true;
        }
    }
}
