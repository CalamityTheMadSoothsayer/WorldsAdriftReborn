namespace WorldsAdriftRebornGameServer.Game.Components.Data
{
    public class Island : CoreEntity
    {
        public string Key { get; set; }
        public Improbable.Collections.List<long> Position { get; set; }
    }
}
