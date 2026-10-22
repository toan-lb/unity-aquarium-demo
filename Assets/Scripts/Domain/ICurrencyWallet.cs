using System;

namespace Aquarium.Domain
{
    /// <summary>
    /// Pure domain contract for wallet operations.
    /// Manages in-memory balance, deductions, additions, and change notifications.
    /// </summary>
    public interface ICurrencyWallet
    {
        int Balance { get; }
        bool TrySpend(int amount);
        void Add(int amount);
        event Action<int> OnBalanceChanged;
    }
}
