using UnityEngine;
using UnityEngine.UI;
using Aquarium.Domain;

namespace Aquarium.Presentation
{
    /// <summary>
    /// UI View managing buy buttons for individual fish and bulk schools.
    /// Includes a debug "+100 Gold" button for effortless sandbox stress testing.
    /// Interacts cleanly with FishShopPresenter without mutating domain rules directly.
    /// </summary>
    public class FishShopView : MonoBehaviour
    {
        [Header("Single Fish Purchases")]
        [SerializeField] private Button _buyGuppyButton;
        [SerializeField] private int _guppyCost = 30;

        [SerializeField] private Button _buyTetraButton;
        [SerializeField] private int _tetraCost = 60;

        [Header("Bulk Purchase (School of Fish)")]
        [SerializeField] private Button _buySchoolButton;
        [SerializeField] private int _schoolCount = 10;
        [SerializeField] private int _schoolCost = 150;

        [Header("Sandbox / Debug Controls")]
        [SerializeField] private Button _addGoldButton;
        [SerializeField] private int _addGoldAmount = 100;

        [Header("Feedback Text")]
        [SerializeField] private Text _statusFeedbackText;

        private FishShopPresenter _presenter;
        private ICurrencyWallet _wallet;

        public void Initialize(FishShopPresenter presenter, ICurrencyWallet wallet)
        {
            _presenter = presenter;
            _wallet = wallet;

            if (_buyGuppyButton != null)
            {
                _buyGuppyButton.onClick.RemoveAllListeners();
                _buyGuppyButton.onClick.AddListener(() => TryBuy("fish_guppy", _guppyCost));
            }

            if (_buyTetraButton != null)
            {
                _buyTetraButton.onClick.RemoveAllListeners();
                _buyTetraButton.onClick.AddListener(() => TryBuy("fish_tetra", _tetraCost));
            }

            if (_buySchoolButton != null)
            {
                _buySchoolButton.onClick.RemoveAllListeners();
                _buySchoolButton.onClick.AddListener(() => TryBuySchool("fish_ambient", _schoolCount, _schoolCost));
            }

            if (_addGoldButton != null)
            {
                _addGoldButton.onClick.RemoveAllListeners();
                _addGoldButton.onClick.AddListener(OnAddGoldClicked);
            }

            _presenter.OnPurchaseSucceeded += OnPurchaseSuccess;
            _presenter.OnPurchaseFailed += OnPurchaseFailure;
        }

        private void OnDestroy()
        {
            if (_presenter != null)
            {
                _presenter.OnPurchaseSucceeded -= OnPurchaseSuccess;
                _presenter.OnPurchaseFailed -= OnPurchaseFailure;
            }
        }

        private void TryBuy(string fishId, int cost)
        {
            if (_presenter == null) return;
            _presenter.TryPurchaseFish(fishId, cost);
        }

        private void TryBuySchool(string fishId, int count, int cost)
        {
            if (_presenter == null) return;
            _presenter.TryPurchaseFishBatch(fishId, count, cost);
        }

        private void OnAddGoldClicked()
        {
            if (_wallet != null)
            {
                _wallet.Add(_addGoldAmount);
                if (_statusFeedbackText != null)
                {
                    _statusFeedbackText.text = $"Added +{_addGoldAmount} Gold (Current: {_wallet.Balance})";
                }
            }
        }

        private void OnPurchaseSuccess(string item)
        {
            if (_statusFeedbackText != null)
            {
                _statusFeedbackText.text = $"Purchased {item} successfully!";
            }
        }

        private void OnPurchaseFailure(string item, string reason)
        {
            if (_statusFeedbackText != null)
            {
                _statusFeedbackText.text = reason;
            }
        }
    }
}
