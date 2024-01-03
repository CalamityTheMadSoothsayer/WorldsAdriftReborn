using Improbable;

namespace WorldsAdriftRebornGameServer.Game.Entity
{
    public class AncientRespawner : CoreEntity
    {
        public static readonly Improbable.Collections.List<EntityId> ARlist = new Improbable.Collections.List<EntityId>();

        public const string DisplayName = "Revival Chamber";

        public override void Awake()
        {
            base.Awake();
            ARlist.Add(new EntityId(Id));
        }
    }
}
