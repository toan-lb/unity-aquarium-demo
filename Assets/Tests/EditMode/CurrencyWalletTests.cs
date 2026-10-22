using NUnit.Framework;
using Aquarium.Domain;

namespace Aquarium.Tests
{
    [TestFixture]
    public class CurrencyWalletTests
    {
        private class InMemoryStorageStub : ICurrencyStorage
        {
            public int? StoredBalance;
            public int SaveCallCount;

            public int LoadBalance(int defaultBalance) => StoredBalance ?? defaultBalance;
            public void SaveBalance(int balance)
            {
                StoredBalance = balance;
                SaveCallCount++;
            }
        }

        [Test]
        public void Constructor_NegativeInitialBalance_ClampsToZero()
        {
            var wallet = new CurrencyWallet(-50);
            Assert.AreEqual(0, wallet.Balance);
        }

        [Test]
        public void Constructor_WithStorage_LoadsExistingBalance()
        {
            var storage = new InMemoryStorageStub { StoredBalance = 250 };
            var wallet = new CurrencyWallet(100, storage);

            Assert.AreEqual(250, wallet.Balance);
        }

        [Test]
        public void Add_PositiveAmount_IncreasesBalanceAndFiresEvent()
        {
            var storage = new InMemoryStorageStub();
            var wallet = new CurrencyWallet(100, storage);
            int eventFiredValue = -1;
            wallet.OnBalanceChanged += val => eventFiredValue = val;

            wallet.Add(50);

            Assert.AreEqual(150, wallet.Balance);
            Assert.AreEqual(150, eventFiredValue);
            Assert.AreEqual(150, storage.StoredBalance);
            Assert.AreEqual(1, storage.SaveCallCount);
        }

        [Test]
        public void Add_NegativeOrZeroAmount_DoesNotChangeBalanceOrFireEvent()
        {
            var wallet = new CurrencyWallet(100);
            bool eventFired = false;
            wallet.OnBalanceChanged += _ => eventFired = true;

            wallet.Add(0);
            wallet.Add(-20);

            Assert.AreEqual(100, wallet.Balance);
            Assert.IsFalse(eventFired);
        }

        [Test]
        public void TrySpend_SufficientBalance_DeductsAmountAndReturnsTrue()
        {
            var storage = new InMemoryStorageStub();
            var wallet = new CurrencyWallet(100, storage);
            int eventFiredValue = -1;
            wallet.OnBalanceChanged += val => eventFiredValue = val;

            bool result = wallet.TrySpend(40);

            Assert.IsTrue(result);
            Assert.AreEqual(60, wallet.Balance);
            Assert.AreEqual(60, eventFiredValue);
            Assert.AreEqual(60, storage.StoredBalance);
        }

        [Test]
        public void TrySpend_InsufficientBalance_FailsAndKeepsBalanceIntact()
        {
            var wallet = new CurrencyWallet(30);
            bool eventFired = false;
            wallet.OnBalanceChanged += _ => eventFired = true;

            bool result = wallet.TrySpend(50);

            Assert.IsFalse(result);
            Assert.AreEqual(30, wallet.Balance);
            Assert.IsFalse(eventFired);
        }

        [Test]
        public void TrySpend_ZeroOrNegativeAmount_ReturnsFalse()
        {
            var wallet = new CurrencyWallet(100);
            Assert.IsFalse(wallet.TrySpend(0));
            Assert.IsFalse(wallet.TrySpend(-10));
            Assert.AreEqual(100, wallet.Balance);
        }
    }
}
