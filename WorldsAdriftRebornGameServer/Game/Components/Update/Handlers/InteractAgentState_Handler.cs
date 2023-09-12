using Bossa.Travellers.Interact;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Networking.Wrapper;

namespace WorldsAdriftRebornGameServer.Game.Components.Update.Handlers
{
    [RegisterComponentUpdateHandler]
    internal class InteractAgentState_Handler : IComponentUpdateHandler<InteractAgentState, InteractAgentState.Update, InteractAgentState.Data>
    {
        public InteractAgentState_Handler() { Init(1211); }
        protected override void Init( uint ComponentId )
        {
            this.ComponentId = ComponentId;
        }

        public override void HandleUpdate( ENetPeerHandle player, long entityId,
            InteractAgentState.Update clientComponentUpdate, InteractAgentState.Data serverComponentData )
        {
            clientComponentUpdate.ApplyTo(serverComponentData);
            InteractAgentState.Update serverComponentUpdate = (InteractAgentState.Update)serverComponentData.ToUpdate();
            
            // TODO: Update PlayerMultitoolVisualizer states depending on the itemSlot
            
            for (int j = 0; j < clientComponentUpdate.useItemKeyPressed.Count; j++)
            {
                Console.WriteLine("[info] game use item");
                Console.WriteLine("[info] slot: " + clientComponentUpdate.useItemKeyPressed[j].itemSlot);
                Console.WriteLine("[info] slot: " + clientComponentUpdate.useItemKeyPressed[j].sourcePosition.X + " " + clientComponentUpdate.useItemKeyPressed[j].sourcePosition.Z);
            }

            for (int j = 0; j < clientComponentUpdate.releaseInteraction.Count; j++)
            {
                Console.WriteLine("[info] game release interact");
                Console.WriteLine("[info] entity: " + clientComponentUpdate.releaseInteraction[j].interactEntityId.Id);
            }

            for (int j = 0; j < clientComponentUpdate.interactWithObject.Count; j++)
            {
                Console.WriteLine("[info] game interact with object");
                Console.WriteLine("[info] entity: " + clientComponentUpdate.interactWithObject[j].target.Id);
                Console.WriteLine("[info] verb: " + clientComponentUpdate.interactWithObject[j].verb);
            }

            for (int j = 0; j < clientComponentUpdate.useItemKeyReleased.Count; j++)
            {
                Console.WriteLine("[info] key released");
                Console.WriteLine("[info] time: " + clientComponentUpdate.useItemKeyReleased[j].timeButtonHeld);
            }

            SendOPHelper.SendComponentUpdateOp(player, entityId, new List<uint> { ComponentId }, new List<object> { serverComponentUpdate });
        }
    }
}
