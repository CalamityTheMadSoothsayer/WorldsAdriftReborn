using Bossa.Travellers.Craftingstation;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Networking.Wrapper;

namespace WorldsAdriftRebornGameServer.Game.Components.State
{
    [ComponentStateHandler]
    internal class PlayerCraftingInteractionStateHandler : IComponentStateHandler<PlayerCraftingInteractionState, PlayerCraftingInteractionState.Update, PlayerCraftingInteractionState.Data>
    {
        public override uint ComponentId => 1003;
        
        public override void HandleUpdate( ENetPeerHandle player, long entityId, PlayerCraftingInteractionState.Update clientComponentUpdate, PlayerCraftingInteractionState.Data serverComponentData)
        {
            clientComponentUpdate.ApplyTo(serverComponentData);
            PlayerCraftingInteractionState.Update serverComponentUpdate = (PlayerCraftingInteractionState.Update)serverComponentData.ToUpdate();

            

            SendOPHelper.SendComponentUpdateOp(player, entityId, new List<uint> { ComponentId }, new List<object> { serverComponentUpdate });
        }
    }
}
