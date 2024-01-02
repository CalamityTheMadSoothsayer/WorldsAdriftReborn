using Bossa.Travellers.Controls;
using Bossa.Travellers.Player;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Networking.Wrapper;

namespace WorldsAdriftRebornGameServer.Game.Components.State
{
    [ComponentStateHandler]
    internal class RespawnClientStateHandler : IComponentStateHandler<RespawnClientState,
        RespawnClientState.Update, RespawnClientState.Data>
    {

        public override uint ComponentId => 1093;
        
        public override void HandleUpdate( ENetPeerHandle player, long entityId, RespawnClientState.Update clientComponentUpdate, RespawnClientState.Data serverComponentData)
        {
            foreach (var r in clientComponentUpdate.respawnAtRandomAncientRespawner)
            {
                Console.WriteLine($"Requested RANDOM ANCIENT Respawn, last biome:" + r.lastValidBiomeId);
            }
            foreach (var r in clientComponentUpdate.respawnAtNearestAncientRespawner)
            {
                Console.WriteLine($"Requested NEAREST ANCIENT Respawn, last biome: " + r.lastValidBiomeId);
            }
            foreach (var r in clientComponentUpdate.respawnDone)
            {
                Console.WriteLine($"Respawn FINISHED, target:" + r.targetEntityId);
            }
            foreach (var r in clientComponentUpdate.respawnAtPersonalReviver)
            {
                Console.WriteLine($"Requested PERSONAL REVIVER Respawn, last biome:" + r.lastValidBiomeId);
            }
            foreach (var r in clientComponentUpdate.getReviverShipInfoRequest)
            {
                Console.WriteLine($"Requested PERSONAL REVIVER INFO");
            }
            
            clientComponentUpdate.ApplyTo(serverComponentData);
            CharacterControlsData.Update serverComponentUpdate = (CharacterControlsData.Update)serverComponentData.ToUpdate();

            SendOPHelper.SendComponentUpdateOp(player, entityId, new System.Collections.Generic.List<uint> { ComponentId }, new System.Collections.Generic.List<object> { serverComponentUpdate });
        }
    }
}
