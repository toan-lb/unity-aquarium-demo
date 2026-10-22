namespace Aquarium.Domain
{
    /// <summary>
    /// Abstract contract for persisting and loading local currency data.
    /// Decouples storage mechanisms (PlayerPrefs, JSON, Binary) from domain logic.
    /// </summary>
    public interface ICurrencyStorage
    {
        int LoadBalance(int defaultBalance);
        void SaveBalance(int balance);
    }
}
