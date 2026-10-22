namespace Aquarium.Domain
{
    /// <summary>
    /// Pure C# value object representing fish species configuration and costs.
    /// </summary>
    public readonly struct FishSpeciesConfig
    {
        public string Id { get; }
        public string DisplayName { get; }
        public int Cost { get; }
        public float MoveSpeed { get; }

        public FishSpeciesConfig(string id, string displayName, int cost, float moveSpeed)
        {
            Id = id;
            DisplayName = displayName;
            Cost = cost;
            MoveSpeed = moveSpeed;
        }
    }
}
