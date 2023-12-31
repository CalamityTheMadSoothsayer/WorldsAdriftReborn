using Improbable.Worker;

namespace WorldsAdriftRebornGameServer.Game.Components.Data
{
    public class CoreEntity : Entity
    {

        private static long nextEntityId = 1;

        public static long GenerateNextId()
        {
            return nextEntityId++;
        }

        public long Id { get; private set; }
        public readonly Dictionary<uint, IComponentData<IComponentMetaclass>> Components = new Dictionary<uint, IComponentData<IComponentMetaclass>>();

        public virtual void Awake()
        {
            Id = GenerateNextId();
            EntityManager.GlobalEntityRealm[Id] = this;
            Console.WriteLine($"Entity {Id} of type {GetType().Name} added to GlobalEntityRealm");
        }

        public virtual Player? ToPlayer() => null;

    }
}
