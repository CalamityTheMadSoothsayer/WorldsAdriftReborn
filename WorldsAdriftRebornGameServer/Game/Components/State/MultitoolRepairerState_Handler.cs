using Bossa.Travellers.Interact;
using Bossa.Travellers.Items;
using Bossa.Travellers.Salvaging;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game.Components.Data;
using WorldsAdriftRebornGameServer.Game.Items;
using WorldsAdriftRebornGameServer.Networking.Wrapper;

namespace WorldsAdriftRebornGameServer.Game.Components.State
{
    [ComponentStateHandler]
    internal class MultitoolRepairerStateHandler : IComponentStateHandler<MultitoolRepairerState,
        MultitoolRepairerState.Update, MultitoolRepairerState.Data>
    {
        public override uint ComponentId => 2002;
        
        public override void HandleUpdate( ENetPeerHandle player, long entityId, MultitoolRepairerState.Update clientComponentUpdate, MultitoolRepairerState.Data serverComponentData)
        {
            clientComponentUpdate.ApplyTo(serverComponentData);
            MultitoolRepairerState.Update serverComponentUpdate = (MultitoolRepairerState.Update)serverComponentData.ToUpdate();
            
            // Console.WriteLine("IsOn: " + serverComponentUpdate.isOn.Value);
            // Console.WriteLine("IsEngaged: " + serverComponentUpdate.isEngaged.Value);
            
            SendOPHelper.SendComponentUpdateOp(player, entityId, new System.Collections.Generic.List<uint> { ComponentId }, new System.Collections.Generic.List<object> { serverComponentUpdate });
        }
    }
}
