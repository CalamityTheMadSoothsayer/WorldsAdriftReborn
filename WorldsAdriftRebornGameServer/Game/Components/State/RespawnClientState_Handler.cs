using Bossa.Travellers.Controls;
using Bossa.Travellers.Player;
using Improbable;
using Improbable.Corelibrary.Math;
using Improbable.Corelibrary.Transforms;
using Improbable.Corelibrary.Transforms.Teleport;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game.Entity;
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
            var entity = EntityManager.GlobalEntityRealm[entityId];
            foreach (var r in clientComponentUpdate.respawnAtRandomAncientRespawner)
            {
                Console.WriteLine($"Requested RANDOM ANCIENT Respawn, last biome:" + r.lastValidBiomeId);
            }
            foreach (var r in clientComponentUpdate.respawnAtNearestAncientRespawner)
            {
                Console.WriteLine($"Requested NEAREST ANCIENT Respawn, last biome: " + r.lastValidBiomeId);
                var respawnUpdate = entity.Get<RespawnState>().Value.ToUpdate().Get();
                respawnUpdate.SetWaitingRespawn(false);
                respawnUpdate.SetWaitingForRespawnDecision(false);
                respawnUpdate.SetWaitingRespawnParent(new EntityId(-1));
                var teleportUpdate = entity.Get<TransformState>().Value.ToUpdate().Get();
                // teleportUpdate.SetParent(new Parent(r.targetEntityId, "Ancient Respawner"));
                teleportUpdate.SetParent(null);
                teleportUpdate.SetLocalPosition(new FixedPointVector3(EntityManager.GlobalEntityRealm[Island.IslandSpawners[0].FirstRespawner.Id].Position));
                
                entity.Update(respawnUpdate);
                entity.Update(teleportUpdate);

                SendOPHelper.SendComponentUpdateOp(player, entityId,
                    new System.Collections.Generic.List<uint> { 1092, 190602 },
                    new System.Collections.Generic.List<object> { respawnUpdate , teleportUpdate });
            }
            foreach (var r in clientComponentUpdate.respawnDone)
            {
                // This happens when the player connects and needs to spawn but also when the player receives respawn demand from the server
                Console.WriteLine($"Respawn READY, target:" + r.targetEntityId);
                var respawnUpdate = entity.Get<RespawnState>().Value.ToUpdate().Get();
                respawnUpdate.SetWaitingRespawn(false);
                respawnUpdate.SetWaitingRespawnParent(new EntityId(-1));
                var teleportUpdate = entity.Get<TransformState>().Value.ToUpdate().Get();
                // teleportUpdate.SetParent(new Parent(r.targetEntityId, "Ancient Respawner"));
                teleportUpdate.SetParent(null);
                teleportUpdate.SetLocalPosition(new FixedPointVector3(entity.Position));
                
                entity.Update(respawnUpdate);
                // entity.Update(teleportUpdate);

                SendOPHelper.SendComponentUpdateOp(player, entityId,
                    new System.Collections.Generic.List<uint> { 1092 },
                    new System.Collections.Generic.List<object> { respawnUpdate }); // , teleportUpdate });
            }
            foreach (var r in clientComponentUpdate.respawnAtPersonalReviver)
            {
                Console.WriteLine($"Requested PERSONAL REVIVER Respawn, last biome:" + r.lastValidBiomeId);
            }
            foreach (var r in clientComponentUpdate.getReviverShipInfoRequest)
            {
                Console.WriteLine($"Requested PERSONAL REVIVER INFO");
            }

            
            entity.Update(clientComponentUpdate);
            
            // SendOPHelper.SendComponentUpdateOp(player, entityId, new System.Collections.Generic.List<uint> { ComponentId }, new System.Collections.Generic.List<object> { serverComponentUpdate });
        }
    }
}
