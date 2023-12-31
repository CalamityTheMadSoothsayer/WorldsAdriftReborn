using WorldsAdriftRebornGameServer.DLLCommunication;

namespace WorldsAdriftRebornGameServer.Game.Components.Data
{
    public class Player : CoreEntity
    {
        public static readonly HashSet<Player> PlayerList = new HashSet<Player>();

        private ENetPeerHandle Client { get; set; }

        public override void Awake()
        {
            PlayerList.Add(this);
            base.Awake();
            Console.WriteLine($"Player {Id} added to PlayerList");
            return;
        }

        public override Player? ToPlayer() => this;
    }
}
