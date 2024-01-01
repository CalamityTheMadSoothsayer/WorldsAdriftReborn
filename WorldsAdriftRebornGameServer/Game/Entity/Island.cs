namespace WorldsAdriftRebornGameServer.Game.Entity
{
    public class Island : CoreEntity
    {
        public static readonly List<Island> IslandList = new List<Island>();

        public override void Awake()
        {
            base.Awake();
            IslandList.Add(this);
            Console.WriteLine($"Island {Id} added to IslandList");
        }
        public string? Key { get; set; }
        public Improbable.Collections.List<long>? Position { get; set; }
    }
}
