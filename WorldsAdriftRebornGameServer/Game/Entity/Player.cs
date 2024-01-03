using WorldsAdriftRebornGameServer.DLLCommunication;

namespace WorldsAdriftRebornGameServer.Game.Entity
{
    public class Player : CoreEntity
    {
        public static readonly HashSet<long> PlayerList = new HashSet<long>();

        internal ENetPeerHandle Client { get; private set; }

        internal Player( ENetPeerHandle peer )
        {
            Client = peer;
        }

        public override Player ToPlayer() => this;

        public override void Awake()
        {
            base.Awake();
            PlayerList.Add(Id);
            Console.WriteLine($"Player {Id} added to PlayerList");
        }
    }
}
