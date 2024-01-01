using Improbable.Worker;

namespace WorldsAdriftRebornGameServer.Game.Entity
{
    public class CoreEntity : Improbable.Worker.Entity
    {
        public long Id { get; private set; }
        public readonly Dictionary<uint, IComponentData<IComponentMetaclass>> Components = new Dictionary<uint, IComponentData<IComponentMetaclass>>();

        public virtual void Awake(long entityId)
        {
            Id = entityId;
            EntityManager.GlobalEntityRealm[entityId] = this;
        }

        public virtual Player ToPlayer() => null;
    }
}
