namespace WorldsAdriftRebornGameServer.Game.Entity
{
    public class CoreEntity : Improbable.Worker.Entity
    {
        private static long nextEntityId = 1;

        public static long GenerateNextId()
        {
            return nextEntityId++;
        }

        public long Id { get; private set; }

        public virtual void Awake()
        {
            Id = GenerateNextId();
            EntityManager.GlobalEntityRealm[Id] = this;
            Console.WriteLine($"Entity {Id} of type {GetType().Name} added to GlobalEntityRealm");
        }

        public virtual Player? ToPlayer() => null;
    }
}
