using WorldsAdriftRebornGameServer.DLLCommunication;

namespace WorldsAdriftRebornGameServer.Game.Entity
{
    public class Player : CoreEntity
    {
        public static readonly HashSet<Player> PlayerList = new HashSet<Player>();
        
        private ENetPeerHandle Client { get; set; }
        // public readonly Dictionary<uint, ulong> ClientComponents = new Dictionary<uint, ulong>();

        public override void Awake(long entityId)
        {
            PlayerList.Add(this);
            base.Awake(entityId);
            // TODO: Acquire Client object somehow
            return;
        }

        public override Player ToPlayer() => this;
    }
}
