using Bossa.Travellers.Spawning;
using WorldsAdriftRebornGameServer.DLLCommunication;

namespace WorldsAdriftRebornGameServer.Game.Components
{
    internal abstract class IComponentStateHandler<T, H, C>
    {
        public abstract uint ComponentId { get; }
        // public abstract C Init(ENetPeerHandle player, long entityId);
        public abstract void HandleUpdate(ENetPeerHandle player, long entityId, H clientComponentUpdate, C serverComponentData);
    }
}
