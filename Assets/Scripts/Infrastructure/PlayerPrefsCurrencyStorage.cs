using UnityEngine;
using Aquarium.Domain;

namespace Aquarium.Infrastructure
{
    /// <summary>
    /// Local persistence provider backed by Unity's PlayerPrefs.
    /// Can be swapped for JSON/SQLite without modifying CurrencyWallet domain logic.
    /// </summary>
    public class PlayerPrefsCurrencyStorage : ICurrencyStorage
    {
        private const string BalanceKey = "AQUARIUM_PLAYER_GOLD";

        public int LoadBalance(int defaultBalance)
        {
            return PlayerPrefs.GetInt(BalanceKey, defaultBalance);
        }

        public void SaveBalance(int balance)
        {
            PlayerPrefs.SetInt(BalanceKey, balance);
            PlayerPrefs.Save();
        }
    }
}
