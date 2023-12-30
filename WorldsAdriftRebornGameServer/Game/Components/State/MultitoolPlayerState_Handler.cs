using Bossa.Travellers.Items;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Networking.Wrapper;

namespace WorldsAdriftRebornGameServer.Game.Components.State
{
    [ComponentStateHandler]
    internal class MultiToolPlayerStateHandler : IComponentStateHandler<MultiToolPlayerState,
        MultiToolPlayerState.Update, MultiToolPlayerState.Data>
    {

        public override uint ComponentId => 2105;
        
        public override void HandleUpdate( ENetPeerHandle player, long entityId, MultiToolPlayerState.Update clientComponentUpdate, MultiToolPlayerState.Data serverComponentData)
        {
            clientComponentUpdate.ApplyTo(serverComponentData);
            MultiToolPlayerState.Update serverComponentUpdate = (MultiToolPlayerState.Update)serverComponentData.ToUpdate();

            for (var i = 0; i < serverComponentUpdate.shotEntityEvent.Count; i++)
            {
                Console.WriteLine("INFO - Game requests shotEntity");
                Console.WriteLine(serverComponentUpdate.shotEntityEvent[i].entityId);
                Console.WriteLine(serverComponentUpdate.shotEntityEvent[i].offset);
            }
            for (var i = 0; i < serverComponentUpdate.shotWorldEvent.Count; i++)
            {
                Console.WriteLine("INFO - Game requests shotWorld");
                Console.WriteLine(serverComponentUpdate.shotWorldEvent[i].shotCoordinates);
            }
            for (var i = 0; i < serverComponentUpdate.toggleModeEvent.Count; i++)
            {
                Console.WriteLine("INFO - Game requests toggleMode");
            }

            
            SendOPHelper.SendComponentUpdateOp(player, entityId, new System.Collections.Generic.List<uint> { ComponentId }, new System.Collections.Generic.List<object> { serverComponentUpdate });
        }
    }
}
