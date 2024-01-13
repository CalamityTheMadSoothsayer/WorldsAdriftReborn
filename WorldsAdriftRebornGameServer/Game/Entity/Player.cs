using Improbable;
using Improbable.Math;
using WorldsAdriftRebornGameServer.DLLCommunication;

namespace WorldsAdriftRebornGameServer.Game.Entity
{
    public class Player : CoreEntity
    {
        public static readonly HashSet<long> PlayerList = new HashSet<long>();

        // added for interactables
        public bool useItemKeyHeld;

        public EntityId lookingAt;

        public EntityId lookingAtInteractive;

        public EntityId debugLookingAt;

        public Vector3f lookDirectionEuler;

        public Coordinates lookHitPoint;

        public int itemSlot;

        public int selectedHotbar;

        
        // added for character comparison
        public long steamId;

        public string charName;

        public int head;

        public int body;

        public int feet;

        public int face;

        public int facialHair;

        public struct PlayerMiscData
        {
            public string Gender { get; }
            public bool SeenIntro { get; }
            public bool SkippedTutorial { get; }

            public PlayerMiscData( string gender, bool seenIntro, bool skippedTutorial )
            {
                Gender = gender;
                SeenIntro = seenIntro;
                SkippedTutorial = skippedTutorial;
            }
        }


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
