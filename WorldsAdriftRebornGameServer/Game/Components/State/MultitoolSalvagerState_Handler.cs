using Bossa.Travellers.Interact;
using Bossa.Travellers.Items;
using Bossa.Travellers.Salvaging;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game.Components.Data;
using WorldsAdriftRebornGameServer.Game.Entity;
using WorldsAdriftRebornGameServer.Game.Items;
using WorldsAdriftRebornGameServer.Networking.Wrapper;

namespace WorldsAdriftRebornGameServer.Game.Components.State
{
    [ComponentStateHandler]
    internal class MultitoolSalvagerStateHandler : IComponentStateHandler<MultitoolSalvagerState,
        MultitoolSalvagerState.Update, MultitoolSalvagerState.Data>
    {

        public override uint ComponentId => 2106;
        
        public override void HandleUpdate( ENetPeerHandle player, long entityId, MultitoolSalvagerState.Update clientComponentUpdate, MultitoolSalvagerState.Data serverComponentData)
        {
            var entity = EntityManager.GlobalEntityRealm[entityId];
            clientComponentUpdate.ApplyTo(serverComponentData);
            MultitoolSalvagerState.Update serverComponentUpdate = (MultitoolSalvagerState.Update)serverComponentData.ToUpdate();

            for (var i = 0; i < clientComponentUpdate.deployedEvent.Count; i++)
            {
                Console.WriteLine("INFO - C - Game requests Salvager deploy");
                clientComponentUpdate.SetIsOn(true);
            }
            for (var i = 0; i < clientComponentUpdate.shotEvent.Count; i++)
            {
                Console.WriteLine("INFO - C - Game requests Salvager shoot");
            }
            for (var i = 0; i < serverComponentUpdate.deployedEvent.Count; i++)
            {
                Console.WriteLine("INFO - Game requests Salvager deploy");
                serverComponentUpdate.SetIsOn(true);
            }
            for (var i = 0; i < serverComponentUpdate.shotEvent.Count; i++)
            {
                Console.WriteLine("INFO - Game requests Salvager shoot");
            }

            // TODO: Server authoritative design. Currently we just trust the client lol
            // TODO: We shouldn't be doing this for every update, only when deployed, but for now this is fine.
            var serverStateData = entity.Get<InteractAgentServerState>().Value;
            if (serverStateData.Get().Value.multitoolMode != MultitoolMode.Salvage) 
            {
                Console.WriteLine("Switching multitool mode to Salvage");
                var serverState = serverStateData.ToUpdate().Get();
                serverState.SetMultitoolMode(MultitoolMode.Salvage);
                serverState.SetSelectedHotbarItem(ItemHelper.GetDefaultItems()[0]);

                serverState.ApplyTo(serverStateData);
                SendOPHelper.SendComponentUpdateOp(player, entityId, new System.Collections.Generic.List<uint> { ComponentId, 1212 }, new System.Collections.Generic.List<object> { serverComponentUpdate, serverState});
                return;
            }

            SendOPHelper.SendComponentUpdateOp(player, entityId, new System.Collections.Generic.List<uint> { ComponentId }, new System.Collections.Generic.List<object> { serverComponentUpdate});
        }
    }
}
