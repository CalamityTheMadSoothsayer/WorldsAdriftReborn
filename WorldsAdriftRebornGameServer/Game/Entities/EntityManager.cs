namespace WorldsAdriftRebornGameServer.Game.Entities
{
    internal class EntityManager
    {
        private static long nextEntityId = 1;
        private static readonly object entityIdLock = new object();

        public static long NextEntityId
        {
            get
            {
                lock (entityIdLock)
                {
                    return ++nextEntityId;
                }
            }
        }

        public static List<PlayerEntities> playerEntities = new List<PlayerEntities>();

        public static List<IslandEntities> islandEntities = new List<IslandEntities>();
    }
}
