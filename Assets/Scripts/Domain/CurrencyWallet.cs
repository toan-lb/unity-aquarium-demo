using System;

namespace Aquarium.Domain
{
    /// <summary>
    /// Pure C# domain entity representing the player's wallet.
    /// Completely independent of UnityEngine and MonoBehaviour lifecycles.
    /// 100% testable in NUnit execution environments.
    /// </summary>
    public class CurrencyWallet : ICurrencyWallet
    {
        private readonly ICurrencyStorage _storage;

        public int Balance { get; private set; }
        public event Action<int> OnBalanceChanged;

        public CurrencyWallet(int defaultBalance, ICurrencyStorage storage = null)
        {
            _storage = storage;
            int initialAmount = _storage != null ? _storage.LoadBalance(defaultBalance) : defaultBalance;
            Balance = Math.Max(0, initialAmount);
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;

            Balance += amount;
            _storage?.SaveBalance(Balance);
            OnBalanceChanged?.Invoke(Balance);
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || Balance < amount)
            {
                return false;
            }

            Balance -= amount;
            _storage?.SaveBalance(Balance);
            OnBalanceChanged?.Invoke(Balance);
            return true;
        }
    }
}
