using Bossa.Travellers.Salvaging;
using WorldsAdriftRebornGameServer.DLLCommunication;
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
            clientComponentUpdate.ApplyTo(serverComponentData);
            MultitoolSalvagerState.Update serverComponentUpdate = (MultitoolSalvagerState.Update)serverComponentData.ToUpdate();

            for (var i = 0; i < serverComponentUpdate.deployedEvent.Count; i++)
            {
                Console.WriteLine("INFO - Game requests Salvager deploy");
                serverComponentUpdate.SetIsOn(true);
            }
            for (var i = 0; i < serverComponentUpdate.shotEvent.Count; i++)
            {
                Console.WriteLine("INFO - Game requests Salvager shoot");
            }

            
            SendOPHelper.SendComponentUpdateOp(player, entityId, new System.Collections.Generic.List<uint> { ComponentId }, new System.Collections.Generic.List<object> { serverComponentUpdate });
        }
    }
}
