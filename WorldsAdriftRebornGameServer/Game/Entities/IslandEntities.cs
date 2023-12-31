using System.Numerics;

namespace WorldsAdriftRebornGameServer.Game.Entities
{
    internal class IslandEntities
    {
        public long EntityId { get; set; }
        public string? Name { get; set; }

        public Improbable.Collections.List<long> Position { get; set; }
    }
}
