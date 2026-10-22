using System;
using NUnit.Framework;
using Aquarium.Domain;
using Aquarium.Presentation;

namespace Aquarium.Tests
{
    [TestFixture]
    public class FishShopPresenterTests
    {
        private class StubWallet : ICurrencyWallet
        {
            public int Balance { get; set; } = 100;
            public bool SpendReturnValue { get; set; } = true;
            public int LastSpendAmountRequested { get; private set; }
            public event Action<int> OnBalanceChanged;

            public bool TrySpend(int amount)
            {
                LastSpendAmountRequested = amount;
                if (SpendReturnValue)
                {
                    Balance -= amount;
                    OnBalanceChanged?.Invoke(Balance);
                    return true;
                }
                return false;
            }

            public void Add(int amount) => Balance += amount;
        }

        [Test]
        public void Constructor_NullWallet_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FishShopPresenter(null, null));
        }

        [Test]
        public void TryPurchaseFish_WhenWalletAllows_InvokesSpawnCallbackAndReturnsTrue()
        {
            var wallet = new StubWallet { SpendReturnValue = true };
            string spawnedFishId = null;
            var presenter = new FishShopPresenter(wallet, id => spawnedFishId = id);

            bool success = presenter.TryPurchaseFish("fish_guppy", 30);

            Assert.IsTrue(success);
            Assert.AreEqual(30, wallet.LastSpendAmountRequested);
            Assert.AreEqual("fish_guppy", spawnedFishId);
        }

        [Test]
        public void TryPurchaseFish_WhenWalletRejects_DoesNotSpawnAndReturnsFalse()
        {
            var wallet = new StubWallet { SpendReturnValue = false };
            string spawnedFishId = null;
            var presenter = new FishShopPresenter(wallet, id => spawnedFishId = id);

            bool success = presenter.TryPurchaseFish("fish_guppy", 150);

            Assert.IsFalse(success);
            Assert.IsNull(spawnedFishId);
        }

        [Test]
        public void TryPurchaseFish_InvalidInput_ReturnsFalseWithoutCallingWallet()
        {
            var wallet = new StubWallet { SpendReturnValue = true };
            var presenter = new FishShopPresenter(wallet, null);

            Assert.IsFalse(presenter.TryPurchaseFish(null, 30));
            Assert.IsFalse(presenter.TryPurchaseFish("", 30));
            Assert.IsFalse(presenter.TryPurchaseFish("fish_guppy", 0));
            Assert.IsFalse(presenter.TryPurchaseFish("fish_guppy", -10));
            Assert.AreEqual(0, wallet.LastSpendAmountRequested);
        }

        [Test]
        public void TryPurchaseFishBatch_SufficientFunds_SpawnsCountTimesAndDeductsCost()
        {
            var wallet = new StubWallet { Balance = 500, SpendReturnValue = true };
            int spawnCount = 0;
            var presenter = new FishShopPresenter(wallet, _ => spawnCount++);

            bool success = presenter.TryPurchaseFishBatch("fish_ambient", 10, 150);

            Assert.IsTrue(success);
            Assert.AreEqual(10, spawnCount);
            Assert.AreEqual(150, wallet.LastSpendAmountRequested);
        }

        [Test]
        public void TryPurchaseFishBatch_InsufficientFunds_DoesNotSpawnAndReturnsFalse()
        {
            var wallet = new StubWallet { Balance = 50, SpendReturnValue = false };
            int spawnCount = 0;
            var presenter = new FishShopPresenter(wallet, _ => spawnCount++);

            bool success = presenter.TryPurchaseFishBatch("fish_ambient", 10, 150);

            Assert.IsFalse(success);
            Assert.AreEqual(0, spawnCount);
        }
    }
}
